using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ecomm_project_1.Models
{
    public class Catagory
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
