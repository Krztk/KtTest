using KtTest.Dtos.Test;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KtTest.Infrastructure.JsonConverters
{
    public class TestQuestionDtoConverter : JsonConverter<QuestionDto>
    {
        enum TypeDiscriminator
        {
            WithWrittenAnswer = 1,
            SingleChoice,
            MultipleChoice,
        }
        public override bool CanConvert(Type typeToConvert) =>
            typeof(QuestionDto).IsAssignableFrom(typeToConvert);

        public override QuestionDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            if (propertyName != "t")
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException();
            }

            TypeDiscriminator typeDiscriminator = (TypeDiscriminator)reader.GetInt32();
            QuestionDto question = typeDiscriminator switch
            {
                TypeDiscriminator.MultipleChoice => new QuestionWithChoiceAnswersDto { ChoiceAnswerType = ChoiceAnswerType.MultipleChoice },
                TypeDiscriminator.SingleChoice => new QuestionWithChoiceAnswersDto { ChoiceAnswerType = ChoiceAnswerType.SingleChoice },
                TypeDiscriminator.WithWrittenAnswer => new QuestionWithWrittenAnswerDto(),
                _ => throw new JsonException()
            };

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return question;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "choices":
                            var choices = new List<string>();

                            if (reader.TokenType != JsonTokenType.StartArray)
                                throw new JsonException();

                            reader.Read();

                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                
                                var choice = reader.GetString();
                                choices.Add(choice);
                                

                                if (!reader.Read())
                                {
                                    throw new JsonException();
                                }
                            }

                            ((QuestionWithChoiceAnswersDto)question).Choices = choices;
                            break;
                        case "question":
                            string questionValue = reader.GetString();
                            question.Question = questionValue;
                            break;
                        case "id":
                            int id = reader.GetInt32();
                            question.Id = id;
                            break;
                    }
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, QuestionDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value is QuestionWithWrittenAnswerDto writtenAnswer)
            {
                writer.WriteNumber("t", (int)TypeDiscriminator.WithWrittenAnswer);
            }
            else if (value is QuestionWithChoiceAnswersDto choiceAnswer)
            {
                writer.WriteNumber("t", (int)GetTypeDiscriminator(choiceAnswer));
                writer.WriteStartArray("choices");

                if (choiceAnswer.Choices != null)
                {
                    foreach (var choice in choiceAnswer.Choices)
                    {
                        writer.WriteStringValue(choice);
                    }
                }

                writer.WriteEndArray();
            }

            writer.WriteString("question", value.Question);
            writer.WriteNumber("id", value.Id);
            writer.WriteEndObject();
        }

        private static TypeDiscriminator GetTypeDiscriminator(QuestionWithChoiceAnswersDto choiceAnswer)
        {
            switch (choiceAnswer.ChoiceAnswerType)
            {
                case ChoiceAnswerType.MultipleChoice:
                    return TypeDiscriminator.MultipleChoice;
                case ChoiceAnswerType.SingleChoice:
                    return TypeDiscriminator.SingleChoice;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
