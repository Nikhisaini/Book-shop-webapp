using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        public  OrderHeader OrderHeader { get; set; }
        public double SubTotal => ListCart?.Sum(c => c.Price * c.Count) ?? 0;

        public double Discount => Math.Round(SubTotal * 0.10, 2);
        public double DiscountedSubTotal => SubTotal - Discount;
        public double GstAmount => Math.Round(DiscountedSubTotal * 0.05, 2);
        public double GrandTotal => Math.Round(DiscountedSubTotal + GstAmount, 2);
        public IEnumerable<SharedCart> SharedCarts { get; set; }
    }
}
