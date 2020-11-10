using KtTest.Dtos.Test;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KtTest.Infrastructure.JsonConverters
{
    public class QuestionAnswerDtoConverter : JsonConverter<QuestionAnswerDto>
    {
        enum AnswerType
        {
            Written,
            Choice,
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeof(QuestionAnswerDto).IsAssignableFrom(typeToConvert);

        public override QuestionAnswerDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            string text = string.Empty;
            int value = 0;
            int? questionId = null;
            AnswerType? answerType = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (!questionId.HasValue) throw new JsonException();
                    QuestionAnswerDto answerDto = answerType switch
                    {
                        AnswerType.Choice => new ChoiceAnswerDto { QuestionId = questionId.Value, Value = value },
                        AnswerType.Written => new WrittenAnswerDto { QuestionId = questionId.Value, Text = text },
                        _ => throw new JsonException()
                    };
                    return answerDto;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "text":
                            text = reader.GetString();
                            if (answerType.HasValue)
                                throw new JsonException();

                            answerType = AnswerType.Written;
                            break;
                        case "value":
                            value = reader.GetInt32();
                            if (answerType.HasValue)
                                throw new JsonException();

                            answerType = AnswerType.Choice;
                            break;
                        case "q":
                            questionId = reader.GetInt32();
                            break;
                    }
                }
            }
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
