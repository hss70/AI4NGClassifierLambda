using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AI4NGClassifierLambda.Models;
using System.Text.Json;

namespace AI4NGClassifierLambda.Services
{
    public interface IClassifierService
    {
        Task<List<Classifier>> GetAllClassifiersAsync(string userId = null);
        Task<Classifier?> GetClassifierByIdAsync(int classifierId);
        Task<Classifier?> GetClassifierBySessionIdAsync(int sessionId);
        Task<Classifier?> GetClassifierBySessionIdAsync(string sessionId);
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
            Console.WriteLine($"Using classifier table: {_classifierTable}");
            Console.WriteLine($"Using status table: {_statusTable}");
            _statusTable = Environment.GetEnvironmentVariable("STATUS_TABLE") ?? "EEGProcessingStatus";
        }

        public async Task<List<Classifier>> GetAllClassifiersAsync(string userId = null)
        {
            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable
            };

            // Temporarily remove userId filter to debug
            // if (!string.IsNullOrEmpty(userId))
            // {
            //     scanRequest.FilterExpression = "userId = :userId";
            //     scanRequest.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            //     {
            //         { ":userId", new AttributeValue { S = userId } }
            //     };
            // }

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
                KeyConditionExpression = "classifierId = :classifierId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":classifierId", new AttributeValue { S = classifierId.ToString() } }
                },
                ScanIndexForward = false,
                Limit = 1
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);
            
            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        public async Task<Classifier?> GetClassifierBySessionIdAsync(string sessionId)
        {
            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable,
                FilterExpression = "sessionName = :sessionName",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionName", new AttributeValue { S = sessionId } }
                },
                Limit = 1
            };

            var response = await _dynamoDb.ScanAsync(scanRequest);
            
            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        public async Task<Classifier?> GetClassifierBySessionIdAsync(int sessionId)
        {
            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable,
                FilterExpression = "sessionId = :sessionId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { S = sessionId.ToString() } }
                },
                Limit = 1
            };

            var response = await _dynamoDb.ScanAsync(scanRequest);
            
            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        private Classifier? MapToClassifier(Dictionary<string, AttributeValue> item)
        {
            try
            {
                // Parse classifierId and sessionId from the data
                var classifierIdStr = item.ContainsKey("classifierId") && item["classifierId"].S != null ? item["classifierId"].S : "0";
                var sessionIdStr = item.ContainsKey("sessionId") && item["sessionId"].S != null ? item["sessionId"].S : "0";
                var sessionName = item.ContainsKey("sessionName") && item["sessionName"].S != null ? item["sessionName"].S : "Unknown";
                var timestampStr = item.ContainsKey("timestamp") && item["timestamp"].S != null ? item["timestamp"].S : "0";
                
                if (!int.TryParse(classifierIdStr, out var classifierId))
                    int.TryParse(classifierIdStr.Substring(0, Math.Min(9, classifierIdStr.Length)), out classifierId);
                    
                if (!int.TryParse(sessionIdStr, out var sessionId))
                    int.TryParse(sessionIdStr.Substring(0, Math.Min(9, sessionIdStr.Length)), out sessionId);
                    
                if (!long.TryParse(timestampStr, out var timestamp))
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                else
                    timestamp = timestamp / 1000; // Convert to seconds if needed

                return new Classifier
                {
                    ClassifierId = classifierId,
                    SessionId = sessionId,
                    SessionName = sessionName,
                    Status = "Ready",
                    UploadDate = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
                    LastUpdated = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
                    PeakAccuracy = 0.0,
                    ErrorMargin = 0.0,
                    Parameters = ExtractParameters(item),
                    Graphs = ExtractGraphs(item)
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
            if (!item.ContainsKey("cf") || item["cf"].S == null)
                return null;

            try
            {
                var cfData = item["cf"].S;
                var jsonData = JsonSerializer.Deserialize<JsonElement>(cfData);
                
                if (jsonData.TryGetProperty("param", out var param) && 
                    param.TryGetProperty("M", out var paramM))
                {
                    var a0 = 0f;
                    if (paramM.TryGetProperty("a0", out var a0Prop) && 
                        a0Prop.TryGetProperty("N", out var a0N) &&
                        float.TryParse(a0N.GetString(), out var a0Value))
                    {
                        a0 = a0Value;
                    }
                    
                    var a1Array = new List<float>();
                    if (paramM.TryGetProperty("a1N", out var a1Prop) && 
                        a1Prop.TryGetProperty("L", out var a1L))
                    {
                        foreach (var arrayElement in a1L.EnumerateArray())
                        {
                            if (arrayElement.TryGetProperty("N", out var nValue) &&
                                float.TryParse(nValue.GetString(), out var floatValue))
                            {
                                a1Array.Add(floatValue);
                            }
                        }
                    }
                    
                    return new Parameters
                    {
                        A0 = a0,
                        A1 = a1Array.ToArray(),
                        FullCfJson = cfData
                    };
                }
                return new Parameters
                {
                    A0 = 0f,
                    A1 = new float[0],
                    FullCfJson = cfData
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting parameters: {ex.Message}");
                return new Parameters
                {
                    A0 = 0f,
                    A1 = new float[0],
                    FullCfJson = item.ContainsKey("cf") ? item["cf"].S : null
                };
            }
        }

        private List<Graph> ExtractGraphs(Dictionary<string, AttributeValue> item)
        {
            var graphs = new List<Graph>();
            
            // Add classifier file
            if (item.ContainsKey("fileName") && item["fileName"].S != null)
            {
                graphs.Add(new Graph { Name = "Classifier", Data = item["fileName"].S });
            }
            
            // Add DA plot based on s3Key path structure
            if (item.ContainsKey("s3Key") && item["s3Key"].S != null)
            {
                var s3Key = item["s3Key"].S;
                // Extract path parts: hss70/S8R11_28_May_12_07_NoFeedback_meta (1)/Online/...
                var pathParts = s3Key.Split('/');
                if (pathParts.Length >= 2)
                {
                    var userId = pathParts[0];
                    var sessionName = pathParts[1];
                    var daPlotPath = $"{userId}/{sessionName}/Offline/{userId}/{sessionName}/DA.png";
                    graphs.Add(new Graph { Name = "DA Plot", Data = daPlotPath });
                }
            }
            
            return graphs;
        }
    }
}