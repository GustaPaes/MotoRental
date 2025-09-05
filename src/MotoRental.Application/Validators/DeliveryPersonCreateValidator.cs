using FluentValidation;
using MotoRental.Application.DTOs;

namespace MotoRental.Application.Validators
{
    public class DeliveryPersonCreateValidator : AbstractValidator<DeliveryPersonCreateDTO>
    {
        public DeliveryPersonCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.Cnpj)
                .NotEmpty().WithMessage("CNPJ é obrigatório")
                .Length(14).WithMessage("CNPJ deve ter 14 caracteres");

            RuleFor(x => x.CnhNumber)
                .NotEmpty().WithMessage("Número da CNH é obrigatório")
                .MaximumLength(11).WithMessage("CNH deve ter no máximo 11 caracteres");

            RuleFor(x => x.CnhType)
                .NotEmpty().WithMessage("Tipo da CNH é obrigatório")
                .Must(BeValidCnhType).WithMessage("Tipo de CNH inválido");
        }

        private bool BeValidCnhType(string cnhType)
        {
            return cnhType == "A" || cnhType == "B" || cnhType == "A+B";
        }
    }
}