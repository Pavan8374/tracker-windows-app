namespace Tracker.Models.Users
{
    /// <summary>
    /// User project
    /// </summary>
    public class UserProject
    {
        /// <summary>
        /// Identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Client identity.
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Limit hours
        /// </summary>
        public string LimitHours { get; set; }

        /// <summary>
        /// Is trackable.
        /// </summary>
        public bool IsTrackable { get; set; }

        /// <summary>
        /// Type identity.
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Manage by member identity.
        /// </summary>
        public int ManageByMemberId { get; set; }

        /// <summary>
        /// Display order.
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
