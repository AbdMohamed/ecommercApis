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
using static OURCart.Infrastructure.Util.Enums;

namespace OURCart.Infrastructure.Services
{
    public class CategoryService : BaseRepository<ItemCategory>, ICategoryService
    {

        public CategoryService(OurCartDBContext _dbContext) : base(_dbContext)
        {

        }

        public async Task<OperationResponse<itemsResponseData>> getAllProductsInCategory(int catID, int pageNum, int itemsPerPage,int PriceFilter,int NameFilter,string SearchText)
        {
            OperationResponse<itemsResponseData> or = new OperationResponse<itemsResponseData>();
            try
            {
                //  parameters --> category id to filter with, all items count to skip , all items that will return
                
                var allItmesCount = await _dbContext.itemsCountInCategory.FromSql($"getItemsCountInCategory {catID}").FirstOrDefaultAsync();
                var allItemsIncategory = await _dbContext.CategorItem.FromSql("GetAllItemsInCategory @p0,@p1,@p2,@p3,@p4,@p5",catID, (pageNum*itemsPerPage), itemsPerPage, PriceFilter,NameFilter,SearchText).ToListAsync();
                if (allItemsIncategory != null)
                    or.Data = new itemsResponseData() { itemsCount = allItmesCount.itemsCount, allItemsData = allItemsIncategory };
                // old data
                //var result = await _dbContext.Items.Where(i => i.FkCategoryId == catID).ToListAsync();
                //if (result != null)
                //    or.Data = result;
            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }
        public async Task<OperationResponse<offersResponseData>> getAllOffersInCategory(int catID, int pageNum, int itemsPerPage)
        {
            OperationResponse<offersResponseData> or = new OperationResponse<offersResponseData>();
            try
            {
                //  parameters --> category id to filter with, all items count to skip , all items that will return
                
                var allItmesCount = await _dbContext.itemsCountInCategory.FromSql($"getItemsCountInCategory {catID}").FirstOrDefaultAsync();
                var allItemsIncategory = await _dbContext.CategoryOffersDisplayItem.FromSql($"GetAllItemsInCategoryOffers {catID},{(pageNum * itemsPerPage)}, {itemsPerPage}").ToListAsync();
                if (allItemsIncategory != null)
                    or.Data = new offersResponseData() { itemsCount = allItmesCount.itemsCount, allItemsData = allItemsIncategory };
                // old data
                //var result = await _dbContext.Items.Where(i => i.FkCategoryId == catID).ToListAsync();
                //if (result != null)
                //    or.Data = result;
            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }
        public async Task<OperationResponse<IEnumerable<Items>>> SearchByItemBarcode(string SearchText)
        {
            OperationResponse<IEnumerable<Items>> or = new OperationResponse<IEnumerable<Items>>();
            try
            {
                var ItemList = (from ItemBarcode in _dbContext.ItemBarCode
                                where ItemBarcode.FkItem.ItemNameEn==SearchText || ItemBarcode.FkItem.ItemName==SearchText|| ItemBarcode.BarCode == SearchText
                                select ItemBarcode.FkItem);
                or.Data = ItemList;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                or.HasErrors = true;
                or.Message = msg;
            }
            return or;
        }

        public async Task<OperationResponse<IEnumerable<Items>>> SearchByItemName(string Name)
        {
            OperationResponse<IEnumerable<Items>> or = new OperationResponse<IEnumerable<Items>>();
            try
            {
                var ItemList = (from Item in _dbContext.Items
                                where Item.ItemName == Name || Item.ItemNameEn==Name
                                select Item);
                or.Data = ItemList;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                or.HasErrors = true;
                or.Message = msg;
            }
            return or;
        }

        public async Task<OperationResponse<CategoryItemsDisplayItem>> getProductDataByItemIDandPAckagedID(int itemID, decimal? itemPackagedID)
        {
            OperationResponse<CategoryItemsDisplayItem> or = new OperationResponse<CategoryItemsDisplayItem>();
            try
            {
                //  parameters --> category id to filter with, all items count to skip , all items that will return
                var allItemsIncategory = await _dbContext.CategoryItemsDisplayItem.FromSql($"GetAllItemsInCategoryByID {itemID},{itemPackagedID}").FirstOrDefaultAsync();

                if (allItemsIncategory != null)
                    or.Data = allItemsIncategory;
            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }

        /// <summary>
        /// get all parent categories or sub categories in ItemCategory table 
        /// </summary>
        /// <param name="parentCatID">
        /// if null will get first level of categories tree 
        /// else will get sub categories under this category id
        /// </param>
        /// <returns></returns>
        public async Task<OperationResponse<IEnumerable<CategoryModel>>> GetCategories(int? parentCatID)
        {
            OperationResponse<IEnumerable<CategoryModel>> or = new OperationResponse<IEnumerable<CategoryModel>>();
            try
            {
                var result =
                    (from c in _dbContext.ItemCategory
                     where c.ParentId == parentCatID
                     select new CategoryModel
                     {
                         CategoryId = c.CategoryId,
                         CategoryName = c.CategoryName,
                         CategoryNameEng = c.CategoryNameEng,
                         FkShopId = c.FkShopId,
                         InsDate = c.InsDate,
                         InsUserId = c.InsUserId,
                         ParentId = c.ParentId,
                         RecId = c.RecId,
                         UpdDate = c.UpdDate,
                         UpdUserId = c.UpdUserId,
                         HasSubCategories = _dbContext.ItemCategory.Any(item => item.ParentId == c.CategoryId)
                     });

                if (result != null)
                    or.Data = result;
            }
            catch (Exception ex)
            {
                or.HasErrors = true;
                or.Message = ex.Message;
            }
            return or;
        }
    }
}