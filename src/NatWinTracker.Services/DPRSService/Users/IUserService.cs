using NatWinTracker.Models.DPRS;
using NatWinTracker.Models.Projects;
using NatWinTracker.Models.Users;

namespace NatWinTracker.Services.DPRSService.Users
{
    /// <summary>
    /// Userservice interface.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user projects.
        /// </summary>
        /// <returns>User project list.</returns>
        Task<List<UserProject>> GetUserProjects();

        /// <summary>
        /// Manage Dprs
        /// </summary>
        /// <param name="model">model</param>
        /// <returns>Tre/false</returns>
        Task<bool> ManageDPRS(AddDPRSRequestModel model);

        Task<List<ProjectDTO>> GetDPRSEntry();
    }
}
