namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Infrastructure.ProfileTransaction;

public interface ITransactionItemVisitor
{
    Task Visit(ActivityStartStop thing);
    Task Visit(NewActivityItemPage thing);
    Task Visit(ITransactionItem thing);
}
