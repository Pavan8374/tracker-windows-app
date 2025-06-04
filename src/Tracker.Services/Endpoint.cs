namespace Tracker.Services
{
    /// <summary>
    /// Endpoint
    /// </summary>
    public static class Endpoint
    {
        public const string BaseUrl = "http://api.dprs.natrixsoftware.com/api/";
        //public const string BaseUrl = "http://10.0.0.223/api/";
        //public const string BaseUrl = "http://localhost:8081/api/";


        //Auth
        public const string Token = BaseUrl + "token";

        public const string Auth = BaseUrl + "Auth/";


        public const string Login = Auth + "login";

        //Projects
        public const string Project = BaseUrl + "Project/";

        //UserProjects 
        public const string UserProjects = Project + "GetProjectsByMemberIDAndStatus?";

        //User details
        public const string UserDetails = Project + "GetMemberDetailsByMemberId?";

        //Active members
        public const string ActiveMembers = Project + "GetActiveMembers";

        //User Profile pic
        public const string ProfileImage = "http://api.dprs.natrixsoftware.com/ProfilePicture/";

        //Manage DPRS
        public const string ManageDPRS = Project + "ManageDPRS";

        //Get DPRS entry
        public const string GetDPRSByQuery = Project + "GetDashboardProjectSummeryDetails?";


    }
}
