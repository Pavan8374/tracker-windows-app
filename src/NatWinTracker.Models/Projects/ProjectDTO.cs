namespace NatWinTracker.Models.Projects
{
    /// <summary>
    /// Project Data transfer object.
    /// </summary>
    public class ProjectDTO
    {
        /// <summary>
        /// Project identity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Project name.
        /// </summary>
        public string ProjectName{ get; set; }

        /// <summary>
        /// Member identity.
        /// </summary>
        public int MemberId{ get; set; }

        /// <summary>
        /// Worked hours
        /// </summary>
        public string WorkedHours{ get; set; }
    }
}
