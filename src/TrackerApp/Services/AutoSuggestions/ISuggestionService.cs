namespace NSTracker.Services.AutoSuggestions
{
    /// <summary>
    /// Suggestion service interfcae
    /// </summary>
    public interface ISuggestionService
    {
        public void Start();
        public void Stop();
        public void TrackingNotStarted(bool isTracking);
    }
}
