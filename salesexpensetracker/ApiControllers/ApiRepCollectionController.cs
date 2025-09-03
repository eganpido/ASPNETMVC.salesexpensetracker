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
    public class ApiRepCollectionController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Collection Summary
        [Authorize, HttpGet, Route("api/collectionReport/collectionSummaryReport/{fromDate}/{toDate}")]
        public List<Entities.RepCollectionSummary> ListCollectionSummary(string fromDate, string toDate)
        {
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var list = db.TrnCollections
                    .AsNoTracking()
                    .Where(d => d.CollectionDate >= from && d.CollectionDate <= to && d.IsLocked == true)
                    .OrderByDescending(d => d.Id)
                    .Select(d => new Entities.RepCollectionSummary
                    {
                        Id = d.Id,
                        CollectionNumber = d.CollectionNumber,
                        CollectionDate = d.CollectionDate.ToShortDateString(),
                        ClientId = d.ClientId,
                        Client = d.MstClient.ClientName,
                        Remarks = d.Remarks,
                        CollectionAmount = d.CollectionAmount
                    })
                    .ToList();

            return list;

        }
        // List Collection Detail
        [Authorize, HttpGet, Route("api/collectionReport/collectionDetailReport/{fromDate}/{toDate}")]
        public List<Entities.RepCollectionDetail> ListCollectionDetail(string fromDate, string toDate)
        {
            DateTime from = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime to = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var list = db.TrnCollectionLines
            .AsNoTracking()
            .Where(d => d.TrnCollection != null &&
                        d.TrnCollection.CollectionDate >= from &&
                        d.TrnCollection.CollectionDate <= to &&
                        d.TrnCollection.IsLocked == true)
            .OrderByDescending(d => d.Id)
            .Select(d => new
            {
                d.CollectionId,
                d.TrnCollection.CollectionNumber,
                d.TrnCollection.CollectionDate,
                ClientName = d.TrnSalesInvoice.MstClient.ClientName,
                SalesNumber = d.TrnSalesInvoice.SalesNumber,
                d.PayTypeId,
                PayType = d.MstPayType.PayType,
                d.CheckNumber,
                d.CheckDate,   // ✅ keep as DateTime? (or DateTime?) — no formatting here
                d.MstBank.BankCode,
                d.Amount
            })
            .AsEnumerable()   // switch to client-side
            .Select(d => new Entities.RepCollectionDetail
            {
                CollectionId = d.CollectionId,
                CollectionNumber = d.CollectionNumber,
                CollectionDate = d.CollectionDate.ToShortDateString(),
                Client = d.ClientName,
                SalesNumber = d.SalesNumber,
                PayTypeId = d.PayTypeId,
                PayType = d.PayType,
                CheckNumber = d.CheckNumber,
                CheckDate = d.CheckNumber != "NA"
                            ? d.CheckDate.ToShortDateString()
                            : string.Empty,
                Bank = d.BankCode,
                Amount = d.Amount
            })
            .ToList();


            return list;
        }
    }
}
