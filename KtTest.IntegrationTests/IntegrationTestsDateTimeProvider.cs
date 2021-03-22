using KtTest.Services;
using System;

namespace KtTest.IntegrationTests
{
    public class IntegrationTestsDateTimeProvider : IDateTimeProvider
    {
        public static DateTime utcNow = new DateTime(2021, 3, 22, 15, 0, 12, DateTimeKind.Utc);
        public DateTime UtcNow { get; } = utcNow;
    }
}
