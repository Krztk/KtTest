using KtTest.Dtos.Wizard;
using KtTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.JsonConverters
{
    public class WizardQuestionDtoConverter : JsonConverter<QuestionDto>
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
                        case "answer":
                            string answer = reader.GetString();
                            ((QuestionWithWrittenAnswerDto)question).Answer = answer;
                            break;
                        case "choices":
                            var choices = new List<ChoiceDto>();

                            if (reader.TokenType != JsonTokenType.StartArray)
                                throw new JsonException();

                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    var choice = JsonSerializer.Deserialize<ChoiceDto>(ref reader, options);
                                    choices.Add(choice);
                                }

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
                        case "categories":
                            if (reader.TokenType != JsonTokenType.StartArray)
                                throw new JsonException();

                           var categories = new List<int>();

                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                if (reader.TokenType == JsonTokenType.Number)
                                {
                                    var category = reader.GetInt32();
                                    categories.Add(category);
                                }

                                if (!reader.Read())
                                {
                                    throw new JsonException();
                                }
                            }
                            question.Categories = categories;
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
                writer.WriteString("answer", writtenAnswer.Answer);
            }
            else if (value is QuestionWithChoiceAnswersDto choiceAnswer)
            {
                writer.WriteNumber("t", (int)GetTypeDiscriminator(choiceAnswer));
                writer.WriteStartArray("choices");

                if (choiceAnswer.Choices != null)
                {
                    foreach (var choice in choiceAnswer.Choices)
                    {
                        JsonSerializer.Serialize(writer, choice, options);
                    }
                }

                writer.WriteEndArray();
            }

            writer.WriteString("question", value.Question);
            writer.WriteNumber("id", value.Id);
            writer.WriteStartArray("categories");
            if (value.Categories != null)
            {
                foreach (var category in value.Categories)
                {
                    writer.WriteNumberValue(category);
                }
            }
            writer.WriteEndArray();
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
