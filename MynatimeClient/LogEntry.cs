namespace MynatimeClient;

using System.Text;

internal class LogEntry
{
    public LogEntry(string methodName)
    {
        this.Time = DateTime.UtcNow;
        this.MethodName = methodName;
    }

    public DateTime Time { get; private set; }

    public string MethodName { get; private set; }

    public string Message { get; private set; }

    public string Exception { get; set; }

    public void SetRequest(HttpRequestMessage request)
    {
        this.Message = "Sending request   " + request.ToString();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(this.Time.ToString("o"));
        builder.Append(' ');
        builder.Append(this.MethodName);
        builder.Append(' ');
        builder.Append(this.Message);
        return builder.ToString();
    }

    public void SetResponse(HttpResponseMessage response)
    {
        this.Message = "Received response " + response.ToString();
    }

    public void SetMessage(string message)
    {
        this.Message = message;
    }

    public void SetException(Exception exception)
    {
        this.Exception = exception.ToString();
        this.Message = exception.Message;
    }
}
