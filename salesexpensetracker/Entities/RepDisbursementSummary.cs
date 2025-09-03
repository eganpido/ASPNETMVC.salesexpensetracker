using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class RepDisbursementSummary
    {
        public int Id { get; set; }
        public string DisbursementNumber { get; set; }
        public string DisbursementDate { get; set; }
        public int SupplierId { get; set; }
        public string Supplier { get; set; }
        public string Remarks { get; set; }
        public decimal DisbursementAmount { get; set; }
        public int PayTypeId { get; set; }
        public string PayType { get; set; }
        public string CheckNumber { get; set; }
        public string CheckDate { get; set; }
        public int BankId { get; set; }
        public string Bank { get; set; }
    }
}