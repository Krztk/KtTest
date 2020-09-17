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
            throw new NotImplementedException();
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
