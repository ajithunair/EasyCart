using EasyCart.OrderApi.Entities;
using EasyCart.SharedLibrary.Interfacess;
using System.Linq.Expressions;

namespace EasyCart.OrderApi.Interfaces
{
    public interface IOrder : IGenericInterface<Order>
    {
        Task<IEnumerable<Order>> GetByAsync(Expression<Func<Order, bool>> predicate);
    }
}
