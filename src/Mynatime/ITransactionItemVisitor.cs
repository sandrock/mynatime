namespace Mynatime.CLI;

using Mynatime.Client;
using Mynatime.Infrastructure.ProfileTransaction;

public interface ITransactionItemVisitor
{
    void Visit(ActivityStartStop thing);
    void Visit(NewActivityItemPage thing);
    void Visit(ITransactionItem thing);
}
