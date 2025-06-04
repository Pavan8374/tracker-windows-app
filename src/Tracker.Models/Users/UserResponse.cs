namespace Tracker.Modelsusers
{
    /// <summary>
    /// User response
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// Identity
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Member name
        /// </summary>
        public string memberName { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string firstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string lastName { get; set; }

        /// <summary>
        /// Joined date
        /// </summary>
        public DateTime? joinDate { get; set; }

        /// <summary>
        /// Image path
        /// </summary>
        public string imagePath { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool status { get; set; }

        /// <summary>
        /// User identity.
        /// </summary>
        public string userID { get; set; }

        /// <summary>
        /// User role
        /// </summary>
        public string userRole { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Phone number.
        /// </summary>
        public string phoneNumber { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// Image content
        /// </summary>
        public string imageContent { get; set; }

        /// <summary>
        /// Image
        /// </summary>
        public string image { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// IIs profile picture exist
        /// </summary>
        public bool isProfilePictureExist { get; set; }

        /// <summary>
        /// Is password set.
        /// </summary>
        public bool isPasswordSet { get; set; }

        /// <summary>
        /// Designation.
        /// </summary>
        public string designation { get; set; }

        /// <summary>
        /// Alternative email.
        /// </summary>
        public string alternativeEmail { get; set; }

        /// <summary>
        /// Alternative phone number
        /// </summary>
        public string alternativePhoneNumber { get; set; }

        /// <summary>
        /// Leave start date
        /// </summary>
        public DateTime? leaveStartDate { get; set; }

        /// <summary>
        /// Leave end date.
        /// </summary>
        public DateTime? leaveEndDate { get; set; }

        /// <summary>
        /// Department identity.
        /// </summary>
        public int? departmentID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string name { get; set; }
    }

}
