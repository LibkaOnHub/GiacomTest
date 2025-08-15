using AutoMapper;
using Order.WebAPI.DTOs;
using System;

namespace Order.WebAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateOrderRequest, Data.Entities.Order>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToByteArray()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.ResellerId, opt => opt.MapFrom(src => src.ResellerId.ToByteArray()))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId.ToByteArray()))
                .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.StatusId.ToByteArray()));

            CreateMap<CreateOrderItemRequest, Data.Entities.OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToByteArray()))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId.ToByteArray()))
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId.ToByteArray()))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));
        }
    }
}