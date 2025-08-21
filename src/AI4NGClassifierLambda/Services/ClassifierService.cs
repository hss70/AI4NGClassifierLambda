using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AI4NGClassifierLambda.Models;
using System.Text.Json;

namespace AI4NGClassifierLambda.Services
{
    public interface IClassifierService
    {
        Task<List<Classifier>> GetAllClassifiersAsync();
        Task<Classifier?> GetClassifierByIdAsync(int classifierId);
        Task<Classifier?> GetClassifierBySessionIdAsync(int sessionId);
    }

    public class ClassifierService : IClassifierService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _classifierTable;
        private readonly string _statusTable;

        public ClassifierService(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
        {
            _dynamoDb = dynamoDb;
            _classifierTable = Environment.GetEnvironmentVariable("CLASSIFIER_TABLE") ?? "FBCSPClassifierParameters";
            _statusTable = Environment.GetEnvironmentVariable("STATUS_TABLE") ?? "EEGProcessingStatus";
        }

        public async Task<List<Classifier>> GetAllClassifiersAsync()
        {
            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable
            };

            var response = await _dynamoDb.ScanAsync(scanRequest);
            Console.WriteLine($"Found {response.Items.Count} items in table {_classifierTable}");
            
            var classifiers = new List<Classifier>();

            foreach (var item in response.Items)
            {
                Console.WriteLine($"Processing item: {JsonSerializer.Serialize(item)}");
                var classifier = MapToClassifier(item);
                if (classifier != null)
                    classifiers.Add(classifier);
                else
                    Console.WriteLine("Failed to map classifier");
            }

            return classifiers;
        }

        public async Task<Classifier?> GetClassifierByIdAsync(int classifierId)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _classifierTable,
                KeyConditionExpression = "sessionId = :sessionId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { S = classifierId.ToString() } }
                },
                ScanIndexForward = false,
                Limit = 1
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);
            
            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        public async Task<Classifier?> GetClassifierBySessionIdAsync(int sessionId)
        {
            var queryRequest = new QueryRequest
            {
                TableName = _classifierTable,
                IndexName = "SessionIdIndex",
                KeyConditionExpression = "sessionId = :sessionId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionId.ToString() } }
                },
                ScanIndexForward = false,
                Limit = 1
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);
            
            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        private Classifier? MapToClassifier(Dictionary<string, AttributeValue> item)
        {
            try
            {
                return new Classifier
                {
                    ClassifierId = item["sessionId"].S.GetHashCode(),
                    SessionId = item["sessionId"].S.GetHashCode(),
                    SessionName = item["sessionId"].S,
                    Status = "Ready",
                    UploadDate = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item["timestamp"].N)).DateTime,
                    LastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(item["timestamp"].N)).DateTime,
                    PeakAccuracy = 0.0,
                    ErrorMargin = 0.0,
                    Parameters = ExtractParameters(item),
                    Graphs = new List<Graph>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mapping classifier: {ex.Message}");
                return null;
            }
        }

        private Parameters? ExtractParameters(Dictionary<string, AttributeValue> item)
        {
            if (!item.ContainsKey("cf"))
                return null;

            try
            {
                var cfData = item["cf"].S;
                var jsonData = JsonSerializer.Deserialize<JsonElement>(cfData);
                
                if (jsonData.TryGetProperty("param", out var param) && 
                    param.TryGetProperty("M", out var paramM))
                {
                    var a0 = paramM.TryGetProperty("a0", out var a0Prop) && 
                             a0Prop.TryGetProperty("N", out var a0N) ? 
                             float.Parse(a0N.GetString()) : 0f;
                    
                    var a1Array = new List<float>();
                    if (paramM.TryGetProperty("a1N", out var a1Prop) && 
                        a1Prop.TryGetProperty("L", out var a1L))
                    {
                        foreach (var item2 in a1L.EnumerateArray())
                        {
                            if (item2.TryGetProperty("N", out var nValue))
                                a1Array.Add(float.Parse(nValue.GetString()));
                        }
                    }
                    
                    return new Parameters
                    {
                        A0 = a0,
                        A1 = a1Array.ToArray()
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting parameters: {ex.Message}");
                return null;
            }
        }
    }
}