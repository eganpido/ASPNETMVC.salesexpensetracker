using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Entities
{
    public class MstExpense
    {
        public Int32 Id { get; set; }
        public String ExpenseCode { get; set; }
        public String ExpenseName { get; set; }
        public Boolean IsLocked { get; set; }
        public Int32 CreatedById { get; set; }
        public String CreatedBy { get; set; }
        public String CreatedDateTime { get; set; }
        public Int32 UpdatedById { get; set; }
        public String UpdatedBy { get; set; }
        public String UpdatedDateTime { get; set; }
    }
}