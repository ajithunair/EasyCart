using EasyCart.InventoryApi.Entities;
using EasyCart.SharedLibrary.Interfacess;

namespace EasyCart.InventoryApi.Interfaces
{
    public interface IInventory : IGenericInterface<Inventory>
    {
        Task<Inventory> GetInventoryByProductId(int productId);
    }
}
