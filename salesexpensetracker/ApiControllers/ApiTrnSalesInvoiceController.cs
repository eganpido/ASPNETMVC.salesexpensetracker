using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.UI;
using System.Web.Util;
using static iTextSharp.text.pdf.AcroFields;

namespace salesexpensetracker.ApiControllers
{
    public class ApiTrnSalesInvoiceController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Sales Invoice
        [Authorize, HttpGet, Route("api/salesInvoice/list")]
        public List<Entities.TrnSalesInvoice> ListSalesInvoice()
        {
            var list = db.TrnSalesInvoices
                        .OrderByDescending(d => d.Id)
                        .Select(d => new Entities.TrnSalesInvoice
                        {
                            Id = d.Id,
                            SalesNumber = d.SalesNumber,
                            SalesDate = d.SalesDate.ToShortDateString(), // handled in memory
                            ClientId = d.ClientId,
                            Client = d.MstClient.ClientName,
                            Remarks = d.Remarks,
                            SalesAmount = d.SalesAmount,
                            PaidAmount = d.PaidAmount,
                            BalanceAmount = d.BalanceAmount,
                            IsLocked = d.IsLocked,
                            CreatedById = d.CreatedById,
                            CreatedBy = d.MstUser.FullName,
                            CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                            UpdatedById = d.UpdatedById,
                            UpdatedBy = d.MstUser1.FullName,
                            UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                        })
                        .ToList();

            return list;
        }

        //// Detail Bank
        //[Authorize, HttpGet, Route("api/bank/detail/{id}")]
        //public Entities.MstBank DetailBank(String id)
        //{
        //    var bank = (from d in db.MstBanks
        //                where d.Id == Convert.ToInt32(id)
        //                select new
        //                {
        //                    d.Id,
        //                    d.BankCode,
        //                    d.Bank,
        //                    d.IsLocked,
        //                    d.CreatedById,
        //                    CreatedBy = d.MstUser != null ? d.MstUser.FullName : "",
        //                    d.CreatedDateTime,
        //                    d.UpdatedById,
        //                    UpdatedBy = d.MstUser1 != null ? d.MstUser1.FullName : "",
        //                    d.UpdatedDateTime
        //                }).FirstOrDefault();

        //    if (bank != null)
        //    {
        //        return new Entities.MstBank
        //        {
        //            Id = bank.Id,
        //            BankCode = bank.BankCode,
        //            Bank = bank.Bank,
        //            IsLocked = bank.IsLocked,
        //            CreatedById = bank.CreatedById,
        //            CreatedBy = bank.CreatedBy,
        //            CreatedDateTime = bank.CreatedDateTime.ToShortDateString(),
        //            UpdatedById = bank.UpdatedById,
        //            UpdatedBy = bank.UpdatedBy,
        //            UpdatedDateTime = bank.UpdatedDateTime.ToShortDateString()
        //        };
        //    }

        //    return null;

        //}

        //// Add Bank
        //[Authorize, HttpPost, Route("api/bank/add")]
        //public HttpResponseMessage AddBank()
        //{
        //    try
        //    {
        //        var currentUser = from d in db.MstUsers
        //                          where d.UserId == User.Identity.GetUserId()
        //                          select d;

        //        if (currentUser.Any())
        //        {
        //            var currentUserId = currentUser.FirstOrDefault().Id;

        //            Data.MstBank newBank = new Data.MstBank
        //            {
        //                BankCode = "NA",
        //                Bank = "NA",
        //                IsLocked = false,
        //                CreatedById = currentUserId,
        //                CreatedDateTime = DateTime.Now,
        //                UpdatedById = currentUserId,
        //                UpdatedDateTime = DateTime.Now,
        //            };

        //            db.MstBanks.InsertOnSubmit(newBank);
        //            db.SubmitChanges();

        //            return Request.CreateResponse(HttpStatusCode.OK, newBank.Id);
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
        //    }
        //}

        //// Save Bank
        //[Authorize, HttpPut, Route("api/bank/save/{id}")]
        //public HttpResponseMessage SaveBank(Entities.MstBank objBank, String id)
        //{
        //    try
        //    {
        //        var currentUser = from d in db.MstUsers
        //                          where d.UserId == User.Identity.GetUserId()
        //                          select d;

        //        if (currentUser.Any())
        //        {
        //            var currentUserId = currentUser.FirstOrDefault().Id;

        //            var bank = from d in db.MstBanks
        //                       where d.Id == Convert.ToInt32(id)
        //                       select d;

        //            if (bank.Any())
        //            {
        //                if (!bank.FirstOrDefault().IsLocked)
        //                {
        //                    var saveBank = bank.FirstOrDefault();
        //                    saveBank.BankCode = objBank.BankCode;
        //                    saveBank.Bank = objBank.Bank;
        //                    saveBank.UpdatedById = currentUserId;
        //                    saveBank.UpdatedDateTime = DateTime.Now;
        //                    db.SubmitChanges();

        //                    return Request.CreateResponse(HttpStatusCode.OK);
        //                }
        //                else
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These details are already locked.");
        //                }
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
        //    }
        //}

        //// Lock Bank
        //[Authorize, HttpPut, Route("api/bank/lock/{id}")]
        //public HttpResponseMessage LockBank(Entities.MstBank objBank, String id)
        //{
        //    try
        //    {
        //        var currentUser = from d in db.MstUsers
        //                          where d.UserId == User.Identity.GetUserId()
        //                          select d;

        //        if (currentUser.Any())
        //        {
        //            var currentUserId = currentUser.FirstOrDefault().Id;

        //            var bank = from d in db.MstBanks
        //                       where d.Id == Convert.ToInt32(id)
        //                       select d;

        //            if (bank.Any())
        //            {
        //                if (!bank.FirstOrDefault().IsLocked)
        //                {
        //                    var bankByCode = from d in db.MstBanks
        //                                     where d.BankCode.Equals(objBank.BankCode)
        //                                     && d.IsLocked == true
        //                                     select d;

        //                    if (!bankByCode.Any())
        //                    {
        //                        var lockBank = bank.FirstOrDefault();
        //                        lockBank.BankCode = objBank.BankCode;
        //                        lockBank.Bank = objBank.Bank;
        //                        lockBank.IsLocked = true;
        //                        lockBank.UpdatedById = currentUserId;
        //                        lockBank.UpdatedDateTime = DateTime.Now;

        //                        db.SubmitChanges();


        //                        return Request.CreateResponse(HttpStatusCode.OK);
        //                    }
        //                    else
        //                    {
        //                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Bank Code is already taken.");
        //                    }
        //                }
        //                else
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These details are already locked.");
        //                }
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
        //    }
        //}

        //// Unlock Bank
        //[Authorize, HttpPut, Route("api/bank/unlock/{id}")]
        //public HttpResponseMessage UnlockBank(String id)
        //{
        //    try
        //    {
        //        var currentUser = from d in db.MstUsers
        //                          where d.UserId == User.Identity.GetUserId()
        //                          select d;

        //        if (currentUser.Any())
        //        {
        //            var currentUserId = currentUser.FirstOrDefault().Id;

        //            var bank = from d in db.MstBanks
        //                           where d.Id == Convert.ToInt32(id)
        //                           select d;

        //            if (bank.Any())
        //            {
        //                if (bank.FirstOrDefault().IsLocked)
        //                {
        //                    var unlockBank = bank.FirstOrDefault();
        //                    unlockBank.IsLocked = false;
        //                    unlockBank.UpdatedById = currentUserId;
        //                    unlockBank.UpdatedDateTime = DateTime.Now;

        //                    db.SubmitChanges();

        //                    return Request.CreateResponse(HttpStatusCode.OK);
        //                }
        //                else
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These details are already unlocked.");
        //                }
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
        //    }
        //}

        //// Delete Bank
        //[Authorize, HttpDelete, Route("api/bank/delete/{id}")]
        //public HttpResponseMessage DeleteBank(String id)
        //{
        //    try
        //    {
        //        var currentUser = from d in db.MstUsers
        //                          where d.UserId == User.Identity.GetUserId()
        //                          select d;

        //        if (currentUser.Any())
        //        {
        //            var currentUserId = currentUser.FirstOrDefault().Id;

        //            var bank = from d in db.MstBanks
        //                           where d.Id == Convert.ToInt32(id)
        //                           select d;

        //            if (bank.Any())
        //            {
        //                db.MstBanks.DeleteOnSubmit(bank.First());

        //                db.SubmitChanges();

        //                return Request.CreateResponse(HttpStatusCode.OK);
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected record is not found in the server.");
        //            }
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e);
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
        //    }
        //}
    }
}
