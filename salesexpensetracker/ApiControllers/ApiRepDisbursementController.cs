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
    public class ApiRepDisbursementController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Disbursement Summary
        [Authorize, HttpGet, Route("api/disbursementReport/disbursementSummaryReport/{fromDate}/{toDate}")]
        public List<Entities.RepDisbursementSummary> ListDisbursementSummary(string fromDate, string toDate)
        {
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var list = db.TrnDisbursements
                        .AsNoTracking()
                        .Where(d => d.DisbursementDate >= from && d.DisbursementDate <= to && d.IsLocked == true)
                        .OrderByDescending(d => d.Id)
                        .Select(d => new
                        {
                            d.Id,
                            d.DisbursementNumber,
                            d.DisbursementDate,
                            d.SupplierId,
                            Supplier = d.MstSupplier.SupplierName,
                            d.Remarks,
                            d.PayTypeId,
                            PayType = d.MstPayType.PayType,
                            d.CheckNumber,
                            d.CheckDate,
                            d.BankId,
                            Bank = d.MstBank.BankCode,
                            d.DisbursementAmount
                        })
                        .AsEnumerable() // switch to client-side
                        .Select(d => new Entities.RepDisbursementSummary
                        {
                            Id = d.Id,
                            DisbursementNumber = d.DisbursementNumber,
                            DisbursementDate = d.DisbursementDate.ToShortDateString(),
                            SupplierId = d.SupplierId,
                            Supplier = d.Supplier,
                            Remarks = d.Remarks,
                            PayTypeId = d.PayTypeId,
                            PayType = d.PayType,
                            CheckNumber = d.CheckNumber,
                            CheckDate = d.CheckNumber != "NA"
                                ? d.CheckDate.ToShortDateString()
                                : string.Empty,
                            BankId = d.BankId,
                            Bank = d.Bank,
                            DisbursementAmount = d.DisbursementAmount,
                        })
                        .ToList();


            return list;

        }
        // List Disbursement Detail
        [Authorize, HttpGet, Route("api/disbursementReport/disbursementDetailReport/{fromDate}/{toDate}")]
        public List<Entities.RepDisbursementDetail> ListDisbursementDetail(string fromDate, string toDate)
        {
            DateTime from = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime to = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var list = db.TrnDisbursementLines
                .AsNoTracking()
                .Where(d => d.TrnDisbursement != null &&
                            d.TrnDisbursement.DisbursementDate >= from &&
                            d.TrnDisbursement.DisbursementDate <= to &&
                            d.TrnDisbursement.IsLocked == true)
                .OrderByDescending(d => d.Id)
                .Select(d => new
                {
                    d.DisbursementId,
                    d.TrnDisbursement.DisbursementNumber,
                    d.TrnDisbursement.DisbursementDate,
                    SupplierName = d.TrnDisbursement.MstSupplier.SupplierName,
                    d.ExpenseId,
                    ExpenseName = d.MstExpense.ExpenseName,
                    d.Particulars,
                    d.Amount
                })
                .AsEnumerable()
                .Select(d => new Entities.RepDisbursementDetail
                {
                    DisbursementId = d.DisbursementId,
                    DisbursementNumber = d.DisbursementNumber,
                    DisbursementDate = d.DisbursementDate.ToShortDateString(),
                    Supplier = d.SupplierName,
                    Expense = d.ExpenseName,
                    Particulars = d.Particulars,
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
