using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KtTest.Infrastructure.Data
{
    public class DataConstraints
    {
        public class Category
        {
            public static int MaxNameLength => 64;
        }

        public class Test
        {
            public static int MaxNameLength => 64;
            public static int MinDuration => 5;
        }

        public class Question
        {
            public static int MaxQuestionLength => 512;
            public static int MaxAnswerLength => 256;
        }

        public class Answer
        {
            public static int MaxWrittenAnswerLength => 256;
        }

        public class Group
        {
            public static int MaxNameLength => 256;
        }

        public class User
        {
            public static int MaxUsernameLength => 256;
        }
    }
}
