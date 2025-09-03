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
    public class ApiRepSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Sales Invoice Summary
        [Authorize, HttpGet, Route("api/salesInvoiceReport/salesSummaryReport/{fromDate}/{toDate}")]
        public List<Entities.RepSalesInvoiceSummary> ListSalesSummary(string fromDate, string toDate)
        {
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var list = db.TrnSalesInvoices
                    .AsNoTracking()
                    .Where(d => d.SalesDate >= from && d.SalesDate <= to && d.IsLocked == true)
                    .OrderByDescending(d => d.Id)
                    .Select(d => new Entities.RepSalesInvoiceSummary
                    {
                        Id = d.Id,
                        SalesNumber = d.SalesNumber,
                        SalesDate = d.SalesDate.ToShortDateString(),
                        ClientId = d.ClientId,
                        Client = d.MstClient.ClientName,
                        Remarks = d.Remarks,
                        SalesAmount = d.SalesAmount,
                        PaidAmount = d.PaidAmount,
                        BalanceAmount = d.BalanceAmount,
                        IsLocked = d.IsLocked,
                        CreatedBy = d.MstUser.FullName,
                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                        UpdatedBy = d.MstUser1.FullName,
                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                    })
                    .ToList();

            return list;

        }
        // List Sales Detail
        [Authorize, HttpGet, Route("api/salesInvoiceReport/salesDetailReport/{fromDate}/{toDate}")]
        public List<Entities.RepSalesInvoiceDetail> ListSalesDetail(string fromDate, string toDate)
        {
            // ✅ safer parsing
            DateTime from = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime to = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var list = db.TrnSalesInvoiceLines
                .AsNoTracking()
                .Where(d => d.TrnSalesInvoice != null &&
                            d.TrnSalesInvoice.SalesDate >= from &&
                            d.TrnSalesInvoice.SalesDate <= to &&
                            d.TrnSalesInvoice.IsLocked == true)
                .OrderByDescending(d => d.Id)
                .Select(d => new
                {
                    d.SalesId,
                    d.TrnSalesInvoice.SalesNumber,
                    d.TrnSalesInvoice.SalesDate,
                    ClientName = d.TrnSalesInvoice.MstClient.ClientName,
                    d.ProductId,
                    ProductDescription = d.MstProduct.ProductDescription,
                    d.Price,
                    d.Quantity,
                    d.Amount
                })
                .AsEnumerable() // ✅ switch to LINQ-to-Objects, safe to format
                .Select(d => new Entities.RepSalesInvoiceDetail
                {
                    SalesId = d.SalesId,
                    SalesNumber = d.SalesNumber,
                    SalesDate = d.SalesDate.ToShortDateString(),
                    Client = d.ClientName,
                    ProductId = d.ProductId,
                    Product = d.ProductDescription,
                    Price = d.Price,
                    Quantity = d.Quantity,
                    Amount = d.Amount
                })
                .ToList();

            return list;
        }
        //// Dropdown List Client
        //[Authorize, HttpGet, Route("api/salesInvoice/list/client")]
        //public List<Entities.MstClient> DropdownListClient()
        //{
        //    var list = db.MstClients
        //                .AsNoTracking()
        //                .Where(d => d.IsLocked)
        //                .OrderBy(d => d.ClientName)
        //                .Select(d => new
        //                {
        //                    d.Id,
        //                    d.ClientName
        //                })
        //                .ToList()
        //                .Select(d => new Entities.MstClient
        //                {
        //                    Id = d.Id,
        //                    ClientName = d.ClientName,
        //                })
        //                .ToList();

        //    return list;
        //}
    }
}
