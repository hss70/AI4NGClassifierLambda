using Microsoft.AspNetCore.Mvc;
using AI4NGClassifierLambda.Models;
using AI4NGClassifierLambda.Services;

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
                var userId = User.FindFirst("username")?.Value;
                var classifiers = await _classifierService.GetAllClassifiersAsync(userId);
                return Ok(classifiers);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                var userId = User.FindFirst("username")?.Value;
                var classifier = await _classifierService.GetClassifierByIdAsync(userId, classifierId);
                if (classifier == null)
                    return NotFound();

                return Ok(classifier);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                var userId = User.FindFirst("username")?.Value;
                var classifier = await _classifierService.GetClassifierBySessionIdAsync(userId, sessionId);
                if (classifier == null)
                    return NotFound();

                return Ok(classifier);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("session/{sessionId}/graphs")]
        [ProducesResponseType(typeof(IEnumerable<Graph>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphsForClassifierBySession(string sessionId)
        {
            try
            {
                var userId = User.FindFirst("username")?.Value;
                var graphs = await _classifierService.GetGraphsForClassifierBySessionAsync(userId, sessionId);
                return Ok(graphs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("session/{sessionId}/graphdata")]
        [ProducesResponseType(typeof(IEnumerable<GraphData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphDataForClassifierBySession(string sessionId)
        {
            try
            {
                var userId = User.FindFirst("username")?.Value;
                var graphData = await _classifierService.GetGraphDataForClassifierBySessionAsync(userId, sessionId);
                return Ok(graphData);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("session/{sessionId}/graphnames")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGraphNamesForClassifierBySession(string sessionId)
        {
            try
            {
                var userId = User.FindFirst("username")?.Value;
                var graphNames = await _classifierService.GetGraphNamesForClassifierBySessionAsync(userId, sessionId);
                return Ok(graphNames);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                var userId = User.FindFirst("username")?.Value;
                var graph = await _classifierService.GetGraphByNameForClassifierBySessionAsync(userId, sessionId, graphName);
                if (graph == null)
                    return NotFound();

                return Ok(graph);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
                var userId = User.FindFirst("username")?.Value;
                var graphData = await _classifierService.GetGraphDataByNameForClassifierBySessionAsync(userId, sessionId, graphName);
                if (graphData == null)
                    return NotFound();

                return Ok(graphData);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
