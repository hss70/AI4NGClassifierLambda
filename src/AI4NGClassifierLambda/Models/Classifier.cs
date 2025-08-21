namespace AI4NGClassifierLambda.Models
{
    public class Classifier
    {
        public long ClassifierId { get; set; }
        public string Status { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public long SessionId { get; set; }
        public required string SessionName { get; set; }
        public double PeakAccuracy { get; set; }
        public double ErrorMargin { get; set; }
        public Parameters? Parameters { get; set; }
        public List<Graph>? Graphs { get; set; }

    }

    public class Parameters
    {
        public float A0 { get; set; }
        public float[] A1 { get; set; }
        public string? FullCfJson { get; set; }
    }

    public class Graph
    {
        public string Name { get; set; }
        public string? Data { get; set; }
    }
}