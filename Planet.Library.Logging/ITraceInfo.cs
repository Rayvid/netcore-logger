namespace Planet.Library.Logging
{
    /// <summary>
    /// Interface to extract loggable info
    /// </summary>
    public interface ITraceInfo
    {
        /// <summary>
        /// For frequent messages
        /// </summary>
        /// <returns>Single line!</returns>
        string GetLogSingleLiner();
    }
}
