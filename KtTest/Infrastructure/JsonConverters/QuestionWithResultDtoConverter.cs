using KtTest.Dtos.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.JsonConverters
{
    public class QuestionWithResultDtoConverter : JsonConverter<QuestionWithResultDto>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(QuestionWithResultDto).IsAssignableFrom(typeToConvert);

        public override QuestionWithResultDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            string question = null;
            int? questionId = null;
            string userAnswer = null, correctAnswer = null;
            List<ChoiceDto> choices = null;
            var visitedProperties = new HashSet<string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (question == null || !questionId.HasValue)
                        throw new JsonException();

                    if (userAnswer != null)
                    {
                        if (correctAnswer == null)
                            throw new JsonException();

                        return new QuestionWithWrittenResultDto
                        {
                            Question = question,
                            QuestionId = questionId.Value,
                            CorrectAnswer = correctAnswer,
                            UserAnswer = userAnswer
                        };
                    }

                    if (choices == null)
                        throw new JsonException();

                    return new QuestionWithChoiceAnswerResultDto
                    {
                        Question = question,
                        QuestionId = questionId.Value,
                        Choices = choices
                    };
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();
                    var alreadyVisited = !visitedProperties.Add(propertyName);
                    if (alreadyVisited)
                        throw new JsonException();

                    switch (propertyName)
                    {
                        case "choices":
                            choices = new List<ChoiceDto>();
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
                            break;
                        case "question":
                            question = reader.GetString();
                            break;
                        case "userAnswer":
                            userAnswer = reader.GetString();
                            break;
                        case "correctAnswer":
                            correctAnswer = reader.GetString();
                            break;
                        case "questionId":
                            questionId = reader.GetInt32();
                            break;
                    }
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, QuestionWithResultDto value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("question", value.Question);
            writer.WriteNumber("questionId", value.QuestionId);

            
            if (value is QuestionWithChoiceAnswerResultDto choiceQuestion)
            {
                writer.WriteStartArray("choices");
                JsonSerializer.Serialize(writer, choiceQuestion.Choices, options);
                writer.WriteEndArray();
            }
            else if (value is QuestionWithWrittenResultDto writtenQuestion)
            {
                writer.WriteString("userAnswer", writtenQuestion.UserAnswer);
                writer.WriteString("correctAnswer", writtenQuestion.CorrectAnswer);
            }
            
            writer.WriteEndObject();
        }
    }
}
