using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.DataAccess
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
