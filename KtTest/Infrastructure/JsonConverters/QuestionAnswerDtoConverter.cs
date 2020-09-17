using KtTest.Dtos.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace KtTest.Infrastructure.JsonConverters
{
    public class QuestionAnswerDtoConverter : JsonConverter<QuestionAnswerDto>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(QuestionAnswerDto).IsAssignableFrom(typeToConvert);

        public override QuestionAnswerDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString();
            if (propertyName != "text" && propertyName != "value")
            {
                throw new JsonException();
            }

            QuestionAnswerDto dto;

            reader.Read();
            if (propertyName == "text")
            {
                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                var text = reader.GetString();
                dto = new WrittenAnswerDto { Text = text };
                
            }
            else if (propertyName == "value")
            {
                if (reader.TokenType != JsonTokenType.Number)
                    throw new JsonException();
                
                var value = reader.GetInt32();
                dto = new ChoiceAnswerDto { Value = value };
            }
            else
                throw new JsonException();

            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            propertyName = reader.GetString();
            if (propertyName != "q")
                throw new JsonException();

            reader.Read();

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException();

            var questionId = reader.GetInt32();
            dto.QuestionId = questionId;

            reader.Read();
            if (reader.TokenType == JsonTokenType.EndObject)
                return dto;
            else
                throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, QuestionAnswerDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value is WrittenAnswerDto writtenAnswer)
            {
                writer.WriteString("text", writtenAnswer.Text);
            }
            else if (value is ChoiceAnswerDto choiceAnswer)
            {
                writer.WriteNumber("value", choiceAnswer.Value);
            }
            writer.WriteNumber("q", value.QuestionId);
            writer.WriteEndObject();
        }
    }
}
