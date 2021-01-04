using OurCart.DataModel;
using OURCart.DataModel.DTO;
using OURCart.DataModel.DTO.LocalModels;
using OURClinic.Core.IServices;
using System.Threading.Tasks;

namespace OURCart.Core.IServices
{
    public interface ICartService : IRepository<CartProducts>
    {
        Task<OperationResponse<UserCartResponseModel>> getAllCartProductsByUserID(decimal userID);
        Task<OperationResponse<CartProducts>> AddItemToUserCart(CartProducts item);
        Task<OperationResponse<decimal>> getTotalPrice(decimal userID);
        Task<OperationResponse<bool>> ClearUserCart(decimal userID);
        Task<OperationResponse<bool>> DeleteItemFromUSerCart(CartProducts item);
        Task<OperationResponse<bool>> UpdateItemQuantityInCart(CartProducts cartItem);
    }
}
