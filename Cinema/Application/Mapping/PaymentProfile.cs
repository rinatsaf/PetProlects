using Application.DTOs.Payments;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
