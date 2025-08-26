using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace salesexpensetracker.Controllers
{
    public class AccountsReceivable
    {
        // Data Context
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // Update Accounts Receivable
        public void UpdateAccountsReceivable(Int32 SalesId)
        {
            var salesInvoice = from d in db.TrnSalesInvoices where d.Id == SalesId select d;
            if (salesInvoice.Any())
            {
                Decimal paidAmount = 0;

                var collectionLines = from d in db.TrnCollectionLines where d.SalesId == SalesId && d.TrnCollection.IsLocked == true select d;
                if (collectionLines.Any())
                {
                    paidAmount = collectionLines.Sum(d => d.Amount);
                }

                Decimal salesInvoiceAmount = salesInvoice.FirstOrDefault().SalesAmount;
                Decimal balanceAmount = salesInvoiceAmount - paidAmount;

                var updateSalesInvoice = salesInvoice.FirstOrDefault();
                updateSalesInvoice.PaidAmount = paidAmount;
                updateSalesInvoice.BalanceAmount = balanceAmount;

                db.SubmitChanges();
            }
        }
    }
}