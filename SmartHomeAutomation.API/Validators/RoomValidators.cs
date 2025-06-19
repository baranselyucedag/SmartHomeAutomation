using FluentValidation;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Validators
{
    public class CreateRoomDtoValidator : AbstractValidator<CreateRoomDto>
    {
        public CreateRoomDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Oda adı boş olamaz.")
                .MaximumLength(100).WithMessage("Oda adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
        }
    }

    public class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDto>
    {
        public UpdateRoomDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Oda adı boş olamaz.")
                .MaximumLength(100).WithMessage("Oda adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
        }
    }
} 