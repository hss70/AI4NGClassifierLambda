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
- **Parameters:** `sessionId` (integer) - The session ID
- **Response:** Classifier object
- **Status:** 200 OK, 404 Not Found

### GET /api/classifiers/{classifierId}/graphs
Get all graphs for a classifier
- **Parameters:** `classifierId` (integer) - The classifier ID
- **Response:** Array of Graph objects
- **Status:** 200 OK, 404 Not Found

### GET /api/classifiers/{classifierId}/graphs/{graphName}
Get specific graph by name
- **Parameters:** 
  - `classifierId` (integer) - The classifier ID
  - `graphName` (string) - The graph name
- **Response:** Graph object
- **Status:** 200 OK, 404 Not Found

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
    "a1": [0.1, 0.2, 0.3, 0.12]
  },
  "graphs": [
    {"name": "DaAccuracy"},
    {"name": "HeatMap"}
  ]
}
```

### Graph
```json
{
  "name": "DaAccuracy"
}
```

## Postman Collection Setup
1. Create environment variable `baseUrl` = `https://{your-api-id}.execute-api.eu-west-2.amazonaws.com/dev`
2. Add Authorization header with Bearer token to collection
3. Use `{{baseUrl}}` in request URLs