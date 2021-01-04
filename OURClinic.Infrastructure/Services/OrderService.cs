using Microsoft.EntityFrameworkCore;
using OurCart.DataModel;
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
    public class OrderService :IOrderService
    {
        ICartService _cartService;
        OurCartDBContext context;
        ICurrentDailyTransfterHeaderService _currentDailyTransfterHeaderService;
        ICurrentDailyTransfterDetailsService _currentDailyTransfterDetailsService;
        public OrderService(ICartService cartService, ICurrentDailyTransfterHeaderService currentDailyTransfterHeaderService, ICurrentDailyTransfterDetailsService currentDailyTransfterDetailsService, OurCartDBContext dBContext)
        {
            context = dBContext;
            _cartService = cartService;
            _currentDailyTransfterDetailsService = currentDailyTransfterDetailsService;
            _currentDailyTransfterHeaderService = currentDailyTransfterHeaderService;
        }



        public async Task<OperationResponse<IEnumerable<userCartItem>>> checkoutUserOrder(decimal delivertClientID)
        {
            OperationResponse<IEnumerable<userCartItem>> or = new OperationResponse<IEnumerable<userCartItem>>();
            ///get all user cart items
            ///and insert master and details data in POSCurrentDailyTransferHeaders, POSCurrentDailyTransferDetails
            /// clear user cart from db
            /// 

            try {
                #region Try Body
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                var StockNoItems = new List<userCartItem>();

                var userCartProducts = await context.userCartItem.FromSql($"GetCurrentDeliveryClientCertProducst {delivertClientID}").ToListAsync();
                // var cartResult = await _cartService.getAllCartProductsByUserID(delivertClientID);
                if (userCartProducts == null)
                    throw new Exception("No Items In cart ");

                #region check Cart Item Quantity in Stock 
                foreach (var item in userCartProducts)
                {
                    var ItemStockQuantity = context.Items.FirstOrDefault(i => i.ItemId == item.ItemId && i.FkCategoryId == item.fkCategoryId).MaxStockQuantity;
                    if ((item.quantity > ItemStockQuantity) || (ItemStockQuantity == 0) || (ItemStockQuantity == null))
                    {
                        StockNoItems.Add(item);
                    }
                }

                if (StockNoItems.Count > 0)
                {
                    or.HasErrors = false;
                    or.Data = StockNoItems;

                }
                #endregion
                else
                {
                    #region Add Order
                    // add order
                    var counter = 0;
                    decimal id = getNextHeaderID();
                    var addedHeader = await _currentDailyTransfterHeaderService.Add(
                                         new PoscurrentDailyTransHeader()
                                         {
                                             Total = (decimal?)(await _cartService.getTotalPrice(delivertClientID)).Data,
                                             FkTransTypeId = 1,
                                             FkBrId = 1,
                                             ClientId = delivertClientID,
                                             InsDate = DateTime.Now,
                                             TransDate = DateTimeUtility.getFormatFromDateTime(DateTime.Now),
                                             FkClientTypeId = 1,// عميل
                                             CashierName = "",
                                             HeaderId = id,
                                             FkInvoiceStatusId = 4,// old//,
                                             PoscurrentDailyTransDetails = userCartProducts.Select(

                                                 item => new PoscurrentDailyTransDetails()
                                                 {
                                                     FkBrId = 1,
                                                     DetailId = _currentDailyTransfterDetailsService.GetAll().Data.Max(d => d.HeaderId) + ++counter,
                                                     HeaderId = 0,
                                                     InsDate = DateTime.Now,
                                                     CustPrice = (decimal)item.CustomerPrice,
                                                     SalePrice = (decimal)item.ItemCost,
                                                     FkItemBarcodeId = (decimal)item.ItemBarCodeID,
                                                     FktransTypeId = 0,
                                                     ItemId = (decimal)item.ItemId,
                                                     ItemName = item.ItemName,
                                                     PackageName = item.PackageName,
                                                     Barcode = item.BarCode,
                                                     TransDate = DateTimeUtility.getFormatFromDateTime(DateTime.Now),


                                                 }).ToList()


                                         });

                    #endregion

                    #region update Stock Quantity
                    foreach (var item in userCartProducts)
                    {
                        foreach (var StockItem in context.Items)
                        {
                            StockItem.MaxStockQuantity = StockItem.MaxStockQuantity - item.quantity;
                        }

                    }
                    #endregion
                    #region Delete Cart

                    var CartId = userCartProducts.FirstOrDefault().Id;
                    var res = context.CartProducts.Find(CartId);
                    context.CartProducts.Remove(res);
                    #endregion
                    #region save changes
                    context.ChangeTracker.AutoDetectChangesEnabled = true;
                    var InsertedRows = context.SaveChanges();
                    if (InsertedRows > 0)
                    {
                        or.Data = null;
                        or.HasErrors = false;
                    }

                    #endregion




                    #endregion


                }
                return or;
            }

           

            catch (Exception ex)
            {
              
                or.HasErrors = true;
                or.Data = null;
                or.Message = ex.Message;
            }
          
            return or;
        }
        

        public  OperationResponse<IEnumerable<PoscurrentDailyTransHeader>> GetCurrentOrders(decimal delivertClientID)
        {
            OperationResponse<IEnumerable<PoscurrentDailyTransHeader>> or = new OperationResponse<IEnumerable<PoscurrentDailyTransHeader>>();
            try
            {
                //get headers with client id  and details with headerID
                // get customer headers of orders
                var result =  context.PoscurrentDailyTransHeader.Where(h => h.ClientId == delivertClientID && h.FkInvoiceStatusId == 1 /*new*/).Select(h => new PoscurrentDailyTransHeader() {
                   HeaderId = h.HeaderId,
                   InsDate = h.InsDate,
                   Total = h.Total,
                   SubTotal = h.SubTotal,
                   Discount = h.Discount,
                   DeliveryAmount = h.DeliveryAmount,
                   Notes =h.Notes,
                   ClientId = h.ClientId,
                   FkInvoiceStatusId = h.FkInvoiceStatusId,
                 PoscurrentDailyTransDetails=  h.PoscurrentDailyTransDetails.ToList()
                   


                });
               
               
                or.Data = result;

            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }
        public  OperationResponse<IEnumerable<PoscurrentDailyTransHeader>> GetHistoryOrders(decimal delivertClientID)
        {
            OperationResponse<IEnumerable<PoscurrentDailyTransHeader>> or = new OperationResponse<IEnumerable<PoscurrentDailyTransHeader>>();
            try
            {
                //get headers with client id  and details with headerID
                // get customer headers of orders
                var result =  context.PoscurrentDailyTransHeader.Where(h => h.ClientId == delivertClientID && h.FkInvoiceStatusId == 4 /*old*/).Select(h => new PoscurrentDailyTransHeader() {
                   HeaderId = h.HeaderId,
                   InsDate = h.InsDate,
                   Total = h.Total,
                   SubTotal = h.SubTotal,
                   Discount = h.Discount,
                    DeliveryAmount = h.DeliveryAmount,
                    Notes =h.Notes,
                    ClientId = h.ClientId,
                     FkInvoiceStatusId = h.FkInvoiceStatusId,
                     PoscurrentDailyTransDetails=  h.PoscurrentDailyTransDetails.ToList()
                   


                });
               
               
                or.Data = result;

            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }
        public  OperationResponse<bool> DeleteHistoryOrder(decimal UserId, decimal HeaderId)
        {
            OperationResponse<bool> or = new OperationResponse<bool>();

            try
            {
                if (UserId == 0 || HeaderId == 0)
                    throw new Exception("insert UserId Or HeaderId");
                //get headers with client id  and details with headerID
                // get customer headers of orders
                var order = context.PoscurrentDailyTransHeader.FirstOrDefault(i => i.ClientId == UserId && i.HeaderId== HeaderId);
                if (order != null)
                {
                    context.PoscurrentDailyTransHeader.Remove(order);
                    
                    int rowsEffected = context.SaveChanges();
                    or.HasErrors = rowsEffected > 0 ? false : true;
                    or.Data = true;
                }
                else
                {
                    throw new Exception("Order not found");
                }
              
            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
                or.Data = false;
            }
            return or;
        }

        //public async Task<OperationResponse<List<OrderReponseModel>>> GetHistoryOrders(decimal delivertClientID)
        //{

        //    OperationResponse<List<OrderReponseModel>> or = new OperationResponse<List<OrderReponseModel>>();
        //    try
        //    {
        //        //get headers with client id  and details with headerID
        //        // get customer headers of orders
        //        var headers = await context.PoscurrentDailyTransHeader.Where(h => h.ClientId == delivertClientID && h.FkInvoiceStatusId == 4 /*Paid*/).ToListAsync();
        //        List<OrderReponseModel> result = new List<OrderReponseModel>();
        //        foreach (var item in headers)
        //        {
        //            var detailsData = await context.PoscurrentDailyTransDetails.Where(d => d.HeaderId == item.HeaderId).ToListAsync();
        //            result.Add(new OrderReponseModel()
        //            {
        //                header = item,
        //                details = detailsData
        //            });
        //        }

        //        or.Data = result;

        //    }
        //    catch (Exception ex)
        //    {
        //        or.HasErrors = true;
        //        or.Message = ex.Message;
        //    }
        //    return or;

        //}

        public async Task<OperationResponse<bool>> reorder(decimal deliveryClientID, decimal HeaderID)
        {

            OperationResponse<bool> or = new OperationResponse<bool>();
            try
            {
                // get headers with client id  and details with headerID
                // get customer headers of orders
                var headerID = getNextHeaderID();
                var header = await context.PoscurrentDailyTransHeader.Where(h => h.HeaderId == HeaderID).FirstOrDefaultAsync();
                header.FkInvoiceStatusId = 4;//old;
                context.SaveChanges();
                or.Data = true;
            }
            catch (Exception ex)
            {
                or.Data = false;
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }

        private decimal getNextHeaderID()
        {
            return _currentDailyTransfterHeaderService.GetAll().Data.Max(h => h.HeaderId) + 1;
        }
    }
}