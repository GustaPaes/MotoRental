using FluentValidation;
using MotoRental.Application.DTOs;

namespace MotoRental.Application.Validators
{
    public class RentalCreateValidator : AbstractValidator<RentalCreateDTO>
    {
        public RentalCreateValidator()
        {
            RuleFor(x => x.DeliveryPersonId)
                .NotEmpty().WithMessage("ID do entregador é obrigatório");

            RuleFor(x => x.MotorcycleId)
                .NotEmpty().WithMessage("ID da moto é obrigatório");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Data de início é obrigatória")
                .GreaterThan(DateTime.Now.Date).WithMessage("Data de início deve ser futura");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Data de término é obrigatória")
                .GreaterThan(x => x.StartDate).WithMessage("Data de término deve ser posterior à data de início");

            RuleFor(x => x.Plan)
                .NotEmpty().WithMessage("Plano é obrigatório")
                .Must(BeValidPlan).WithMessage("Plano inválido");
        }

        private bool BeValidPlan(int plan)
        {
            return new[] { 7, 15, 30, 45, 50 }.Contains(plan);
        }
    }
}