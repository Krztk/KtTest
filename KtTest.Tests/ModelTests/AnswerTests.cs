using FluentAssertions;
using KtTest.Exceptions.ModelExceptions;
using KtTest.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace KtTest.Tests.ModelTests
{
    public class AnswerTests
    {
        [Fact]
        public void WrittenAnswer_WrittenUserAnswer_ReturnsMaxScore()
        {
            string validAnswer = "valid answer";
            float maxScore = 6f;
            Answer answer = new WrittenAnswer(validAnswer, maxScore)
            {
                QuestionId = 1
            };
            var score = answer.GetScore(new WrittenUserAnswer(validAnswer, 1, 1, 1));
            score.Should().Be(maxScore);
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
            Answer answer = new ChoiceAnswer(choices, ChoiceAnswerType.SingleChoice, 6f)
            {
                QuestionId = 1
            };

            Action act = () => answer.GetScore(new WrittenUserAnswer("written answer", 1, 1, 1));
            act.Should().Throw<WrongAnswerTypeException>();
        }

        [Theory]
        [MemberData(nameof(GetExpectedScoresAndNumericAnswers))]
        public void ChoiceAnswer_NotAllValidChoicesRequired_ReturnValidScore(float maxScore, float expectedScore, int numericAnswer)
        {
            var choices = new List<Choice>
            {
                new Choice {Content = "1", Valid = true},
                new Choice {Content = "2", Valid = false},
                new Choice {Content = "3", Valid = false},
                new Choice {Content = "3", Valid = true}
            };

            Answer answer = new ChoiceAnswer(choices, ChoiceAnswerType.MultipleChoice, maxScore, false)
            {
                QuestionId = 1
            };
            
            var score = answer.GetScore(new ChoiceUserAnswer(numericAnswer, 1, 1, 1));
            score.Should().Be(expectedScore);
        }

        private static int GetNumericValue(params bool[] choicesValidity)
        {
            var content = "c";
            var choices = choicesValidity.Select(x => new Choice { Content = content, Valid = x }).ToList();
            return new ChoiceAnswer(choices, ChoiceAnswerType.MultipleChoice, 1f).NumericValue;
        }

        public static IEnumerable<object[]> GetExpectedScoresAndNumericAnswers()
        {
            yield return new object[]
            {
               6f,
               6f,
               GetNumericValue(true, false, false, true)
            };
            yield return new object[]
            {
               6f,
               3f,
               GetNumericValue(true, false, false, false)
            };
            yield return new object[]
            {
               6f,
               3f,
               GetNumericValue(false, false, false, true)
            };
            yield return new object[]
            {
               6f,
               0f,
               GetNumericValue(true, false, true, true)
            };
        }
    }
}
