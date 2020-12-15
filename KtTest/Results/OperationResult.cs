using KtTest.Results.Errors;
using System;

namespace KtTest.Results
{
    public class OperationResult
    {
        private static readonly OperationResult okResult = new OperationResult();
        public ErrorBase Error { get; }
        public bool Succeeded => Error == null;
        public OperationResult()
        {
        }

        public OperationResult(ErrorBase error)
        {
            Error = error;
        }

        public static OperationResult Ok() => okResult;

        public static implicit operator OperationResult(ErrorBase error) => new OperationResult(error);
    }

    public class OperationResult<TData> : OperationResult
    {
        public TData Data { get; set; }

        public OperationResult(TData data)
        {
            Data = data;
        }

        public OperationResult(ErrorBase error) : base(error)
        {
        }

        public OperationResult<TResult> Then<TResult>(Func<TData, TResult> func)
        {
            return Succeeded ? (OperationResult<TResult>)func(Data) : Error;
        }

        public T Match<T>(Func<ErrorBase, T> onError, Func<TData, T> onSuccess)
        {
            return Succeeded ? onSuccess(Data) : onError(Error);
        }

        public static implicit operator OperationResult<TData>(ErrorBase error) => new OperationResult<TData>(error);

        public static implicit operator OperationResult<TData>(TData data) => new OperationResult<TData>(data);

        public static implicit operator TData(OperationResult<TData> result) => result.Data;
    }
}
