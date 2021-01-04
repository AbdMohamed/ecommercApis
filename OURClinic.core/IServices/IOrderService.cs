using OurCart.DataModel;
using OURCart.DataModel.DTO;
using OURCart.DataModel.DTO.LocalModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OURCart.Core.IServices
{
    public interface IOrderService
    {
        // convert currnt items in cart to order
        Task<OperationResponse<IEnumerable<userCartItem>>> checkoutUserOrder(decimal delivertClientID);
        //get all old orders of user as header data and details with all items data 
        OperationResponse<IEnumerable<PoscurrentDailyTransHeader>> GetHistoryOrders(decimal delivertClientID);
        Task<OperationResponse<bool>> reorder(decimal deliveryClientID, decimal HeaderID);
       OperationResponse<IEnumerable<PoscurrentDailyTransHeader>> GetCurrentOrders(decimal delivertClientID);
        OperationResponse<bool> DeleteHistoryOrder(decimal UserId, decimal HeaderId);

    }
}
