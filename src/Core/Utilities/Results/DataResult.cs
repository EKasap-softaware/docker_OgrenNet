namespace Core.Utilities.Results;

public class DataResult<T> : Result, IDataResult<T>
{
    public DataResult(T? data, bool success, string message = "") : base(success, message)
    {
        Data = data;
    }

    public T? Data { get; }
}

public class SuccessDataResult<T> : DataResult<T>
{
    public SuccessDataResult(T data, string message = "") : base(data, true, message) { }
}

public class ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(string message = "") : base(default, false, message) { }
}
