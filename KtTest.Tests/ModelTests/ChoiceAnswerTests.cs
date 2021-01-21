using FluentAssertions;
using KtTest.Models;
using System.Collections.Generic;
using Xunit;

namespace KtTest.Tests.ModelTests
{
    public class ChoiceAnswerTests
    {
        public static float MaxScore = 6f;
        public static IEnumerable<object[]> GetValidChoiceAnswerContructorParamaters()
        {
            yield return new object[]
            {
                new List<Choice>
                {
                    //{ true, true, false, true }; => 1101 => 13
                    new Choice { Content = "Choice 1", Valid = true },
                    new Choice { Content = "Choice 2", Valid = true },
                    new Choice { Content = "Choice 3", Valid = false },
                    new Choice { Content = "Choice 4", Valid = true },
                },
                ChoiceAnswerType.MultipleChoice,
                13,
                MaxScore
            };
            yield return new object[]
            {
                new List<Choice>
                {
                    //{ false, false, false, true }; => 0001 => 1
                    new Choice { Content = "Choice 1", Valid = false },
                    new Choice { Content = "Choice 2", Valid = false },
                    new Choice { Content = "Choice 3", Valid = false },
                    new Choice { Content = "Choice 4", Valid = true },
                },
                ChoiceAnswerType.SingleChoice,
                1,
                MaxScore
            };
            yield return new object[]
            {
                new List<Choice>
                {
                    //{ false, false, false, true }; => 1001 => 9
                    new Choice { Content = "Choice 1", Valid = true },
                    new Choice { Content = "Choice 2", Valid = false },
                    new Choice { Content = "Choice 3", Valid = false },
                    new Choice { Content = "Choice 4", Valid = true },
                },
                ChoiceAnswerType.MultipleChoice,
                9,
                MaxScore
            };
        }

        [Theory]
        [MemberData(nameof(GetValidChoiceAnswerContructorParamaters))]
        public void Constructor_ValidData_CreatesObjectWithCalculatedNumericValue(List<Choice> choices, ChoiceAnswerType choiceAnswerType, int expectedNumericValue, float maxScore)
        {
            //act
            ChoiceAnswer choiceAnswer = new ChoiceAnswer(choices, choiceAnswerType, maxScore);

            //assert
            choiceAnswer.NumericValue.Should().Be(expectedNumericValue);
        }

    }
}
