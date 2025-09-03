using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class RepCollectionSummary
    {
        public int Id { get; set; }
        public string CollectionNumber { get; set; }
        public string CollectionDate { get; set; }
        public int ClientId { get; set; }
        public string Client { get; set; }
        public string Remarks { get; set; }
        public decimal CollectionAmount { get; set; }
    }
}