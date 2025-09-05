using FluentValidation;
using MotoRental.Application.DTOs;

namespace MotoRental.Application.Validators
{
    public class MotorcycleCreateValidator : AbstractValidator<MotorcycleCreateDTO>
    {
        public MotorcycleCreateValidator()
        {
            RuleFor(x => x.Year)
                .NotEmpty().WithMessage("Ano é obrigatório")
                .InclusiveBetween(1900, DateTime.Now.Year + 1).WithMessage("Ano inválido");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Modelo é obrigatório")
                .MaximumLength(100).WithMessage("Modelo deve ter no máximo 100 caracteres");

            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("Placa é obrigatória")
                .Length(7).WithMessage("Placa deve ter 7 caracteres")
                .Matches(@"^[A-Z]{3}\d{4}$|^[A-Z]{3}\d{1}[A-Z]{1}\d{2}$").WithMessage("Formato de placa inválido");
        }
    }
}