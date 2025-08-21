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
        public async Task<IActionResult> GetAllClassifiers()
        {
            var classifiers = await _classifierService.GetAllClassifiersAsync();
            return Ok(classifiers);
        }

        [HttpGet("{classifierId}")]
        [ProducesResponseType(typeof(Classifier), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassifierById(int classifierId)
        {
            var classifier = await _classifierService.GetClassifierByIdAsync(classifierId);
            if (classifier == null)
                return NotFound();

            return Ok(classifier);
        }

        [HttpGet("session/{sessionId}")]
        [ProducesResponseType(typeof(Classifier), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClassifierBySessionId(int sessionId)
        {
            var classifier = await _classifierService.GetClassifierBySessionIdAsync(sessionId);
            if (classifier == null)
                return NotFound();

            return Ok(classifier);
        }

        [HttpGet("{classifierId}/graphs")]
        [ProducesResponseType(typeof(IEnumerable<Graph>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGraphsForClassifier(int classifierId)
        {
            var classifier = await _classifierService.GetClassifierByIdAsync(classifierId);
            if (classifier == null)
                return NotFound();

            return Ok(classifier.Graphs);
        }

        [HttpGet("{classifierId}/graphs/{graphName}")]
        [ProducesResponseType(typeof(Graph), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGraphByName(int classifierId, string graphName)
        {
            var classifier = await _classifierService.GetClassifierByIdAsync(classifierId);
            if (classifier == null)
                return NotFound();

            var graph = classifier.Graphs?.FirstOrDefault(g => g.Name.Equals(graphName, StringComparison.OrdinalIgnoreCase));
            if (graph == null)
                return NotFound();

            return Ok(graph);
        }
    }
}
