using System;
using System.Collections.Generic;
using System.Text;

namespace Ecomm_project_1.Utility
{
    public class SD
    {
        //roles
        public const string Role_Admin = "Admin User";
        public const string Role_Employee = "Employee User";
        public const string Role_Company = "Company User";
        public const string Role_Individual = "Individual User";
        //Session
        public const string Ss_CartSessionCount = "Cart Count Session";
        //
        public static double GetPriceBasedOnOuentity(double count, double price,double price50,double price100)
        {
            if (count < 50)
                return price;
            else
                if (count < 100)
                    return price50;
            return price100;
        }
        //order Status
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProgress = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStatusRefunded = "Refunded";
        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayPayment = "PaymentStatusDelay";
        public const string PaymentStatusRejected = "Rejected";
    }
}
