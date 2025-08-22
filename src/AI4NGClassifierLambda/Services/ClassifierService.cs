using System.Text.Json;
using AI4NGClassifierLambda.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AI4NGClassifierLambda.Services
{
    public interface IClassifierService
    {
        /// <summary>
        /// Get all classifiers for a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Classifier>> GetAllClassifiersAsync(string userId);
        /// <summary>
        /// Get a classifier by its ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="classifierId"></param>
        /// <returns></returns>
        Task<Classifier?> GetClassifierByIdAsync(string userId, long classifierId);
        /// <summary>
        /// Get a classifier by its session ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<Classifier?> GetClassifierBySessionIdAsync(string userId, long sessionId);
        /// <summary>
        /// Get the graphs for a classifier by its session ID
        /// Brings back the DA plot (smoothed).png, DA plot.png, and heatmap (Freq v4).png as base64 strings
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<List<Graph>> GetGraphsForClassifierBySessionAsync(string userId, string sessionId);
        /// <summary>
        /// Get the graph data for a classifier by its session ID
        /// Brings back the DA plot (smoothed).json, DA plot.json, and heatmap (Freq v4).json data so we can render in the frontend
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<List<GraphData>> GetGraphDataForClassifierBySessionAsync(string userId, string sessionId);
        /// <summary>
        /// Get the graph names for a classifier by its session ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<List<string>> GetGraphNamesForClassifierBySessionAsync(string userId, string sessionId);
        /// <summary>
        ///  Get a specific graph by name for a classifier by its session ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        Task<Graph> GetGraphByNameForClassifierBySessionAsync(string userId, string sessionId, string graphName);
        /// <summary>
        /// Get a graph data for a classifier by its name
        /// Brings back the graph data as a JSON object
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        Task<GraphData> GetGraphDataByNameForClassifierBySessionAsync(string userId, string sessionId, string graphName);
    }

    public class ClassifierService : IClassifierService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly string _classifierTable;
        private readonly string _fileTable;
        private static readonly string[] TargetGraphs = ["DA plot (smoothed).png", "DA plot.png", "heatmap (Freq v4).png"];
        private static readonly string[] TargetGraphsData = ["DA plot (smoothed).json", "DA plot.json", "heatmap (Freq v4).json"];

        public ClassifierService(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
        {
            _dynamoDb = dynamoDb;
            _classifierTable = Environment.GetEnvironmentVariable("CLASSIFIER_TABLE") ?? "FBCSPClassifierParameters";
            Console.WriteLine($"Using classifier table: {_classifierTable}");
            _fileTable = Environment.GetEnvironmentVariable("FILE_TABLE") ?? "FBCSPSessionFiles";
            Console.WriteLine($"Using file table: {_fileTable}");
        }

        public async Task<List<Classifier>> GetAllClassifiersAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));

            var queryRequest = new QueryRequest
            {
                TableName = _classifierTable,
                IndexName = "UserIdIndex",
                KeyConditionExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);
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

        public async Task<Classifier?> GetClassifierByIdAsync(string userId, long classifierId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (classifierId <= 0)
                throw new ArgumentException("ClassifierId must be greater than 0", nameof(classifierId));

            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable,
                FilterExpression = "classifierId = :classifierId AND userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":classifierId", new AttributeValue { N = classifierId.ToString() } },
                    { ":userId", new AttributeValue { S = userId } }
                },
                Limit = 1
            };

            var response = await _dynamoDb.ScanAsync(scanRequest);

            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        public async Task<Classifier?> GetClassifierBySessionIdAsync(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("SessionId is required", nameof(sessionId));

            var scanRequest = new ScanRequest
            {
                TableName = _classifierTable,
                FilterExpression = "sessionName = :sessionName AND userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionName", new AttributeValue { S = sessionId } },
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await _dynamoDb.ScanAsync(scanRequest);

            if (response.Items.Count == 0)
                return null;

            // Sort by timestamp descending to get latest
            var sortedItems = response.Items
                .OrderByDescending(item =>
                {
                    if (item.ContainsKey("timestamp") && item["timestamp"].N != null &&
                        long.TryParse(item["timestamp"].N, out var timestamp))
                        return timestamp;
                    return 0;
                })
                .ToList();

            return MapToClassifier(sortedItems[0]);
        }

        public async Task<Classifier?> GetClassifierBySessionIdAsync(string userId, long sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (sessionId <= 0)
                throw new ArgumentException("SessionId must be greater than 0", nameof(sessionId));

            var queryRequest = new QueryRequest
            {
                TableName = _classifierTable,
                IndexName = "SessionIdTimestampIndex",
                KeyConditionExpression = "sessionId = :sessionId",
                FilterExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionId.ToString() } },
                    { ":userId", new AttributeValue { S = userId } }
                },
                ScanIndexForward = false,
                Limit = 1
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);

            if (response.Items.Count == 0)
                return null;

            return MapToClassifier(response.Items[0]);
        }

        public async Task<List<Graph>> GetGraphsForClassifierBySessionAsync(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("SessionId is required", nameof(sessionId));

            var graphs = new List<Graph>();

            // Convert sessionId to long for DynamoDB query
            if (!long.TryParse(sessionId, out var sessionIdLong))
                throw new ArgumentException("SessionId must be a valid number", nameof(sessionId));

            // Query files by sessionId and png extension, filter by userId
            var queryRequest = new QueryRequest
            {
                TableName = _fileTable,
                IndexName = "SessionIdExtensionIndex",
                KeyConditionExpression = "sessionId = :sessionId AND extension = :extension",
                FilterExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionIdLong.ToString() } },
                    { ":extension", new AttributeValue { S = "png" } },
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);

            // Filter for target files and fetch from S3
            foreach (var item in response.Items)
            {
                var fileName = item.ContainsKey("fileName") && item["fileName"].S != null ? item["fileName"].S : "";
                var filePath = item.ContainsKey("filePath") && item["filePath"].S != null ? item["filePath"].S : "";

                if (TargetGraphs.Contains(fileName) && !string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        var fileData = await GetFileFromS3(filePath);
                        graphs.Add(new Graph { Name = fileName, Data = fileData });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching file {fileName}: {ex.Message}");
                    }
                }
            }

            return graphs;
        }

        public async Task<List<GraphData>> GetGraphDataForClassifierBySessionAsync(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("SessionId is required", nameof(sessionId));

            var graphsData = new List<GraphData>();

            // Convert sessionId to long for DynamoDB query
            if (!long.TryParse(sessionId, out var sessionIdLong))
                throw new ArgumentException("SessionId must be a valid number", nameof(sessionId));

            // Query files by sessionId and json extension, filter by userId
            var queryRequest = new QueryRequest
            {
                TableName = _fileTable,
                IndexName = "SessionIdExtensionIndex",
                KeyConditionExpression = "sessionId = :sessionId AND extension = :extension",
                FilterExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionIdLong.ToString() } },
                    { ":extension", new AttributeValue { S = "json" } },
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);

            // Filter for target files and fetch from S3
            foreach (var item in response.Items)
            {
                var fileName = item.ContainsKey("fileName") && item["fileName"].S != null ? item["fileName"].S : "";
                var filePath = item.ContainsKey("filePath") && item["filePath"].S != null ? item["filePath"].S : "";

                if (TargetGraphsData.Contains(fileName) && !string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        var jsonData = await GetJsonFromS3(filePath);
                        graphsData.Add(new GraphData { Name = fileName, Data = jsonData });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching file {fileName}: {ex.Message}");
                    }
                }
            }

            return graphsData;
        }

        public async Task<List<string>> GetGraphNamesForClassifierBySessionAsync(string userId, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("SessionId is required", nameof(sessionId));

            var graphNames = new List<string>();

            // Convert sessionId to long for DynamoDB query
            if (!long.TryParse(sessionId, out var sessionIdLong))
                throw new ArgumentException("SessionId must be a valid number", nameof(sessionId));

            // Query files by sessionId, filter by userId
            var queryRequest = new QueryRequest
            {
                TableName = _fileTable,
                IndexName = "SessionIdIndex",
                KeyConditionExpression = "sessionId = :sessionId",
                FilterExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionIdLong.ToString() } },
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);

            // Extract file names
            foreach (var item in response.Items)
            {
                var fileName = item.ContainsKey("fileName") && item["fileName"].S != null ? item["fileName"].S : "";
                
                if (!string.IsNullOrEmpty(fileName))
                {
                    graphNames.Add(fileName);
                }
            }

            return graphNames.Distinct().ToList();
        }

        public async Task<Graph> GetGraphByNameForClassifierBySessionAsync(string userId, string sessionId, string graphName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("SessionId is required", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(graphName))
                throw new ArgumentException("GraphName is required", nameof(graphName));

            if (!long.TryParse(sessionId, out var sessionIdLong))
                throw new ArgumentException("SessionId must be a valid number", nameof(sessionId));

            var queryRequest = new QueryRequest
            {
                TableName = _fileTable,
                IndexName = "SessionIdFileNameIndex",
                KeyConditionExpression = "sessionId = :sessionId AND fileName = :fileName",
                FilterExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionIdLong.ToString() } },
                    { ":fileName", new AttributeValue { S = graphName } },
                    { ":userId", new AttributeValue { S = userId } }
                },
                ScanIndexForward = false,
                Limit = 1
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);

            if (response.Items.Count == 0)
                throw new FileNotFoundException($"Graph {graphName} not found for session {sessionId}");

            var item = response.Items[0];
            var filePath = item.ContainsKey("filePath") && item["filePath"].S != null ? item["filePath"].S : "";

            if (string.IsNullOrEmpty(filePath))
                throw new InvalidOperationException("File path is empty");

            var fileData = await GetFileFromS3(filePath);
            return new Graph { Name = graphName, Data = fileData };
        }

        public async Task<GraphData> GetGraphDataByNameForClassifierBySessionAsync(string userId, string sessionId, string graphName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required", nameof(userId));
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("SessionId is required", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(graphName))
                throw new ArgumentException("GraphName is required", nameof(graphName));

            if (!long.TryParse(sessionId, out var sessionIdLong))
                throw new ArgumentException("SessionId must be a valid number", nameof(sessionId));

            var queryRequest = new QueryRequest
            {
                TableName = _fileTable,
                IndexName = "SessionIdFileNameIndex",
                KeyConditionExpression = "sessionId = :sessionId AND fileName = :fileName",
                FilterExpression = "userId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":sessionId", new AttributeValue { N = sessionIdLong.ToString() } },
                    { ":fileName", new AttributeValue { S = graphName } },
                    { ":userId", new AttributeValue { S = userId } }
                },
                ScanIndexForward = false,
                Limit = 1
            };

            var response = await _dynamoDb.QueryAsync(queryRequest);

            if (response.Items.Count == 0)
                throw new FileNotFoundException($"Graph data {graphName} not found for session {sessionId}");

            var item = response.Items[0];
            var filePath = item.ContainsKey("filePath") && item["filePath"].S != null ? item["filePath"].S : "";

            if (string.IsNullOrEmpty(filePath))
                throw new InvalidOperationException("File path is empty");

            var jsonData = await GetJsonFromS3(filePath);
            return new GraphData { Name = graphName, Data = jsonData };
        }

        private async Task<string> GetFileFromS3(string filePath)
        {
            var s3Client = new Amazon.S3.AmazonS3Client();
            var resultsBucket = Environment.GetEnvironmentVariable("RESULTS_BUCKET") ?? "ai4ng-eeg-results";

            var request = new Amazon.S3.Model.GetObjectRequest
            {
                BucketName = resultsBucket,
                Key = filePath
            };

            using var response = await s3Client.GetObjectAsync(request);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private async Task<string> GetJsonFromS3(string filePath)
        {
            var s3Client = new Amazon.S3.AmazonS3Client();
            var resultsBucket = Environment.GetEnvironmentVariable("RESULTS_BUCKET") ?? "ai4ng-eeg-results";

            var request = new Amazon.S3.Model.GetObjectRequest
            {
                BucketName = resultsBucket,
                Key = filePath
            };

            using var response = await s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
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
                    Parameters = ExtractParameters(item)
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
    }
}