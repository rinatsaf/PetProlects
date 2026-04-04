using Application.DTOs.Orders;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;

public sealed class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateOrderRequest, Order>()
            .ForMember(d => d.Status, opt => opt.MapFrom(_ => Domain.Enums.OrderStatus.Pending))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(d => d.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}
