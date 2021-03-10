using KtTest.Infrastructure.JsonConverters;
using System.Text.Json;

namespace KtTest.IntegrationTests.Helpers
{
    public static class JsonSerializerOptionsHelper
    {
        public static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new WizardQuestionDtoConverter());
            options.Converters.Add(new TestQuestionDtoConverter());
            options.Converters.Add(new QuestionAnswerDtoConverter());
            options.Converters.Add(new QuestionWithResultDtoConverter());
            return options;
        }
    }
}
