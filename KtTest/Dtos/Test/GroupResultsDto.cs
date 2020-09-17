using System.Collections.Generic;

namespace KtTest.Dtos.Test
{
    public class GroupResultsDto
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int NumberOfQuestion { get; set; }
        public List<UserTestResultDto> Results { get; set; }
        public bool Ended { get; set; }
    }

    public class UserTestResultDto
    {
        public string Username { get; set; }
        public int NumberOfValidAnswers { get; set; }
    }
}
