using Microsoft.AspNetCore.Mvc;
using AI4NGClassifierLambda.Models;
using AI4NGClassifierLambda.Services;
using AI4NGClassifierLambda.Interfaces;


namespace AI4NGClassifierLambda.Controllers
{
    [ApiController]
    [Route("api/classifiers")]
    public class ClassifiersController : ControllerBase
    {
        private readonly IClassifierService _classifierService;

        public ClassifiersController(IClassifierService classifierService)
        {
            _classifierService = classifierService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Classifier>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllClassifiers()
        {
            try
            {
                Console.WriteLine("Endpoint: GET /api/classifiers");
                var userId = User.FindFirst("username")?.Value;
                var classifiers = await _classifierService.GetAllClassifiersAsync(userId);
                return Ok(classifiers);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "GET /api/classifiers");
            }
        }

        [HttpGet("{classifierId}")]
        [ProducesResponseType(typeof(Classifier), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClassifierById(long classifierId)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/{classifierId}");
                var userId = User.FindFirst("username")?.Value;
                var classifier = await _classifierService.GetClassifierByIdAsync(userId, classifierId);
                if (classifier == null)
                    return NotFound();

                return Ok(classifier);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/{classifierId}");
            }
        }

        [HttpGet("session/{sessionId:long}")]
        [ProducesResponseType(typeof(Classifier), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClassifierBySessionId(long sessionId)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/session/{sessionId}");
                var userId = User.FindFirst("username")?.Value;
                var classifier = await _classifierService.GetClassifierBySessionIdAsync(userId, sessionId);
                if (classifier == null)
                    return NotFound();

                return Ok(classifier);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/session/{sessionId}");
            }
        }



        [HttpGet("session/{sessionId}/graphs")]
        [ProducesResponseType(typeof(IEnumerable<Graph>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphsForClassifierBySession(string sessionId)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/session/{sessionId}/graphs");
                var userId = User.FindFirst("username")?.Value;
                var graphs = await _classifierService.GetGraphsForClassifierBySessionAsync(userId, sessionId);
                return Ok(graphs);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/session/{sessionId}/graphs");
            }
        }

        [HttpGet("session/{sessionId}/graphdata")]
        [ProducesResponseType(typeof(IEnumerable<GraphData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphDataForClassifierBySession(string sessionId)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/session/{sessionId}/graphdata");
                var userId = User.FindFirst("username")?.Value;
                var graphData = await _classifierService.GetGraphDataForClassifierBySessionAsync(userId, sessionId);
                return Ok(graphData);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/session/{sessionId}/graphdata");
            }
        }

        [HttpGet("session/{sessionId}/graphnames")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphNamesForClassifierBySession(string sessionId)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/session/{sessionId}/graphnames");
                var userId = User.FindFirst("username")?.Value;
                var graphNames = await _classifierService.GetGraphNamesForClassifierBySessionAsync(userId, sessionId);
                return Ok(graphNames);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/session/{sessionId}/graphnames");
            }
        }

        [HttpGet("session/{sessionId}/graphs/{graphName}")]
        [ProducesResponseType(typeof(Graph), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphByNameForSession(string sessionId, string graphName)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/session/{sessionId}/graphs/{graphName}");
                var userId = User.FindFirst("username")?.Value;
                var graph = await _classifierService.GetGraphByNameForClassifierBySessionAsync(userId, sessionId, graphName);
                if (graph == null)
                    return NotFound();

                return Ok(graph);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/session/{sessionId}/graphs/{graphName}");
            }
        }

        [HttpGet("session/{sessionId}/graphdata/{graphName}")]
        [ProducesResponseType(typeof(GraphData), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphDataByNameForSession(string sessionId, string graphName)
        {
            try
            {
                Console.WriteLine($"Endpoint: GET /api/classifiers/session/{sessionId}/graphdata/{graphName}");
                var userId = User.FindFirst("username")?.Value;
                var graphData = await _classifierService.GetGraphDataByNameForClassifierBySessionAsync(userId, sessionId, graphName);
                if (graphData == null)
                    return NotFound();

                return Ok(graphData);
            }
            catch (Exception ex)
            {
                return HandleException(ex, $"GET /api/classifiers/session/{sessionId}/graphdata/{graphName}");
            }
        }
        private IActionResult HandleException(Exception ex, string endpoint)
        {
            Console.WriteLine($"Error in {endpoint}: {ex.Message}");
            return ex switch
            {
                ArgumentException => BadRequest(ex.Message),
                UnauthorizedAccessException => Forbid(),
                Amazon.DynamoDBv2.AmazonDynamoDBException => StatusCode(503, "Database temporarily unavailable"),
                Amazon.S3.AmazonS3Exception => StatusCode(503, "Storage temporarily unavailable"),
                TimeoutException => StatusCode(408, "Request timeout"),
                _ => throw ex
            };
        }
    }
}
