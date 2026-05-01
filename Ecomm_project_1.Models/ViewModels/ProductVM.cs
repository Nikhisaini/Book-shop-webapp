using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.Models.ViewModels
{
    public class ProductVM
    {
        public IEnumerable<SelectListItem> CategoryLIst { get; set; }
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }
        public Product product { get; set; }
    }
}
