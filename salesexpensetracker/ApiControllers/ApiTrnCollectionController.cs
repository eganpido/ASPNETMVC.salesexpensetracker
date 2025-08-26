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
    public class ApiTrnCollectionController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Collection
        [Authorize, HttpGet, Route("api/collection/list/{fromDate}/{toDate}")]
        public List<Entities.TrnCollection> List(string fromDate, string toDate)
        {
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var list = db.TrnCollections
                    .AsNoTracking()
                    .Where(d => d.CollectionDate >= from && d.CollectionDate <= to)
                    .OrderByDescending(d => d.Id)
                    .Select(d => new Entities.TrnCollection
                    {
                        Id = d.Id,
                        CollectionNumber = d.CollectionNumber,
                        CollectionDate = d.CollectionDate.ToShortDateString(),
                        ClientId = d.ClientId,
                        Client = d.MstClient.ClientName,
                        Remarks = d.Remarks,
                        CollectionAmount = d.CollectionAmount,
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
        [Authorize, HttpGet, Route("api/collection/list/client")]
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
        // Detail Collection
        [Authorize, HttpGet, Route("api/collection/detail/{id}")]
        public Entities.TrnCollection Detail(String id)
        {
            int collectionId = Convert.ToInt32(id);

            var detail = db.TrnCollections
                        .AsNoTracking()
                        .Where(d => d.Id == collectionId)
                        .Select(d => new Entities.TrnCollection
                        {
                            Id = d.Id,
                            CollectionNumber = d.CollectionNumber,
                            CollectionDate = d.CollectionDate.ToShortDateString(),
                            ClientId = d.ClientId,
                            Client = d.MstClient.ClientName,
                            Remarks = d.Remarks,
                            CollectionAmount = d.CollectionAmount,
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

        // Add Collection
        [Authorize, HttpPost, Route("api/collection/add")]
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

                var lastRecord = db.TrnCollections
                                   .OrderByDescending(d => d.Id)
                                   .FirstOrDefault();

                if (lastRecord != null)
                {
                    int number = Convert.ToInt32(lastRecord.CollectionNumber) + 1;
                    defaultNumber = FillLeadingZeroes(number, 10);
                }

                var newRecord = new Data.TrnCollection
                {
                    CollectionNumber = defaultNumber,
                    CollectionDate = DateTime.Today,
                    ClientId = 1,
                    Remarks = "NA",
                    CollectionAmount = 0,
                    IsLocked = false,
                    CreatedById = currentUser.Id,
                    CreatedDateTime = DateTime.Now,
                    UpdatedById = currentUser.Id,
                    UpdatedDateTime = DateTime.Now
                };

                db.TrnCollections.InsertOnSubmit(newRecord);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK, newRecord.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Save Collection
        [Authorize, HttpPut, Route("api/collection/save/{id}")]
        public HttpResponseMessage Save(Entities.TrnCollection detail, String id)
        {
            try
            {
                int collectionId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnCollections.FirstOrDefault(d => d.Id == collectionId);
                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These order details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These order details are already locked.");
                }

                record.CollectionDate = Convert.ToDateTime(detail.CollectionDate);
                record.ClientId = detail.ClientId;
                record.Remarks = detail.Remarks;
                record.CollectionAmount = detail.CollectionAmount;
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

        // Lock Collection
        [Authorize, HttpPut, Route("api/collection/lock/{id}")]
        public HttpResponseMessage Lock(Entities.TrnCollection detail, String id)
        {
            try
            {
                int collectionId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers.FirstOrDefault(d => d.UserId == currentUserName);
                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnCollections.FirstOrDefault(d => d.Id == collectionId);
                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. This record is already locked.");
                }

                record.CollectionDate = Convert.ToDateTime(detail.CollectionDate);
                record.ClientId = detail.ClientId;
                record.Remarks = detail.Remarks;
                record.CollectionAmount = detail.CollectionAmount;
                record.IsLocked = true;
                record.UpdatedById = currentUser.Id;
                record.UpdatedDateTime = DateTime.Now;

                db.SubmitChanges();

                Controllers.AccountsReceivable accountsReceivable = new Controllers.AccountsReceivable();
                var salesIds = db.TrnCollectionLines
                             .Where(d => d.CollectionId == collectionId)
                             .Select(d => d.SalesId)
                             .ToList();

                foreach (var salesId in salesIds)
                {
                    accountsReceivable.UpdateAccountsReceivable(salesId);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Unlock Collection
        [Authorize, HttpPut, Route("api/collection/unlock/{id}")]
        public HttpResponseMessage Unlock(String id)
        {
            try
            {
                int collectionId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnCollections.FirstOrDefault(d => d.Id == collectionId);

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

                Controllers.AccountsReceivable accountsReceivable = new Controllers.AccountsReceivable();
                var salesIds = db.TrnCollectionLines
                             .Where(d => d.CollectionId == collectionId)
                             .Select(d => d.SalesId)
                             .ToList();

                foreach (var salesId in salesIds)
                {
                    accountsReceivable.UpdateAccountsReceivable(salesId);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Delete Collection
        [Authorize, HttpDelete, Route("api/collection/delete/{id}")]
        public HttpResponseMessage DeleteOrder(String id)
        {
            try
            {
                int collectionId = Convert.ToInt32(id);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var record = db.TrnCollections
                    .FirstOrDefault(d => d.Id == collectionId);

                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                }

                if (record.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Delete Error. You cannot delete this record because it is locked.");
                }

                db.TrnCollections.DeleteOnSubmit(record);
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
