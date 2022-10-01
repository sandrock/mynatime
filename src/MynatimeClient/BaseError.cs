namespace Mynatime.Client;

/// <summary>
/// An error associated to an API access. 
/// </summary>
public sealed class BaseError
{
    public BaseError(string code)
    {
        this.Code = code;
        this.Message = code;
    }

    public BaseError(string code, string? message)
    {
        this.Code = code;
        this.Message = message;
    }

    public string Code { get; }

    public string? Message { get; }
}
