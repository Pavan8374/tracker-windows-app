namespace NatWinTracker.Models.Users
{
    /// <summary>
    /// User project api response
    /// </summary>
    public class UserProjectAPIResponse
    {
        /// <summary>
        /// Identity
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string projectName { get; set; }

        /// <summary>
        /// Client identity.
        /// </summary>
        public int clientID { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public string duration { get; set; }

        /// <summary>
        /// Limit hours
        /// </summary>
        public string limitHours { get; set; }

        /// <summary>
        /// Is trackable
        /// </summary>
        public bool isTrackable { get; set; }

        /// <summary>
        /// Type identity 
        /// </summary>
        public int typeID { get; set; }

        /// <summary>
        /// Mange by member identity
        /// </summary>
        public int manageByMemberId { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int displayOrder { get; set; }
    }
}
