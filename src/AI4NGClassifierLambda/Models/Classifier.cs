namespace AI4NGClassifierLambda.Models
{
    public class Classifier
    {
        public int ClassifierId { get; set; }
        public string Status { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public int SessionId { get; set; }
        public required string SessionName { get; set; }
        public Parameters? Parameters { get; set; }
        public List<Graph>? Graphs { get; set; }
    }

    public class Parameters
    {
        public float A0 { get; set; }
        public float[] A1 { get; set; }
    }

    public class Graph
    {
        public string Name { get; set; }
        // Add additional fields from the `Graph` schema if needed
    }
}