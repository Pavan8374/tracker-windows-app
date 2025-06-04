using AutoMapper;
using Tracker.Models.Auth;
using Tracker.Models.DPRS;
using Tracker.Models.Projects;
using Tracker.Models.Users;
using Tracker.Services.DPRSService.Auth;

namespace Tracker.Services
{
    /// <summary>
    /// Natwin tarcker mapper profile
    /// </summary>
    public class TrackerMapperProfile : Profile
    {
        public TrackerMapperProfile()
        {
            CreateAuthMaps();
            CreateUserMaps();
            CreateDPRSMaps();
        }
        public void CreateAuthMaps()
        {
            CreateMap<AuthTokenAPIResponse, TokenResponse>()
                .ForMember(x => x.AccessToken, o => o.MapFrom(x => x.access_token))
                .ForMember(x => x.RefreshToken, o => o.MapFrom(x => x.refresh_token))
                .ForMember(x => x.TokenType, o => o.MapFrom(x => x.token_type))
                .ForMember(x => x.Expires, o => o.MapFrom(x => x.expires))
                .ForMember(x => x.ExpiresIn, o => o.MapFrom(x => x.expires_in))
                .ForMember(x => x.Refresh, o => o.MapFrom(x => x.refresh))
                .ForMember(x => x.Issued, o => o.MapFrom(x => x.issued))
                .ForMember(x => x.UserName, o => o.MapFrom(x => x.userName));

            CreateMap<LoginResponseModel, Login>()
                .ForMember(x => x.Token, o => o.MapFrom(x => x.token))
                .ForMember(x => x.TokenType, o => o.MapFrom(x => x.tokenType))
                .ForMember(x => x.UserId, o => o.MapFrom(x => x.userId))
                .ForMember(x => x.UserName, o => o.MapFrom(x => x.userName))
                .ForMember(x => x.MemberId, o => o.MapFrom(x => x.memberId))
                .ForMember(x => x.Expiration, o => o.MapFrom(x => x.expiration))
                .ForMember(x => x.Role, o => o.MapFrom(x => x.role))
                ;
        }

        public void CreateUserMaps()
        {
            CreateMap<UserProjectAPIResponse, UserProject>()
                .ForMember(x => x.Id, o => o.MapFrom(x => x.id))
                .ForMember(x => x.Code, o => o.MapFrom(x => x.code))
                .ForMember(x => x.ProjectName, o => o.MapFrom(x => x.projectName))
                .ForMember(x => x.ClientId, o => o.MapFrom(x => x.clientID))
                .ForMember(x => x.Duration, o => o.MapFrom(x => x.duration))
                .ForMember(x => x.LimitHours, o => o.MapFrom(x => x.limitHours))
                .ForMember(x => x.IsTrackable, o => o.MapFrom(x => x.isTrackable))
                .ForMember(x => x.TypeId, o => o.MapFrom(x => x.typeID))
                .ForMember(x => x.ManageByMemberId, o => o.MapFrom(x => x.manageByMemberId))
                .ForMember(x => x.DisplayOrder, o => o.MapFrom(x => x.displayOrder))
                ;
        }


        public void CreateDPRSMaps()
        {
            CreateMap<WorkEntry, ProjectDTO>()
                .ForMember(x => x.MemberId, o => o.MapFrom(x => x.MemberID))
                .ForMember(x => x.Id, o => o.MapFrom(x => x.Id))
                .ForMember(x => x.ProjectName, o => o.MapFrom(x => x.ProjectName))
                //.ForMember(x => x.WorkedHours, o => o.MapFrom(x => x.WorkedHours))
                ;
        }


    }
}
