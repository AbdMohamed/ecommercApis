﻿using System;
using System.ComponentModel.DataAnnotations;

namespace OURCart.DataModel.DTO.LocalModels
{
    public class CategoryItemsDisplayItem
    {
        [Key]
        public decimal ItemId { get; set; }
        public int fkCategoryId { get; set; }
        public decimal? PurchaseDiscount { get; set; }
        public string Notes { get; set; }
        public string ItemName { get; set; }
   

        public string ItemCode { get; set; }
        public decimal? ItemCost { get; set; }
        public decimal? CustomerPrice { get; set; }
        public Int16 PackageId { get; set; }
        public decimal ItemPackageID { get; set; }
        public string PackageName { get; set; }
        public string PackageNameEn { get; set; }
        public byte PackageSize { get; set; }
        public decimal ItemBarCodeID { get; set; }
        public string BarCode { get; set; }
        public string BarCodeFormat { get; set; }
        
    }

    public class CategoryItem
    {
        [Key]
        public decimal ItemId { get; set; }
        public int fkCategoryId { get; set; }
        public decimal? PurchaseDiscount { get; set; }
        public string Notes { get; set; }
        public string ItemName { get; set; }
        public string ItemNameEn { get; set; }

        public string ItemCode { get; set; }
        public decimal? ItemCost { get; set; }
        public decimal? CustomerPrice { get; set; }
        public Int16 PackageId { get; set; }
        public decimal ItemPackageID { get; set; }
        public string PackageName { get; set; }
        public string PackageNameEn { get; set; }
        public byte PackageSize { get; set; }
        public decimal ItemBarCodeID { get; set; }
        public string BarCode { get; set; }
        public string BarCodeFormat { get; set; }

    }
}
