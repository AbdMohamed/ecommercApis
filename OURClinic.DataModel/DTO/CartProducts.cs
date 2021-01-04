using System;

namespace OURCart.DataModel.DTO
{

    /// <summary>
    ///  ItemId required    /// </summary>
    public partial class CartProducts
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public bool IsNew { get; set; }
        public decimal? FkItemPackageId { get; set; }
        public decimal FkTemId { get; set; }
        public DateTime? InsertDateTime { get; set; }
        //2019-06-22 20:33:00 used to bind date time in flutter
        public String InsertDateTimeFormat { get { return InsertDateTime?.ToString("yyyy-MM-dd HH:mm:ss"); } }
        public decimal? fk_itemBarCodeID { get; set; }
        public decimal FkDeliveryClientId { get; set; }
        public virtual Items item { get; set; }
        //public virtual ItemsPackages Fk { get; set; }
        //public virtual DeliveryClient FkDeliveryClient { get; set; }
    }
}
