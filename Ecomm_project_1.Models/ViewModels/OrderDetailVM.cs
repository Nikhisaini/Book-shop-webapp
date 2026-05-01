using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.Models.ViewModels
{
    public class OrderDetailVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }
}
