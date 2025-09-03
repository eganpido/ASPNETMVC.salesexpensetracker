using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class RepDisbursementDetail
    {
        public int DisbursementId { get; set; }
        public string DisbursementNumber { get; set; }
        public string DisbursementDate { get; set; }
        public int SupplierId { get; set; }
        public string Supplier { get; set; }
        public int ExpenseId { get; set; }
        public string Expense { get; set; }
        public string Particulars { get; set; }
        public decimal Amount { get; set; }
    }
}