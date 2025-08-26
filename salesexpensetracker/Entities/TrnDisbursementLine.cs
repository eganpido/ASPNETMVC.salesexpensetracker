using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class TrnDisbursementLine
    {
        public int Id { get; set; }
        public int DisbursementId { get; set; }
        public int ExpenseId { get; set; }
        public string Expense { get; set; }
        public string Particulars { get; set; }
        public decimal Amount { get; set; }
    }
}