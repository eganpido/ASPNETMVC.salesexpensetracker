using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class MstClient
    {
        public Int32 Id { get; set; }
        public String ClientCode { get; set; }
        public String ClientName { get; set; }
        public String ClientAddress { get; set; }
        public String ContactNumber { get; set; }
        public String ContactPerson { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedById { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedById { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
    }
}