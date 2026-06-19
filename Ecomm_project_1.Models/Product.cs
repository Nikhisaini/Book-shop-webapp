using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecomm_project_1.Models
{
    public class Product
    {

        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        [Range(1, 1000)]
        public double ListPrice { get; set; }//600
        [Required]
        [Range(1, 1000)]
        public double Price { get; set; }//550
        [Required]
        [Range(1, 1000)]
        public double Price50 { get; set; }//500
        [Required]
        [Range(1, 1000)]
        public double Price100 { get; set; }//450
        [Display(Name = "ImageUrl")]
        public string ImageUrl { get; set; }
        [Required]
        [Display(Name = "Category")]
        public int CatagoryID { get; set; }
        public Catagory catagory { get; set; }
        [Required]
        [Display(Name = "CoverType")]
        public int CoverTypeID { get; set; }
        public CoverType coverType { get; set; }
        public int SalesCount { get; set; } = 0;

        // 2. Admin override to force a book to be a bestseller
        public bool IsBestseller { get; set; } = false;
    }
}
