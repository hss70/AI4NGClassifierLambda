namespace AI4NGClassifierLambda.Interfaces
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
        Task<Graph?> GetGraphByNameForClassifierBySessionAsync(string userId, string sessionId, string graphName);
        /// <summary>
        /// Get a graph data for a classifier by its name
        /// Brings back the graph data as a JSON object
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="graphName"></param>
        /// <returns></returns>
        Task<GraphData?> GetGraphDataByNameForClassifierBySessionAsync(string userId, string sessionId, string graphName);
    }
}
