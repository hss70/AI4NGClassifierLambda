using Microsoft.AspNetCore.Mvc;
using AI4NGClassifierLambda.Models;

namespace AI4NGClassifierLambda.Controllers
{
    [ApiController]
    [Route("api/classifiers")]
    public class ClassifiersController : ControllerBase
    {
        private static readonly List<Classifier> MockClassifiers = new()
        {
            new Classifier
            {
                ClassifierId = 1,
                Status = "Ready",
                UploadDate = DateTime.UtcNow.AddDays(-2),
                LastUpdated = DateTime.UtcNow,
                SessionId = 42,
                SessionName = "Test Session",
                PeakAccuracy = 92.1,
                ErrorMargin = 12.1,
                Parameters = new Parameters
                {
                    A0 = 0.5f,
                    A1 = new float[4] {0.1f, 0.2f, 0.3f, 0.12f}
                },
                Graphs = new List<Graph>
                {
                    new() { Name = "DaAccuracy" },
                    new() { Name = "HeatMap" }
                }
            }
        };

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Classifier>), StatusCodes.Status200OK)]
        public IActionResult GetAllClassifiers()
        {
            return Ok(MockClassifiers);
        }

        [HttpGet("{classifierId}")]
        [ProducesResponseType(typeof(Classifier), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetClassifierById(int classifierId)
        {
            var classifier = MockClassifiers.FirstOrDefault(c => c.ClassifierId == classifierId);
            if (classifier == null)
                return NotFound();

            return Ok(classifier);
        }

        [HttpGet("{classifierId}/graphs")]
        [ProducesResponseType(typeof(IEnumerable<Graph>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetGraphsForClassifier(int classifierId)
        {
            var classifier = MockClassifiers.FirstOrDefault(c => c.ClassifierId == classifierId);
            if (classifier == null)
                return NotFound();

            return Ok(classifier.Graphs);
        }

        [HttpGet("{classifierId}/graphs/{graphName}")]
        [ProducesResponseType(typeof(Graph), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetGraphByName(int classifierId, string graphName)
        {
            var classifier = MockClassifiers.FirstOrDefault(c => c.ClassifierId == classifierId);
            if (classifier == null)
                return NotFound();

            var graph = classifier.Graphs.FirstOrDefault(g => g.Name.Equals(graphName, StringComparison.OrdinalIgnoreCase));
            if (graph == null)
                return NotFound();

            return Ok(graph);
        }
    }
}
