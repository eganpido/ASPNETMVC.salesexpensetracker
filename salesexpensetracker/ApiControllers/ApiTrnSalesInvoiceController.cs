using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace salesexpensetracker.ApiControllers
{
    public class ApiTrnSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Sales Invoice
        [Authorize, HttpGet, Route("api/salesInvoice/list/{fromDate}/{toDate}")]
        public List<Entities.TrnSalesInvoice> List(string fromDate, string toDate)
        {
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var list = db.TrnSalesInvoices
                    .AsNoTracking()
                    .Where(d => d.SalesDate >= from && d.SalesDate <= to)
                    .OrderByDescending(d => d.Id)
                    .Select(d => new Entities.TrnSalesInvoice
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
        // Dropdown List Client
        [Authorize, HttpGet, Route("api/salesInvoice/list/client")]
        public List<Entities.MstClient> DropdownListClient()
        {
            var list = db.MstClients
                        .AsNoTracking()
                        .Where(d => d.IsLocked)
                        .OrderBy(d => d.ClientName)
                        .Select(d => new
                        {
                            d.Id,
                            d.ClientName
                        })
                        .ToList()
                        .Select(d => new Entities.MstClient
                        {
                            Id = d.Id,
                            ClientName = d.ClientName,
                        })
                        .ToList();

            return list;
        }
        // Detail Sales Invoice
        [Authorize, HttpGet, Route("api/salesInvoice/detail/{id}")]
        public Entities.TrnSalesInvoice Detail(String id)
        {
            int invoiceId = Convert.ToInt32(id);

            var detail = db.TrnSalesInvoices
                        .AsNoTracking()
                        .Where(d => d.Id == invoiceId)
                        .Select(d => new Entities.TrnSalesInvoice
                        {
                            Id = d.Id,
                            SalesNumber = d.SalesNumber,
                            // Better to keep as DateTime, format in UI
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
                        .FirstOrDefault();

            return detail;
        }

        // Fill Leading Zeroes
        public String FillLeadingZeroes(Int32 number, Int32 length)
        {
            var result = number.ToString();
            var pad = length - result.Length;
            while (pad > 0)
            {
                result = '0' + result;
                pad--;
            }

            return result;
        }

        // Add Sales Invoice
        [Authorize, HttpPost, Route("api/salesInvoice/add")]
        public HttpResponseMessage Add()
        {
            try
            {
                string currentUserName = User.Identity.GetUserId();
                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                // Default sales number
                string defaultNumber = "0000000001";

                // Get last record once
                var lastRecord = db.TrnSalesInvoices
                                   .OrderByDescending(d => d.Id)
                                   .FirstOrDefault();

                if (lastRecord != null)
                {
                    int number = Convert.ToInt32(lastRecord.SalesNumber) + 1;
                    defaultNumber = FillLeadingZeroes(number, 10);
                }

                // Build new invoice
                var newRecord = new Data.TrnSalesInvoice
                {
                    SalesNumber = defaultNumber,
                    SalesDate = DateTime.Today,
                    ClientId = 1,
                    Remarks = "NA",
                    SalesAmount = 0,
                    PaidAmount = 0,
                    BalanceAmount = 0,
                    IsLocked = false,
                    CreatedById = currentUser.Id,
                    CreatedDateTime = DateTime.Now,
                    UpdatedById = currentUser.Id,
                    UpdatedDateTime = DateTime.Now
                };

                db.TrnSalesInvoices.InsertOnSubmit(newRecord);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK, newRecord.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Save Sales Invoice
        [Authorize, HttpPut, Route("api/salesInvoice/save/{id}")]
        public HttpResponseMessage Save(Entities.TrnSalesInvoice detail, String id)
        {
            try
            {
                int invoiceId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                // Get current user once
                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                // Get record once
                var record = db.TrnSalesInvoices.FirstOrDefault(d => d.Id == invoiceId);
                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These order details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These order details are already locked.");
                }

                // Update record
                record.SalesDate = Convert.ToDateTime(detail.SalesDate);
                record.ClientId = detail.ClientId;
                record.Remarks = detail.Remarks;
                record.SalesAmount = detail.SalesAmount;
                record.UpdatedById = currentUser.Id;
                record.UpdatedDateTime = DateTime.Now;

                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Lock Sales Invoice
        [Authorize, HttpPut, Route("api/salesInvoice/lock/{id}")]
        public HttpResponseMessage Lock(Entities.TrnSalesInvoice detail, String id)
        {
            try
            {
                int invoiceId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                // Get current user once
                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                // Get record once
                var record = db.TrnSalesInvoices.FirstOrDefault(d => d.Id == invoiceId);
                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. This record is already locked.");
                }

                // Update and lock record
                record.SalesDate = Convert.ToDateTime(detail.SalesDate);
                record.ClientId = detail.ClientId;
                record.Remarks = detail.Remarks;
                record.SalesAmount = detail.SalesAmount;
                record.IsLocked = true;
                record.UpdatedById = currentUser.Id;
                record.UpdatedDateTime = DateTime.Now;

                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Unlock Sales Invoice
        [Authorize, HttpPut, Route("api/salesInvoice/unlock/{id}")]
        public HttpResponseMessage Unlock(String id)
        {
            try
            {
                int invoiceId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                // Get current user once
                var currentUser = db.MstUsers
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                // Get record once
                var record = db.TrnSalesInvoices.FirstOrDefault(d => d.Id == invoiceId);

                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (!record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. This record is already unlocked.");
                }

                // Unlock record
                record.IsLocked = false;
                record.UpdatedById = currentUser.Id;
                record.UpdatedDateTime = DateTime.Now;

                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Delete Sales Invoice
        [Authorize, HttpDelete, Route("api/salesInvoice/delete/{id}")]
        public HttpResponseMessage DeleteOrder(String id)
        {
            try
            {
                int invoiceId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                // Get current user once
                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                // Get record once
                var record = db.TrnSalesInvoices
                    .FirstOrDefault(d => d.Id == invoiceId);

                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete this record because it is locked.");
                }

                // Delete record
                db.TrnSalesInvoices.DeleteOnSubmit(record);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
