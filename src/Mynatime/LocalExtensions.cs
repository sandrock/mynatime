
namespace Mynatime.CLI;

using Mynatime.Infrastructure.ProfileTransaction;
using System;

public static class LocalExtensions
{
    public static void Accept(this ITransactionItem item, ITransactionItemVisitor visitor)
    {
        dynamic thing = item;
        visitor.Visit(thing);
    }
}
