namespace Mynatime.GUI.ViewModels;

using System;

public class DataEventArgs<T> : EventArgs
{
    public DataEventArgs()
    {
    }

    public DataEventArgs(T? data)
    {
        this.Data = data;
    }

    public T? Data { get; }
}
