using FluentValidation;
using KtTest.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Dtos.Wizard
{
    public class PublishTestDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationInMinutes { get; set; }
        public int GroupId { get; set; }
    }

    public class PublishTestDtoValidator : AbstractValidator<PublishTestDto>
    {
        public PublishTestDtoValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x.DurationInMinutes).GreaterThan(DataConstraints.Test.MinDuration);
            RuleFor(x => x.GroupId).NotEmpty();
        }
    }
}
