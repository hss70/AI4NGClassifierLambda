using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AI4NGClassifierLambda.Models;
using System.Text.Json;

namespace AI4NGClassifierLambda.Services
{
    public interface IClassifierService
    {
        Task<List<Classifier>> GetAllClassifiersAsync(string userId = null);
        Task<Classifier?> GetClassifierByIdAsync(long classifierId);
        Task<Classifier?> GetClassifierBySessionIdAsync(long sessionId);
        Task<Classifier?> GetClassifierBySessionIdAsync(string sessionId);
    }

    public class ClassifierService : IClassifierService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _classifierTable;

        public ClassifierService(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
        {
            _dynamoDb = dynamoDb;
            _classifierTable = Environment.GetEnvironmentVariable("CLASSIFIER_TABLE") ?? "FBCSPClassifierParameters";
            Console.WriteLine($"Using classifier table: {_classifierTable}");
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

        public async Task<Classifier?> GetClassifierByIdAsync(long classifierId)
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

        public async Task<Classifier?> GetClassifierBySessionIdAsync(long sessionId)
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
                // classifierId
                var classifierIdStr = item.ContainsKey("classifierId") && item["classifierId"].N != null
                    ? item["classifierId"].N
                    : "0";

                // sessionId
                var sessionIdStr = item.ContainsKey("sessionId") && item["sessionId"].N != null
                    ? item["sessionId"].N
                    : "0";

                // sessionName
                var sessionName = item.ContainsKey("sessionName") && item["sessionName"].S != null
                    ? item["sessionName"].S
                    : "Unknown";

                // timestamp
                var timestampStr = item.ContainsKey("timestamp") && item["timestamp"].N != null
                    ? item["timestamp"].N
                    : "0";

                if (!long.TryParse(classifierIdStr, out var classifierId))
                    classifierId = 0;

                if (!long.TryParse(sessionIdStr, out var sessionId))
                    sessionId = 0;

                if (!long.TryParse(timestampStr, out var timestamp))
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // Dynamo stores ms, convert to seconds
                var timestampSecs = timestamp / 1000;

                return new Classifier
                {
                    ClassifierId = classifierId,
                    SessionId = sessionId,
                    SessionName = sessionName,
                    Status = "Ready",
                    UploadDate = DateTimeOffset.FromUnixTimeSeconds(timestampSecs).DateTime,
                    LastUpdated = DateTimeOffset.FromUnixTimeSeconds(timestampSecs).DateTime,
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
            if (!item.ContainsKey("cf") || item["cf"].M == null)
            {
                Console.WriteLine("No cf field found in item");
                return null;
            }

            try
            {
                var cfMap = item["cf"].M;

                if (!cfMap.ContainsKey("param") || cfMap["param"].M == null)
                    return null;

                var paramMap = cfMap["param"].M;

                // Extract a0 (single number)
                float a0 = 0f;
                if (paramMap.ContainsKey("a0") && paramMap["a0"].N != null &&
                    float.TryParse(paramMap["a0"].N, out var a0Val))
                {
                    a0 = a0Val;
                }

                // Extract a1N (list of numbers)
                var a1Array = new List<float>();
                if (paramMap.ContainsKey("a1N") && paramMap["a1N"].L != null)
                {
                    foreach (var element in paramMap["a1N"].L)
                    {
                        if (element.N != null && float.TryParse(element.N, out var val))
                            a1Array.Add(val);
                    }
                }

                return new Parameters
                {
                    A0 = a0,
                    A1 = a1Array.ToArray(),
                    // if you want the raw cf JSON as a string:
                    FullCfJson = System.Text.Json.JsonSerializer.Serialize(cfMap)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting parameters: {ex.Message}");
                return new Parameters
                {
                    A0 = 0f,
                    A1 = Array.Empty<float>(),
                    FullCfJson = null
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