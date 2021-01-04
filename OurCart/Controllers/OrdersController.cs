      using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OurCart.DataModel;
using OURCart.Core.IServices;
using OURCart.DataModel.DTO;
using OURCart.Infrastructure.Util;
using System.Threading.Tasks;

namespace OurCart.Controllers
{
    [Produces("application/json")]
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        OurCartDBContext _ourERPClinicContext;
        IOrderService _orderService;
        public OrdersController(OurCartDBContext ourERPClinicContext, IOrderService orderService)
        {
            _ourERPClinicContext = ourERPClinicContext;
            _orderService = orderService;
        }

        [HttpGet("checkout")]
        public async Task<IActionResult> CheckOutUSerCart([FromQuery] decimal deliveryClientID)
        {
            var result =  await _orderService.checkoutUserOrder(deliveryClientID);
            if (result.Data ==null)
            return Ok(result);

            return BadRequest(result);
           
        }

        [HttpGet("history")]
        public  IActionResult GetHistoryOrders([FromQuery] decimal deliveryClientID)
        {
            var res = _orderService.GetHistoryOrders(deliveryClientID);
            if (!res.HasErrors)
                return Ok(res);
            return BadRequest(res);
        }

        [HttpDelete("DeleteHistoryOrder")]
        public IActionResult DeleteHistoryOrder(decimal UserId,decimal HeaderId)
        {
            var res = _orderService.DeleteHistoryOrder(UserId,HeaderId);
            if (!res.HasErrors)
                return Ok(res);
            return BadRequest(res);
        }

        [HttpGet("Current")]
        public IActionResult GetCurrentOrders( decimal deliveryClientID)
        {
          var res =  _orderService.GetCurrentOrders(deliveryClientID);
            if (!res.HasErrors)
                return Ok (res);
            return BadRequest(res);
        }

        [HttpGet("reorder")]
        public async Task<IActionResult> Reorder([FromQuery] decimal deliveryClientID, [FromQuery]decimal HeaderID)
        {
            var res = await _orderService.reorder(deliveryClientID, HeaderID);
            if (!res.HasErrors)
                return Ok(res);
            return BadRequest(res);
        }

    }
}