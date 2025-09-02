using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class RepSalesInvoiceDetail
    {
        public int SalesId { get; set; }
        public string SalesNumber { get; set; }
        public string SalesDate { get; set; }
        public string Client { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}