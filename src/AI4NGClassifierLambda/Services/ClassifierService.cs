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
    }

    public class ClassifierService : IClassifierService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _classifierTable;
        private readonly string _statusTable;

        public ClassifierService(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
        {
            _dynamoDb = dynamoDb;
            _classifierTable = configuration["CLASSIFIER_TABLE"] ?? "FBCSPClassifierParameters";
            _statusTable = configuration["STATUS_TABLE"] ?? "EEGProcessingStatus";
        }

        public async Task<List<Classifier>> GetAllClassifiersAsync()
        {
            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable
            };

            var response = await _dynamoDb.ScanAsync(scanRequest);
            var classifiers = new List<Classifier>();

            foreach (var item in response.Items)
            {
                var classifier = MapToClassifier(item);
                if (classifier != null)
                    classifiers.Add(classifier);
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

        private Classifier? MapToClassifier(Dictionary<string, AttributeValue> item)
        {
            try
            {
                return new Classifier
                {
                    ClassifierId = int.Parse(item["sessionId"].S),
                    SessionId = int.Parse(item["sessionId"].S),
                    SessionName = item.ContainsKey("sessionName") ? item["sessionName"].S : $"Session {item["sessionId"].S}",
                    Status = "Ready",
                    UploadDate = DateTime.FromBinary(long.Parse(item["timestamp"].N)),
                    LastUpdated = DateTime.FromBinary(long.Parse(item["timestamp"].N)),
                    PeakAccuracy = 0.0,
                    ErrorMargin = 0.0,
                    Parameters = ExtractParameters(item),
                    Graphs = new List<Graph>()
                };
            }
            catch
            {
                return null;
            }
        }

        private Parameters? ExtractParameters(Dictionary<string, AttributeValue> item)
        {
            if (!item.ContainsKey("classifierData"))
                return null;

            try
            {
                var jsonData = JsonSerializer.Deserialize<JsonElement>(item["classifierData"].S);
                return new Parameters
                {
                    A0 = jsonData.TryGetProperty("a0", out var a0) ? a0.GetSingle() : 0f,
                    A1 = jsonData.TryGetProperty("a1", out var a1) && a1.ValueKind == JsonValueKind.Array 
                        ? a1.EnumerateArray().Select(x => x.GetSingle()).ToArray() 
                        : new float[0]
                };
            }
            catch
            {
                return null;
            }
        }
    }
}