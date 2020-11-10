using FluentAssertions;
using KtTest.Dtos.Test;
using KtTest.Infrastructure.JsonConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Xunit;

namespace KtTest.Tests.ConverterTests
{
    public class QuestionAnswerDtoConverterTests
    {
        public static IEnumerable<object[]> GetValidJsonStringsAndExpectedEquivalentDtos()
        {
            yield return new object[]
            {
                "{\"q\": 1, \"text\": \"string :)\"}",
                new WrittenAnswerDto { QuestionId = 1, Text = "string :)" }
            };
            yield return new object[]
            {
                "{\"text\": \"string :)\", \"q\": 1}",
                new WrittenAnswerDto { QuestionId = 1, Text = "string :)" }
            };
            yield return new object[]
            {
                "{\"q\": 1, \"value\": 2}",
                new ChoiceAnswerDto { QuestionId = 1, Value = 2 }
            };
            yield return new object[]
            {
                "{\"value\": 2, \"q\": 1}",
                new ChoiceAnswerDto { QuestionId = 1, Value = 2 }
            };
        }

        [Theory]
        [MemberData(nameof(GetValidJsonStringsAndExpectedEquivalentDtos))]
        public void Order_Of_JSON_Properties_Should_Not_Matter_When_Read(string jsonString, QuestionAnswerDto expectedObject)
        {
            var converter = new QuestionAnswerDtoConverter();
            var utf8JsonReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString), false, new JsonReaderState(new JsonReaderOptions()));

            while (utf8JsonReader.Read())
                if (utf8JsonReader.TokenType == JsonTokenType.StartObject)
                    break;
            QuestionAnswerDto obj = converter.Read(ref utf8JsonReader, typeof(QuestionAnswerDto), null);

            obj.Should().BeEquivalentTo(expectedObject);
        }
    }
}
