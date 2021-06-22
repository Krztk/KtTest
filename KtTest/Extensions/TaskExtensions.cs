using KtTest.Results;
using System;
using System.Threading.Tasks;

namespace KtTest.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<OperationResult<TOutput>> Then<TInput, TOutput>(this Task<OperationResult<TInput>> task, Func<TInput, OperationResult<TOutput>> func)
        {
            return (await task).Then(func);
        }

        public static async Task<OperationResult<TOutput>> Then<TInput, TOutput>(this Task<OperationResult<TInput>> task, Func<TInput, Task<OperationResult<TOutput>>> func)
        {
            return await (await task).Then(func);
        }
    }
}
