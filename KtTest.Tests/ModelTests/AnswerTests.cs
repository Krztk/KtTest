using FluentAssertions;
using KtTest.Exceptions.ModelExceptions;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KtTest.Tests.ModelTests
{
    public class AnswerTests
    {
        [Fact]
        public void WrittenAnswer_WrittenUserAnswer_ReturnsTrue()
        {
            string expectedValue = "valid answer";
            Answer answer = new WrittenAnswer(expectedValue)
            {
                QuestionId = 1
            };
            var isValid = answer.ValidateAnswer(new WrittenUserAnswer(expectedValue, 1, 1, 1));
            isValid.Should().BeTrue();
        }

        [Fact]
        public void ChoiceAnswer_WrittenUserAnswer_ThrowsException()
        {
            var choices = new List<Choice>
            {
                new Choice {Content = "1", Valid = true},
                new Choice {Content = "2", Valid = false},
                new Choice {Content = "3", Valid = false},
            };
            Answer answer = new ChoiceAnswer(choices, ChoiceAnswerType.SingleChoice)
            {
                QuestionId = 1
            };

            Action act = () => answer.ValidateAnswer(new WrittenUserAnswer("written answer", 1, 1, 1));
            act.Should().Throw<WrongAnswerTypeException>();
            
        }
    }
}
