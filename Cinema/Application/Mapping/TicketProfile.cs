using Application.DTOs.Tickets;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(t => t.Status.ToString()));
    }
}
