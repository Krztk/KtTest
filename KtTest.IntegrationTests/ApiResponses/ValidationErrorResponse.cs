using System.Collections.Generic;

namespace KtTest.IntegrationTests.ApiResponses
{
    public class ValidationErrorResponse
    {
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}
