namespace MynatimeClient;

/// <summary>
/// A result base object associated to an API access. 
/// </summary>
public class BaseResult
{
    /// <summary>
    /// Nullable list of errors. 
    /// </summary>
    public List<BaseError>? Errors { get; set; }

    /// <summary>
    /// Indicates whether the operation succeeded. 
    /// </summary>
    public bool Succeed
    {
        get { return this.Errors == null || this.Errors.Count == 0; }
    }

    /// <summary>
    /// Creates a success response of the specified type. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Success<T>()
        where T : BaseResult, new()
    {
        var item = new T();
        return item;
    }

    /// <summary>
    /// Creates an error response of the specified type with the specified error. 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T Error<T>(string code, string? message)
        where T : BaseResult, new()
    {
        var item = new T();
        item.Errors = new List<BaseError>();
        item.Errors.Add(new BaseError(code, message));
        return item;
    }

    /// <summary>
    /// Obtains, if any, error message from this result. 
    /// </summary>
    /// <returns></returns>
    public string? GetErrorMessage()
    {
        return this.Errors?.FirstOrDefault()?.Message;
    }

    /// <summary>
    /// Adds an error to this operation result. 
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public BaseResult AddError(BaseError error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }
        
        if (this.Errors == null)
        {
            this.Errors = new List<BaseError>();
        }
        
        this.Errors.Add(error);
        return this;
    }
}