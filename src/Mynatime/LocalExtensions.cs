
namespace Mynatime.CLI;

using Mynatime.Infrastructure.ProfileTransaction;
using System;

public static class LocalExtensions
{
    public static Task Accept(this ITransactionItem item, ITransactionItemVisitor visitor)
    {
        dynamic thing = item;
        return visitor.Visit(thing);
    }
}
