using System.Collections.Generic;

namespace KtTest.Dtos.Test
{
    public class GroupResultsDto
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public float MaxTestScore { get; set; }
        public List<UserTestResultDto> Results { get; set; }
        public bool Ended { get; set; }
    }

    public class UserTestResultDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public float? UserScore { get; set; }
        public string Status { get; set; }
    }
}
