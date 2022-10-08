namespace Mynatime.Infrastructure.ProfileTransaction;

public interface ITransactionItem
{
    MynatimeProfileTransactionItem ToTransactionItem(MynatimeProfileTransactionItem? root, DateTime utcNow);
}
