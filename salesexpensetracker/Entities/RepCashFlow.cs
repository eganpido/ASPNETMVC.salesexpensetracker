using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class RepCashFlow
    {
        public string TransactionNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Particulars { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal RunningTotal { get; set; }
    }
}