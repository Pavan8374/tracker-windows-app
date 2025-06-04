using System.ComponentModel;

namespace NatWinTracker.Models.DPRS
{
    /// <summary>
    /// Add dprs requestmodel.
    /// </summary>
    public class AddDPRSRequestModel
    {
        /// <summary>
        /// Identity
        /// Default value 0
        /// </summary>
        [DefaultValue(0)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// Member identity.
        /// </summary>
        public int MemberID { get; set; }

        /// <summary>
        /// Projectidentiy.
        /// </summary>
        public int ProjectID { get; set; }

        /// <summary>
        /// Worked date
        /// </summary>
        public DateTime WorkedDate { get; set; }

        /// <summary>
        /// _str worked date.
        /// </summary>
        public DateTime _strWorkedDate { get; set; }

        /// <summary>
        /// Worked hours
        /// </summary>
        [DefaultValue("00 : 00 : 00")]
        public string WorkedHours { get; set; } = "00 : 00 : 00";

        /// <summary>
        /// Tracked hours
        /// </summary>
        [DefaultValue("00 : 00 : 00")]
        public string TrackedHours { get; set; } = "00 : 00 : 00";

        /// <summary>
        /// Management suport hours.
        /// </summary>
        [DefaultValue("00 : 00 : 00")]
        public string ManagementSupportHours { get; set; } = "00 : 00 : 00";

        /// <summary>
        /// Summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Is need meeting
        /// </summary>
        [DefaultValue(false)]
        public bool IsNeedMeeting { get; set; } = false;

        /// <summary>
        /// Manager identity
        /// </summary>
        public int ManagerId { get; set; }

        /// <summary>
        /// Activity events json
        /// </summary>
        public string? KeyCountJSON { get; set; }
        public string? WorkMode { get; set; }
    }
}
