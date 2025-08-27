# Classifier API Documentation

**Base URL:** `https://{api-id}.execute-api.eu-west-2.amazonaws.com/dev`

## Authentication
All endpoints require JWT token in Authorization header:
```
Authorization: Bearer {jwt-token}
```

## Endpoints

### GET /api/classifiers
Get all classifiers
- **Response:** Array of Classifier objects
- **Status:** 200 OK

### GET /api/classifiers/{classifierId}
Get specific classifier by ID
- **Parameters:** `classifierId` (integer) - The classifier ID
- **Response:** Classifier object
- **Status:** 200 OK, 404 Not Found

### GET /api/classifiers/session/{sessionId}
Get classifier by session ID
- **Parameters:** `sessionId` (string) - The session ID
- **Response:** Classifier object
- **Status:** 200 OK, 404 Not Found

### GET /api/classifiers/session/{sessionId}/graphs
Get all graphs for a classifier by session ID
- **Parameters:** `sessionId` (string) - The session ID
- **Response:** Array of Graph objects
- **Status:** 200 OK, 400 Bad Request

### GET /api/classifiers/session/{sessionId}/graphdata
Get all graph data for a classifier by session ID
- **Parameters:** `sessionId` (string) - The session ID
- **Response:** Array of GraphData objects
- **Status:** 200 OK, 400 Bad Request

### GET /api/classifiers/session/{sessionId}/graphnames
Get all graph names for a classifier by session ID
- **Parameters:** `sessionId` (string) - The session ID
- **Response:** Array of strings
- **Status:** 200 OK, 400 Bad Request

### GET /api/classifiers/session/{sessionId}/graphs/{graphName}
Get specific graph by name for session
- **Parameters:** 
  - `sessionId` (string) - The session ID
  - `graphName` (string) - The graph name
- **Response:** Graph object
- **Status:** 200 OK, 404 Not Found, 400 Bad Request

### GET /api/classifiers/session/{sessionId}/graphdata/{graphName}
Get specific graph data by name for session
- **Parameters:** 
  - `sessionId` (string) - The session ID
  - `graphName` (string) - The graph name
- **Response:** GraphData object
- **Status:** 200 OK, 404 Not Found, 400 Bad Request

## Response Models

### Classifier
```json
{
  "classifierId": 123,
  "status": "Ready",
  "uploadDate": "2024-01-15T10:30:00Z",
  "lastUpdated": "2024-01-15T10:30:00Z",
  "sessionId": 456,
  "sessionName": "Session 456",
  "peakAccuracy": 92.1,
  "errorMargin": 12.1,
  "parameters": {
    "a0": 0.5,
    "a1": [0.1, 0.2, 0.3, 0.12],
    "fullCfJson": "{\"param\":{\"a0\":0.5,\"a1N\":[0.1,0.2,0.3]}}"
  },
  "graphs": [
    {"name": "DA plot (smoothed).png"},
    {"name": "heatmap (Freq v4).png"}
  ]
}
```

### Graph
```json
{
  "name": "DA plot (smoothed).png",
  "data": "base64-encoded-image-data"
}
```

### GraphData
```json
{
  "name": "DA plot (smoothed).json",
  "data": {
    "x": [1, 2, 3],
    "y": [0.1, 0.2, 0.3]
  }
}
```

## Postman Collection Setup
1. Create environment variable `baseUrl` = `https://{your-api-id}.execute-api.eu-west-2.amazonaws.com/dev`
2. Add Authorization header with Bearer token to collection
3. Use `{{baseUrl}}` in request URLs

## DynamoDB Dependencies

### Tables Used
- **FBCSPClassifierParameters**: Stores classifier data and parameters
- **FBCSPSessionFiles**: Stores file metadata for sessions

### Summary
| Endpoint | Table | Index | Purpose |
|----------|-------|-------|---------|
| GET /api/classifiers | FBCSPClassifierParameters | UserIdTimestampIndex | Get all classifiers for user |
| GET /api/classifiers/{id} | FBCSPClassifierParameters | Primary Key | Get classifier by ID |
| GET /api/classifiers/session/{sessionId} | FBCSPClassifierParameters | SessionIdTimestampIndex | Get classifier by session |
| GET /api/classifiers/session/{sessionId}/graphs | FBCSPSessionFiles | SessionIdExtensionIndex | Get PNG files for session |
| GET /api/classifiers/session/{sessionId}/graphdata | FBCSPSessionFiles | SessionIdExtensionIndex | Get JSON files for session |
| GET /api/classifiers/session/{sessionId}/graphnames | FBCSPSessionFiles | SessionIdExtensionIndex | Get PNG file names |
| GET /api/classifiers/session/{sessionId}/graphs/{name} | FBCSPSessionFiles | SessionIdExtensionIndex | Get specific PNG file |
| GET /api/classifiers/session/{sessionId}/graphdata/{name} | FBCSPSessionFiles | SessionIdExtensionIndex | Get specific JSON file |

### Index Details

#### FBCSPClassifierParameters
- **Primary Key**: classifierId (N)
- **UserIdTimestampIndex**: userId (HASH), timestamp (RANGE)
- **SessionIdTimestampIndex**: sessionId (HASH), timestamp (RANGE)

#### FBCSPSessionFiles
- **Primary Key**: sessionName (HASH), filePath (RANGE)
- **SessionIdExtensionIndex**: sessionId (HASH), extension (RANGE)
- **UserIdCreatedAtIndex**: userId (HASH), createdAt (RANGE)