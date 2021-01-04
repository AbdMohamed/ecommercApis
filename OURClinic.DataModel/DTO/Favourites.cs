namespace OURCart.DataModel.DTO
{
    public partial class Favourites
    {
        public int Id { get; set; }
        public decimal FkItemId { get; set; }   
        public decimal FkItemPackageId { get; set; }
        public decimal FkDeliveryClientId { get; set; }
        public string InsDateTime { get; set; }

        public virtual ItemsPackages FkItem { get; set; }
    }
}
