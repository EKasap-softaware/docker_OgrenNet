namespace Core.Utilities.Results;

public class Result : IResult
{
    public Result(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }
}

public class SuccessResult : Result
{
    public SuccessResult(string message = "") : base(true, message) { }
}

public class ErrorResult : Result
{
    public ErrorResult(string message = "") : base(false, message) { }
}
