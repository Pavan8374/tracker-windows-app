namespace NSTracker.Utility
{
    public class ActivityEntry
    {
        public string Time { get; set; }
        public int MouseClicks { get; set; }
        public int KeyboardClicks { get; set; }
    }

    public class ActivityLog
    {
        public string WorkedDate { get; set; }
        public List<ActivityEntry> Activity { get; set; } = new List<ActivityEntry>();
    }
}
