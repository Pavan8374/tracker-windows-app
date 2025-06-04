namespace Tracker.Models.ActivityEvents
{
    public class ActivityEvent
    {
        public DateTime DateTime { get; set; }
        public string FromTimeToTime { get; set; }
        public int MouseClicks { get; set; }
        public int KeyboardClicks { get; set; }
    }

    public class ActivityTrackingData
    {
        public List<ActivityEvent> ActivityEventsJson { get; set; } = new List<ActivityEvent>();
    }
}
