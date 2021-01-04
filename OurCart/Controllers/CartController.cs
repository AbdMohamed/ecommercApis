using Microsoft.AspNetCore.Mvc;
using OurCart.Utils;
using OURCart.Core.IServices;
using OURCart.DataModel.DTO;
using OURCart.Infrastructure.Util;
using System;
using System.Threading.Tasks;

namespace OurCart.Controllers
{

    [Produces("application/json")]
    [Route("api/Cart")]
    public class CartController : BaseController<CartProducts>
    {
        ICartService _cartService;
        public CartController(OurCartDBContext _dbContext, ICartService cartService) : base(_dbContext)
        {

            _cartService = cartService;
        }

        [HttpGet("getCartItemsByUserID")]
        public async Task<IActionResult> gteAllCartProductsByUserID([FromQuery]decimal userID)
        {
            var result = await _cartService.getAllCartProductsByUserID(userID);
            return resultWithStatus(result);
        }
        [HttpPost("Create")]
        [ModelStateValidation]
      
        public async Task<IActionResult> AddItemToUserCart([FromBody]CartProducts cartItem)
        {
            var result = await _cartService.AddItemToUserCart(cartItem);
            return resultWithStatus(result);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteItemFromUserCart([FromBody]CartProducts cartItem)
        {
            var result = await _cartService.DeleteItemFromUSerCart(cartItem);
            return resultWithStatus(result);
        }

        [HttpPost("updateQuantitiy")]
        [ModelStateValidation]
        public async Task<IActionResult> UpdateItemQuantityInCart([FromBody]CartProducts cartItem)
        {
            var result = await _cartService.UpdateItemQuantityInCart(cartItem);
            return resultWithStatus(result);
        }


    }
}