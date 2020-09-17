using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Results
{
    public class Failure
    {
        public static Failure NotFound(string description = "") => new Failure(FailureType.NotFound, description);
        public static Failure BadRequest(string description = "") => new Failure(FailureType.BadRequest, description);
        public static Failure Unauthorized(string description = "") => new Failure(FailureType.Unauthorized, description);

        public string Description { get; private set; }
        public FailureType Status { get; private set; }

        public Failure(FailureType status, string description)
        {
            Status = status;
            Description = description;
        }
    }

    public enum FailureType
    {
        NotFound,
        BadRequest,
        Unauthorized
    }
}
