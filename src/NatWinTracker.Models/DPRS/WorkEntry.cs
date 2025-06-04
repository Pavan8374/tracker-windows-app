namespace NatWinTracker.Models.DPRS
{
    /// <summary>
    /// Work entry.
    /// </summary>
    public class WorkEntry
    {
        /// <summary>
        /// Profile Name
        /// </summary>
        public string? ProfileName { get; set; }

        /// <summary>
        /// Project Name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Limit Hours
        /// </summary>
        public string LimitHours { get; set; }

        /// <summary>
        /// Project identity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Worked Date
        /// </summary>
        public DateTime WorkedDate { get; set; }

        /// <summary>
        /// Worked Hours
        /// </summary>
        public string? WorkedHours { get; set; }

        /// <summary>
        /// SWorked Hours
        /// </summary>
        public string SWorkedHours { get; set; }

        /// <summary>
        /// STracked Hours
        /// </summary>
        public string? STrackedHours { get; set; }

        /// <summary>
        /// Tracked Hours
        /// </summary>
        public string? TrackedHours { get; set; }

        /// <summary>
        /// Member identity
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// Member name
        /// </summary>
        public string? MemberName { get; set; }

        /// <summary>
        /// Summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Is verified
        /// </summary>
        public bool? IsVerified { get; set; }

        /// <summary>
        /// Verified By User ientity
        /// </summary>
        public int? VerifiedByUserID { get; set; }

        /// <summary>
        /// Verified date
        /// </summary>
        public DateTime? VerifiedDate { get; set; }

        /// <summary>
        /// S management support hours
        /// </summary>
        public string? SManagementSupportHours { get; set; }

        /// <summary>
        /// Management support hours
        /// </summary>
        public string? ManagementSupportHours { get; set; }

        /// <summary>
        /// Meeting
        /// </summary>
        public string? Meeting { get; set; }

        /// <summary>
        /// InTime
        /// </summary>
        public string? InTime { get; set; }

        /// <summary>
        /// OutTime
        /// </summary>
        public string? OutTime { get; set; }
    }
}
