using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.Utility
{
    public class EmailSettings                                                                                                                                                                                      
    {
        public string PrimaryDomain { get; set; }
        public int PrimaryPort { get; set; }
        public string SecondaryDomain { get; set; }
        public int SecondaryPort { get; set; }
        public string UserNameEmail { get; set; }
        public string UserNamePassword { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string CcEmail { get; set; }
    }
}
