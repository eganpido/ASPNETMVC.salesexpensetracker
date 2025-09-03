using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace salesexpensetracker.ApiControllers
{
    public class ApiRepCashFlowController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Cash Flow
        [Authorize, HttpGet, Route("api/cashFlowReport/cashFlow/{fromDate}/{toDate}")]
        public List<Entities.RepCashFlow> ListCashFlow(string fromDate, string toDate)
        {
            DateTime dateFrom = DateTime.Parse(fromDate);
            DateTime dateTo = DateTime.Parse(toDate);

            var cashFlow = db.TrnCollections
                .Where(c => c.CollectionDate >= dateFrom && c.CollectionDate <= dateTo && c.IsLocked)
                .Select(c => new Entities.RepCashFlow
                {
                    TransactionNumber = "CR-" + c.CollectionNumber,
                    TransactionDate = c.CollectionDate,
                    Particulars = c.MstClient.ClientName + " | " + c.Remarks,
                    TransactionAmount = c.CollectionAmount
                })
                .Concat(
                    db.TrnDisbursements
                      .Where(d => d.DisbursementDate >= dateFrom && d.DisbursementDate <= dateTo && d.IsLocked)
                      .Select(d => new Entities.RepCashFlow
                      {
                          TransactionNumber = "CV-" + d.DisbursementNumber,
                          TransactionDate = d.DisbursementDate,
                          Particulars = d.MstSupplier.SupplierName + " | " + d.Remarks,
                          TransactionAmount = d.DisbursementAmount * -1
                      })
                )
                .OrderBy(r => r.TransactionDate)
                .ToList();

            // Compute running total
            decimal runningTotal = 0;
            foreach (var item in cashFlow)
            {
                runningTotal += item.TransactionAmount;
                item.RunningTotal = runningTotal;
            }

            return cashFlow;
        }


    }
}
