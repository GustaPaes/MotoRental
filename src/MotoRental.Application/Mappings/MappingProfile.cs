using AutoMapper;
using MotoRental.Application.DTOs;
using MotoRental.Domain.Entities;

namespace MotoRental.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DeliveryPerson
            CreateMap<DeliveryPersonCreateDTO, DeliveryPerson>();
            CreateMap<DeliveryPerson, DeliveryPersonResponseDTO>();

            // Motorcycle
            CreateMap<MotorcycleCreateDTO, Motorcycle>();
            CreateMap<Motorcycle, MotorcycleResponseDTO>();

            // Rental
            CreateMap<RentalCreateDTO, Rental>();
            CreateMap<Rental, RentalResponseDTO>();
        }
    }
}