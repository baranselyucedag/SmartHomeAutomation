using FluentValidation;
using SmartHomeAutomation.API.DTOs;

namespace SmartHomeAutomation.API.Validators
{
    public class CreateSceneDtoValidator : AbstractValidator<CreateSceneDto>
    {
        public CreateSceneDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Senaryo adı boş olamaz.")
                .MaximumLength(100).WithMessage("Senaryo adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

            RuleFor(x => x.SceneDevices)
                .NotNull().WithMessage("Senaryo en az bir cihaz içermelidir.");

            RuleForEach(x => x.SceneDevices)
                .SetValidator(new CreateSceneDeviceDtoValidator());
        }
    }

    public class UpdateSceneDtoValidator : AbstractValidator<UpdateSceneDto>
    {
        public UpdateSceneDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Senaryo adı boş olamaz.")
                .MaximumLength(100).WithMessage("Senaryo adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

            RuleFor(x => x.SceneDevices)
                .NotNull().WithMessage("Senaryo en az bir cihaz içermelidir.");

            RuleForEach(x => x.SceneDevices)
                .SetValidator(new UpdateSceneDeviceDtoValidator());
        }
    }

    public class CreateSceneDeviceDtoValidator : AbstractValidator<CreateSceneDeviceDto>
    {
        public CreateSceneDeviceDtoValidator()
        {
            RuleFor(x => x.DeviceId)
                .GreaterThan(0).WithMessage("Geçerli bir cihaz seçilmelidir.");

            RuleFor(x => x.TargetState)
                .NotEmpty().WithMessage("Hedef durum belirtilmelidir.");
        }
    }

    public class UpdateSceneDeviceDtoValidator : AbstractValidator<UpdateSceneDeviceDto>
    {
        public UpdateSceneDeviceDtoValidator()
        {
            RuleFor(x => x.DeviceId)
                .GreaterThan(0).WithMessage("Geçerli bir cihaz seçilmelidir.");

            RuleFor(x => x.TargetState)
                .NotEmpty().WithMessage("Hedef durum belirtilmelidir.");
        }
    }

    public class SceneScheduleDtoValidator : AbstractValidator<SceneScheduleDto>
    {
        public SceneScheduleDtoValidator()
        {
            RuleFor(x => x.CronExpression)
                .NotEmpty().WithMessage("Zamanlama ifadesi boş olamaz.")
                .Must(BeValidCronExpression).WithMessage("Geçerli bir CRON ifadesi girilmelidir.");
        }

        private bool BeValidCronExpression(string cronExpression)
        {
            try
            {
                // Burada Quartz.NET veya başka bir CRON parser kullanılabilir
                // Şimdilik basit bir kontrol yapıyoruz
                return cronExpression.Split(' ').Length == 5;
            }
            catch
            {
                return false;
            }
        }
    }
} 