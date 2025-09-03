using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class RepCollectionDetail
    {
        public int CollectionId { get; set; }
        public string CollectionNumber { get; set; }
        public string CollectionDate { get; set; }
        public string Client { get; set; }
        public int SalesId { get; set; }
        public string SalesNumber { get; set; }
        public int PayTypeId { get; set; }
        public string PayType { get; set; }
        public int BankId { get; set; }
        public string Bank { get; set; }
        public string CheckNumber { get; set; }
        public string CheckDate { get; set; }
        public decimal Amount { get; set; }
    }
}