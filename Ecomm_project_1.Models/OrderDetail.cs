using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Ecomm_project_1.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderHeaderId { get; set; }
        [ForeignKey("OrderHeaderId")]
        public OrderHeader orderHeader { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product product { get; set; }
        public int Count { get; set; }
        public double Price { get; set; }
    }
}
