﻿using FluentValidation;
using KtTest.Infrastructure.Data;
using KtTest.Models;
using System.Collections.Generic;

namespace KtTest.Dtos.Wizard
{
    public abstract class QuestionDto
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public List<int> Categories { get; set; } = new List<int>();
        public float Score { get; set; }
    }

    public class QuestionWithWrittenAnswerDto : QuestionDto
    {
        public string Answer { get; set; }
    }

    public class QuestionWithChoiceAnswersDto : QuestionDto
    {
        public ChoiceAnswerType ChoiceAnswerType { get; set; }
        public bool AllValidChoicesRequired { get; set; }
        public List<ChoiceDto> Choices { get; set; }
    }

    public class ChoiceDto
    {
        public string Content { get; set; }
        public bool Valid { get; set; }
    }

    public class QuestionDtoValidator<T> : AbstractValidator<T> where T : QuestionDto
    {
        public QuestionDtoValidator()
        {
            RuleFor(x => x.Question).NotEmpty().MaximumLength(DataConstraints.Question.MaxQuestionLength);
            RuleFor(x => x.Score).GreaterThan(DataConstraints.Question.MinScore).LessThanOrEqualTo(DataConstraints.Question.MaxScore);
        }
    }

    public class QuestionWithWrittenAnswerDtoValidator : QuestionDtoValidator<QuestionWithWrittenAnswerDto>
    {
        public QuestionWithWrittenAnswerDtoValidator()
        {
            RuleFor(x => x.Answer).NotEmpty().MaximumLength(DataConstraints.Question.MaxAnswerLength);
        }
    }

    public class QuestionWithChoiceAnswersDtoValidator : QuestionDtoValidator<QuestionWithChoiceAnswersDto>
    {
        public QuestionWithChoiceAnswersDtoValidator()
        {
            RuleFor(x => x.Choices).NotEmpty();
            RuleForEach(x => x.Choices).ChildRules(choices =>
            {
                choices.RuleFor(x => x.Content).NotEmpty().MaximumLength(DataConstraints.Question.MaxAnswerLength);
            });
        }
    }
}
