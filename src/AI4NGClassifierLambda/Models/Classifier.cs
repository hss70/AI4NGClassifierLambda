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

        public T1ResultTable? ResultsTable { get; set; }
    }

    public class T1ResultTable
    {
        [System.Text.Json.Serialization.JsonPropertyName("ssr_code")]
        public string SsrCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("classes")]
        public int Classes { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("CVfolds")]
        public int CVfolds { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("allTrials")]
        public int AllTrials { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("trPerClass")]
        public int[] TrPerClass { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("testTrials")]
        public int TestTrials { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("testTrPerClass")]
        public int[] TestTrPerClass { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("trainTrials")]
        public int TrainTrials { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("trainTrPerClass")]
        public int[] TrainTrPerClass { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("cfWin_ms")]
        public int CfWinMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("cfStep_ms")]
        public int CfStepMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smoothWidth_ms")]
        public int SmoothWidthMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("opt_tt_ID")]
        public int OptTtId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("trigPoint_ms")]
        public int TrigPointMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refInterval_ms")]
        public int[] RefIntervalMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("taskInterval_ms")]
        public int[] TaskIntervalMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refPeak_ms")]
        public int RefPeakMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("taskPeak_ms")]
        public int TaskPeakMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_refPeak_ms")]
        public int SmoothRefPeakMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_taskPeak_ms")]
        public int SmoothTaskPeakMs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refPeakDA_mean")]
        public double RefPeakDA_Mean { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refPeakDA_std")]
        public double RefPeakDA_Std { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("taskPeakDA_mean")]
        public double TaskPeakDA_Mean { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("taskPeakDA_std")]
        public double TaskPeakDA_Std { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_refPeakDA_mean")]
        public double SmoothRefPeakDA_Mean { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_refPeakDA_std")]
        public double SmoothRefPeakDA_Std { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_taskPeakDA_mean")]
        public double SmoothTaskPeakDA_Mean { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_taskPeakDA_std")]
        public double SmoothTaskPeakDA_Std { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refPeakDA_lower_taskPeakDA_ttest_p")]
        public double RefPeakDA_Lower_TaskPeakDA_Ttest_P { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refPeakDA_lower_taskPeakDA_wilc_p")]
        public double RefPeakDA_Lower_TaskPeakDA_Wilc_P { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_refPeakDA_lower_taskPeakDA_ttest_p")]
        public double SmoothRefPeakDA_Lower_TaskPeakDA_Ttest_P { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("smooth_refPeakDA_lower_taskPeakDA_wilc_p")]
        public double SmoothRefPeakDA_Lower_TaskPeakDA_Wilc_P { get; set; }
    }

    /// <summary>
    /// Parameters for the classifier
    /// </summary>
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

    public class GraphData
    {
        public string Name { get; set; }
        public object Data { get; set; } // JSON object for graph data
    }
}