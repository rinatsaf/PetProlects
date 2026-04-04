using Application.DTOs.Sessions;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<Session, SessionDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateSessionRequest, Session>()
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

        CreateMap<UpdateSessionRequest, Session>()
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}
