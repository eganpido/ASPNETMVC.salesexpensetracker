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
    public class ApiTrnDisbursementController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Disbursement
        [Authorize, HttpGet, Route("api/disbursement/list/{fromDate}/{toDate}")]
        public List<Entities.TrnDisbursement> List(string fromDate, string toDate)
        {
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var list = db.TrnDisbursements
                    .AsNoTracking()
                    .Where(d => d.DisbursementDate >= from && d.DisbursementDate <= to)
                    .OrderByDescending(d => d.Id)
                    .Select(d => new Entities.TrnDisbursement
                    {
                        Id = d.Id,
                        DisbursementNumber = d.DisbursementNumber,
                        DisbursementDate = d.DisbursementDate.ToShortDateString(),
                        SupplierId = d.SupplierId,
                        Supplier = d.MstSupplier.SupplierName,
                        Remarks = d.Remarks,
                        DisbursementAmount = d.DisbursementAmount,
                        PayTypeId = d.PayTypeId,
                        PayType = d.MstPayType.PayType,
                        CheckNumber = d.CheckNumber,
                        CheckDate = d.CheckDate.ToShortDateString(),
                        BankId = d.BankId,
                        Bank = d.MstBank.Bank,
                        IsLocked = d.IsLocked,
                        CreatedBy = d.MstUser.FullName,
                        CreatedDateTime = d.CreatedDateTime.ToShortDateString(),
                        UpdatedBy = d.MstUser1.FullName,
                        UpdatedDateTime = d.UpdatedDateTime.ToShortDateString()
                    })
                    .ToList();

            return list;

        }
        // Dropdown List Supplier
        [Authorize, HttpGet, Route("api/disbursement/list/supplier")]
        public List<Entities.MstSupplier> DropdownListSupplier()
        {
            var list = db.MstSuppliers
                        .AsNoTracking()
                        .Where(d => d.IsLocked)
                        .OrderBy(d => d.SupplierName)
                        .Select(d => new
                        {
                            d.Id,
                            d.SupplierName
                        })
                        .ToList()
                        .Select(d => new Entities.MstSupplier
                        {
                            Id = d.Id,
                            SupplierName = d.SupplierName,
                        })
                        .ToList();

            return list;
        }
        // Dropdown List Pay Type
        [Authorize, HttpGet, Route("api/disbursement/list/paytype")]
        public List<Entities.MstPayType> DropdownListPayType()
        {
            var list = db.MstPayTypes
                        .AsNoTracking()
                        .Where(d => d.IsLocked)
                        .OrderBy(d => d.PayType)
                        .Select(d => new
                        {
                            d.Id,
                            d.PayType
                        })
                        .ToList()
                        .Select(d => new Entities.MstPayType
                        {
                            Id = d.Id,
                            PayType = d.PayType,
                        })
                        .ToList();

            return list;
        }
        // Dropdown List Bank
        [Authorize, HttpGet, Route("api/disbursement/list/bank")]
        public List<Entities.MstBank> DropdownListBank()
        {
            var list = db.MstBanks
                        .AsNoTracking()
                        .Where(d => d.IsLocked)
                        .OrderBy(d => d.Bank)
                        .Select(d => new
                        {
                            d.Id,
                            d.Bank
                        })
                        .ToList()
                        .Select(d => new Entities.MstBank
                        {
                            Id = d.Id,
                            Bank = d.Bank,
                        })
                        .ToList();

            return list;
        }
        // Detail Disbursement
        [Authorize, HttpGet, Route("api/disbursement/detail/{id}")]
        public Entities.TrnDisbursement Detail(String id)
        {
            int invoiceId = Convert.ToInt32(id);

            var detail = db.TrnDisbursements
                        .AsNoTracking()
                        .Where(d => d.Id == invoiceId)
                        .Select(d => new Entities.TrnDisbursement
                        {
                            Id = d.Id,
                            DisbursementNumber = d.DisbursementNumber,
                            DisbursementDate = d.DisbursementDate.ToShortDateString(),
                            SupplierId = d.SupplierId,
                            Supplier = d.MstSupplier.SupplierName,
                            Remarks = d.Remarks,
                            DisbursementAmount = d.DisbursementAmount,
                            PayTypeId = d.PayTypeId,
                            PayType = d.MstPayType.PayType,
                            CheckNumber = d.CheckNumber,
                            CheckDate = d.CheckDate.ToShortDateString(),
                            BankId = d.BankId,
                            Bank = d.MstBank.Bank,
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

        // Add Disbursement
        [Authorize, HttpPost, Route("api/disbursement/add")]
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

                string defaultNumber = "0000000001";

                var lastRecord = db.TrnDisbursements
                                   .OrderByDescending(d => d.Id)
                                   .FirstOrDefault();

                if (lastRecord != null)
                {
                    int number = Convert.ToInt32(lastRecord.DisbursementNumber) + 1;
                    defaultNumber = FillLeadingZeroes(number, 10);
                }

                var newRecord = new Data.TrnDisbursement
                {
                    DisbursementNumber = defaultNumber,
                    DisbursementDate = DateTime.Today,
                    SupplierId = 1,
                    Remarks = "NA",
                    DisbursementAmount = 0,
                    PayTypeId = 1,
                    CheckNumber = "NA",
                    CheckDate = DateTime.Today,
                    BankId = 1,
                    IsLocked = false,
                    CreatedById = currentUser.Id,
                    CreatedDateTime = DateTime.Now,
                    UpdatedById = currentUser.Id,
                    UpdatedDateTime = DateTime.Now
                };

                db.TrnDisbursements.InsertOnSubmit(newRecord);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK, newRecord.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Save Disbursement
        [Authorize, HttpPut, Route("api/disbursement/save/{id}")]
        public HttpResponseMessage Save(Entities.TrnDisbursement detail, String id)
        {
            try
            {
                int disbursementId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnDisbursements.FirstOrDefault(d => d.Id == disbursementId);
                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These order details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These order details are already locked.");
                }

                record.DisbursementDate = Convert.ToDateTime(detail.DisbursementDate);
                record.SupplierId = detail.SupplierId;
                record.Remarks = detail.Remarks;
                record.DisbursementAmount = detail.DisbursementAmount;
                record.PayTypeId = detail.PayTypeId;
                record.CheckNumber = detail.CheckNumber;
                record.CheckDate = Convert.ToDateTime(detail.CheckDate);
                record.BankId = detail.BankId;
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

        // Lock Disbursement
        [Authorize, HttpPut, Route("api/disbursement/lock/{id}")]
        public HttpResponseMessage Lock(Entities.TrnDisbursement detail, String id)
        {
            try
            {
                int disbursementId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnDisbursements.FirstOrDefault(d => d.Id == disbursementId);
                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. This record is already locked.");
                }

                record.DisbursementDate = Convert.ToDateTime(detail.DisbursementDate);
                record.SupplierId = detail.SupplierId;
                record.Remarks = detail.Remarks;
                record.DisbursementAmount = detail.DisbursementAmount;
                record.PayTypeId = detail.PayTypeId;
                record.CheckNumber = detail.CheckNumber;
                record.CheckDate = Convert.ToDateTime(detail.CheckDate);
                record.BankId = detail.BankId;
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

        // Unlock Disbursement
        [Authorize, HttpPut, Route("api/disbursement/unlock/{id}")]
        public HttpResponseMessage Unlock(String id)
        {
            try
            {
                int disbursementId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnDisbursements.FirstOrDefault(d => d.Id == disbursementId);

                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (!record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. This record is already unlocked.");
                }

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

        // Delete Disbursement
        [Authorize, HttpDelete, Route("api/disbursement/delete/{id}")]
        public HttpResponseMessage DeleteOrder(String id)
        {
            try
            {
                int disbursementId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnDisbursements
                    .FirstOrDefault(d => d.Id == disbursementId);

                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete this record because it is locked.");
                }

                db.TrnDisbursements.DeleteOnSubmit(record);
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
