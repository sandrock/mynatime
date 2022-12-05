namespace Mynatime.Domain;

using Mynatime.Client;

public class ActivityItemWrapper
{
    public ActivityItemWrapper(NewActivityItemPage item)
    {
        this.Item = item;
    }

    public ActivityItemWrapper()
        : this(new NewActivityItemPage())
    {
    }

    public NewActivityItemPage Item { get; }

    /// <summary>
    /// Indicates whether the current item is completed. 
    /// </summary>
    public bool IsStartAndStop { get; set; }
}
