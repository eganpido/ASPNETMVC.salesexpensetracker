using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class TrnCollection
    {
        public int Id { get; set; }
        public string CollectionNumber { get; set; }
        public string CollectionDate { get; set; }
        public int ClientId { get; set; }
        public string Client { get; set; }
        public string Remarks { get; set; }
        public decimal CollectionAmount { get; set; }
        public bool IsLocked { get; set; }
        public int CreatedById { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDateTime { get; set; }
        public int UpdatedById { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
    }
}