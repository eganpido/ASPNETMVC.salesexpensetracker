using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class TrnSalesInvoiceLine
    {
        public int Id { get; set; }
        public int SalesId { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}