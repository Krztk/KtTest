using KtTest.Results.Errors;
using System;
using System.Threading.Tasks;

namespace KtTest.Results
{
    public class OperationResult<TData>
    {
        public ErrorBase Error { get; }
        public bool Succeeded => Error == null;
        public TData Data { get; set; }

        public OperationResult(TData data)
        {
            Data = data;
        }

        public OperationResult(ErrorBase error)
        {
            Error = error;
        }

        public OperationResult<TResult> Bind<TResult>(Func<TData, TResult> func)
        {
            return Succeeded ? new OperationResult<TResult>(func(Data)) : Error;
        }

        public async Task<OperationResult<TResult>> Bind<TResult>(Func<TData, Task<TResult>> func)
        {
            return Succeeded ? new OperationResult<TResult>(await func(Data)) : Error;
        }

        public OperationResult<TResult> Then<TResult>(Func<TData, OperationResult<TResult>> func)
        {
            return Succeeded ? func(Data) : Error;
        }

        public async Task<OperationResult<TResult>> Then<TResult>(Func<TData, Task<OperationResult<TResult>>> func)
        {
            return Succeeded ? await func(Data) : Error;
        }

        public T Match<T>(Func<ErrorBase, T> onError, Func<TData, T> onSuccess)
        {
            return Succeeded ? onSuccess(Data) : onError(Error);
        }

        public static implicit operator OperationResult<TData>(ErrorBase error) => new OperationResult<TData>(error);
        public static implicit operator OperationResult<TData>(TData data) => new OperationResult<TData>(data);
        public static implicit operator TData(OperationResult<TData> result) => result.Data;
    }

    public static class OperationResult
    {
        public static OperationResult<Unit> Ok => okResult;
        private static readonly OperationResult<Unit> okResult = new OperationResult<Unit>(Unit.Value);
    }
}
