namespace Tracker.Modelsusers
{
    /// <summary>
    /// User
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Memmber name
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Joined date
        /// </summary>
        public DateTime JoinDate { get; set; }

        /// <summary>
        /// Image path
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// User identity.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User role
        /// </summary>
        public string UserRole { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Image content
        /// </summary>
        public string ImageContent { get; set; }

        /// <summary>
        /// Image
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// IS profile picture exist
        /// </summary>
        public bool IsProfilePictureExist { get; set; }

        /// <summary>
        /// Is Password set
        /// </summary>
        public bool IsPasswordSet { get; set; }

        /// <summary>
        /// Designation
        /// </summary>
        public string Designation { get; set; }

        /// <summary>
        /// Alternative email
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Alternativr phone number
        /// </summary>
        public string AlternativePhoneNumber { get; set; }

        /// <summary>
        /// Leave start date.
        /// </summary>
        public DateTime LeaveStartDate { get; set; }

        /// <summary>
        /// Leave end date.
        /// </summary>
        public DateTime LeaveEndDate { get; set; }

        /// <summary>
        /// Department identity.
        /// </summary>
        public int DepartmentId { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
    }

}
