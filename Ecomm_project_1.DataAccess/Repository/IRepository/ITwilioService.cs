using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.DataAccess.Repository.IRepository
{
    public interface ITwilioService
    {
        Task SendOrderConfirmationSmsAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames);
        Task MakeOrderConfirmationCallAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames);
        Task SendOrderConfirmationWhatsAppAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames);
    }
}

