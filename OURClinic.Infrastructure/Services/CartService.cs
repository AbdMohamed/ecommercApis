using Microsoft.EntityFrameworkCore;
using OurCart.DataModel;
using OurCart.Infrastructure.Services;
using OURCart.Core.IServices;
using OURCart.DataModel.DTO;
using OURCart.DataModel.DTO.LocalModels;
using OURCart.Infrastructure.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OURCart.Infrastructure.Services
{
    public class CartService : BaseRepository<CartProducts>, ICartService
    {
        ICategoryService _catService;
        public CartService(OurCartDBContext dbContext, ICategoryService catService) : base(dbContext)
        {
            _catService = catService;
        }

        public async Task<OperationResponse<CartProducts>> AddItemToUserCart(CartProducts item)
        {
            OperationResponse<CartProducts> or = new OperationResponse<CartProducts>();
            try
            {
                var isExistPBefore = await _dbContext.CartProducts.Where(i => i.FkDeliveryClientId == item.FkDeliveryClientId
                && i.FkItemPackageId == (item.FkItemPackageId ?? 0) && i.FkTemId == item.FkTemId).AnyAsync();
                if (!isExistPBefore)
                {
                    item.InsertDateTime = DateTime.Now;
                    item.IsNew = true;
                    var addedItem = await _dbContext.CartProducts.AddAsync(item);
                    var rowsEffected = _dbContext.SaveChanges();
                    if (rowsEffected > 0)
                        or.Data = item;
                }
                else
                {
                    or.HasErrors = true;
                    or.Message = "this item is already Added in Cart Before you can increase or decrease quantity only";
                }
            }
            catch (Exception e)
            {
                or.HasErrors = true;
                or.Message = e.Message;
            }
            return or;
        }

        public async Task<OperationResponse<bool>> ClearUserCart(decimal userID)
        {
            OperationResponse<bool> or = new OperationResponse<bool>();
            try
            {
                var cartItems = await _dbContext.CartProducts.Where(i => i.FkDeliveryClientId == userID).ToListAsync();
                _dbContext.CartProducts.RemoveRange(cartItems);
                //foreach (var item in cartItems)
                //{
                //    _dbContext.CartProducts.Remove(item);
                //}
                var rowsEffected = _dbContext.SaveChanges();
                if (rowsEffected >= 0)
                    or.Data = true;
            }
            catch (Exception ex)
            {
                or.Message = ex.Message;
                or.HasErrors = true;
            }
            return or;
        }

        // get cart products for specific ID
        // response will be object with simple data of default item id 
        // CartResponseModel --> item detail data
        public async Task<OperationResponse<UserCartResponseModel>> getAllCartProductsByUserID(decimal userID)
        {
            OperationResponse<UserCartResponseModel> or = new OperationResponse<UserCartResponseModel>();
            try
            {
                //get all cart products data from stored procedure
                var userCartProducts = await _dbContext.userCartItem.FromSql($"GetCurrentDeliveryClientCertProducst {userID}").ToListAsync();

                //get SubTotal price (total of all items)
                decimal subTotal = 0;
                foreach (var item in userCartProducts) // calculate all price needed with the item discount
                    subTotal += ((item.CustomerPrice ?? 0) - (item.PurchaseDiscount ?? 0)) * item.quantity;
                //get delivry price
                var areaID = _dbContext.DeliveryClient.FirstOrDefault(c => c.DelClientId == userID).FkAreaId;
                var deliveryPrice = _dbContext.Area.Where(a => a.AreaId == areaID).FirstOrDefault().DeliveryAmount;

                UserCartResponseModel result = new UserCartResponseModel()
                {
                    allProducts = userCartProducts,
                    Total = subTotal + deliveryPrice,
                    SubTotal = subTotal,
                    Delivery = deliveryPrice
                };
                if (result != null)
                    or.Data = result;

                /**
                                //var result =
                                //    from c in _dbContext.CartProducts
                                //    where c.FkDeliveryClientId == userID
                                //    select new CartProducts
                                //    {
                                //        Id = c.Id,
                                //        FkDeliveryClientId = c.FkDeliveryClientId,
                                //        FkItemPackageId = c.FkItemPackageId,
                                //        FkTemId = c.FkTemId,
                                //        item = c.item,
                                //        InsertDateTime = c.InsertDateTime,
                                //        Quantity = c.Quantity

                                //    };
                                */

                /**
                // _dbContext.CartProducts.Where(p => p.FkDeliveryClientId == userID).ToList();
                //if (result != null)
                //{
                //    var cartItems = new List<CartResponseModel>();
                //    foreach (var item in result)
                //    {
                //        cartItems.Add(
                //            new CartResponseModel()
                //            {
                //                cartProduct = item,
                //                // get items in from categoy service with all data of item   
                //                itemsData = (await _catService.getProductDataByItemIDandPAckagedID(item.Id, item.FkItemPackageId)).Data
                //            }
                //        );
                //    }
                //    or.Data = result;
                //}*/
            }
            catch (Exception ex)
            {

                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }

        public async Task<OperationResponse<decimal>> getTotalPrice(decimal userID)
        {
            OperationResponse<decimal> or = new OperationResponse<decimal>();
            try
            {
                var totalPrice = await _dbContext.CartTotalModel.FromSql($"GetSumOfCartPRoducts {userID}").FirstOrDefaultAsync();
                or.Data = totalPrice.total;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                or.HasErrors = true;
                or.Message = msg;
            }
            return or;
        }
       
        public async Task<OperationResponse<bool>> DeleteItemFromUSerCart(CartProducts item)
        {
            OperationResponse<bool> or = new OperationResponse<bool>();
            try
            {
                // get all item form user cart with the same createria (case inserted with worng many times before)
                var itemsWithSameCreateria = await _dbContext.CartProducts.Where(i =>
                i.FkDeliveryClientId == item.FkDeliveryClientId
                && i.FkItemPackageId == item.FkItemPackageId
                && i.FkTemId == item.FkTemId).ToListAsync();

                if (itemsWithSameCreateria != null && itemsWithSameCreateria.Count > 0)
                {
                    _dbContext.RemoveRange(itemsWithSameCreateria); // remove Items / Item
                    var rowsEffected = _dbContext.SaveChanges();
                    if (rowsEffected > 0)
                        or.Data = true;

                }
                if (!or.Data)
                {
                    or.Message = "no item in user cart with sended createria";
                    or.HasErrors = true;
                }
            }
            catch (Exception ex)
            {
                or.Message = ex.Message;
                or.HasErrors = true;
            }
            return or;
        }

        public async Task<OperationResponse<bool>> UpdateItemQuantityInCart(CartProducts item)
        {
            OperationResponse<bool> or = new OperationResponse<bool>();
            try
            {
                var product = await _dbContext.CartProducts.Where(i => i.FkDeliveryClientId == item.FkDeliveryClientId
                && i.FkItemPackageId == (item.FkItemPackageId ?? 0) && i.FkTemId == item.FkTemId)
                .FirstOrDefaultAsync();
                if (product != null)
                {
                    product.Quantity = item.Quantity == 0 ? product.Quantity : item.Quantity;
                }
                else
                    throw new Exception("item not exist in usercart");
                _dbContext.SaveChanges();
                or.Data = true;
            }
            catch (Exception e)
            {
                or.Data = false;
                or.HasErrors = true;
                or.Message = e.Message;
            }
            return or;
        }
    }
}
