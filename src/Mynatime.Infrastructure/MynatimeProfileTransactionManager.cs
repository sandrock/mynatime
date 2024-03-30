
namespace Mynatime.Infrastructure;

using Mynatime.Infrastructure.ProfileTransaction;
using System;

/// <summary>
/// Allows conversion from domain objects (<see cref="ITransactionItem"/>) to transaction objects (<see cref="MynatimeProfileTransactionItem"/>) and back.
/// </summary>
public sealed class MynatimeProfileTransactionManager
{
    private static MynatimeProfileTransactionManager? defaultInstance;

    private static readonly List<TransactionItemType> transactionItemTypes = new List<TransactionItemType>();

    public static MynatimeProfileTransactionManager Default
    {
        get { return defaultInstance ?? (defaultInstance = CreateDefaultInstance()); }
    }

    public void RegisterTransactionItemType<T>(string typeName, Func<MynatimeProfileTransactionItem, ITransactionItem> create)
        where T : ITransactionItem
    {
        if (typeName == null)
        {
            throw new ArgumentNullException(nameof(typeName));
        }

        if (create == null)
        {
            throw new ArgumentNullException(nameof(create));
        }

        transactionItemTypes.Add(new TransactionItemType(typeof(T), typeName, create));
    }

    /// <summary>
    /// Checks whether the transaction item corresponds to the type specified. 
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool OfClass<T>(MynatimeProfileTransactionItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var requestType = typeof(T);
        foreach (var itemType in transactionItemTypes)
        {
            if (itemType.Type == requestType)
            {
                return item.ObjectType == itemType.ObjectType;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates an instance of the domain object (<see cref="ITransactionItem"/>) using the specified transaction object. 
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ITransactionItem GetInstanceOf(MynatimeProfileTransactionItem operation)
    {
        foreach (var itemType in transactionItemTypes)
        {
            if (itemType.ObjectType == operation.ObjectType)
            {
                return itemType.CreateInstanceFromItem(operation);
            }
        }

        throw new InvalidOperationException("Cannot create an instance for a " + operation.ObjectType);
    }

    private static MynatimeProfileTransactionManager CreateDefaultInstance()
    {
        var me = new MynatimeProfileTransactionManager();
        return me;
    }

    private class TransactionItemType
    {
        private readonly Func<MynatimeProfileTransactionItem, ITransactionItem> create;

        public TransactionItemType(Type type, string objectType, Func<MynatimeProfileTransactionItem, ITransactionItem>  create)
        {
            this.create = create;
            this.Type = type;
            this.ObjectType = objectType;
        }

        public Type Type { get; }

        public string ObjectType { get; }

        public ITransactionItem CreateInstanceFromItem(MynatimeProfileTransactionItem operation)
        {
            return this.create(operation);
        }
    }
}
