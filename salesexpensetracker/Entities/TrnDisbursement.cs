using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class TrnDisbursement
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
        public bool IsLocked { get; set; }
        public int CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDateTime { get; set; }
        public int UpdatedById { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
    }
}