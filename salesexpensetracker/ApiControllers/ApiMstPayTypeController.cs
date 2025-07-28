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
    public class ApiMstPayTypeController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List PayTypes
        [Authorize, HttpGet, Route("api/paytype/list")]
        public List<Entities.MstPayType> ListPayType()
        {
            var rawPayTypes = from d in db.MstPayTypes.OrderByDescending(d => d.Id)
                              select new
                              {
                                  Id = d.Id,
                                  PayTypeCode = d.PayTypeCode,
                                  PayType = d.PayType,
                                  IsLocked = d.IsLocked,
                                  CreatedById = d.CreatedById,
                                  CreatedBy = d.MstUser.FullName,
                                  CreatedDateTime = d.CreatedDateTime, // Keep as DateTime
                                  UpdatedById = d.UpdatedById,
                                  UpdatedBy = d.MstUser1.FullName,
                                  UpdatedDateTime = d.UpdatedDateTime, // Keep as DateTime
                              };

            // Step 2: Materialize and format in memory
            var payTypes = rawPayTypes.ToList() // Execute query on database
                                      .Select(d => new Entities.MstPayType
                                      {
                                          Id = d.Id,
                                          PayTypeCode = d.PayTypeCode,
                                          PayType = d.PayType,
                                          IsLocked = d.IsLocked,
                                          CreatedById = d.CreatedById,
                                          CreatedBy = d.CreatedBy,
                                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(), // Format in memory
                                          UpdatedById = d.UpdatedById,
                                          UpdatedBy = d.UpdatedBy,
                                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(), // Format in memory
                                      })
                                      .ToList();

            return payTypes;
        }

        // Detail PayType
        [Authorize, HttpGet, Route("api/paytype/detail/{id}")]
        public Entities.MstPayType DetailPayType(String id)
        {
            var payType = (from d in db.MstPayTypes
                           where d.Id == Convert.ToInt32(id)
                           select new
                           {
                               d.Id,
                               d.PayTypeCode,
                               d.PayType,
                               d.IsLocked,
                               d.CreatedById,
                               CreatedBy = d.MstUser != null ? d.MstUser.FullName : "",
                               d.CreatedDateTime,
                               d.UpdatedById,
                               UpdatedBy = d.MstUser1 != null ? d.MstUser1.FullName : "",
                               d.UpdatedDateTime
                           }).FirstOrDefault();

            if (payType != null)
            {
                return new Entities.MstPayType
                {
                    Id = payType.Id,
                    PayTypeCode = payType.PayTypeCode,
                    PayType = payType.PayType,
                    IsLocked = payType.IsLocked,
                    CreatedById = payType.CreatedById,
                    CreatedBy = payType.CreatedBy,
                    CreatedDateTime = payType.CreatedDateTime.ToShortDateString(),
                    UpdatedById = payType.UpdatedById,
                    UpdatedBy = payType.UpdatedBy,
                    UpdatedDateTime = payType.UpdatedDateTime.ToShortDateString()
                };
            }

            return null;

        }

        // Add PayType
        [Authorize, HttpPost, Route("api/paytype/add")]
        public HttpResponseMessage AddPayType()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    Data.MstPayType newPayType = new Data.MstPayType
                    {
                        PayTypeCode = "NA",
                        PayType = "NA",
                        IsLocked = false,
                        CreatedById = currentUserId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUserId,
                        UpdatedDateTime = DateTime.Now,
                    };

                    db.MstPayTypes.InsertOnSubmit(newPayType);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newPayType.Id);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Save PayType
        [Authorize, HttpPut, Route("api/paytype/save/{id}")]
        public HttpResponseMessage SavePayType(Entities.MstPayType objPayType, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var payType = from d in db.MstPayTypes
                                  where d.Id == Convert.ToInt32(id)
                                  select d;

                    if (payType.Any())
                    {
                        if (!payType.FirstOrDefault().IsLocked)
                        {
                            var savePayType = payType.FirstOrDefault();
                            savePayType.PayTypeCode = objPayType.PayTypeCode;
                            savePayType.PayType = objPayType.PayType;
                            savePayType.UpdatedById = currentUserId;
                            savePayType.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Lock PayType
        [Authorize, HttpPut, Route("api/paytype/lock/{id}")]
        public HttpResponseMessage LockPayType(Entities.MstPayType objPayType, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var payType = from d in db.MstPayTypes
                                  where d.Id == Convert.ToInt32(id)
                                  select d;

                    if (payType.Any())
                    {
                        if (!payType.FirstOrDefault().IsLocked)
                        {
                            var payTypeByCode = from d in db.MstPayTypes
                                                where d.PayTypeCode.Equals(objPayType.PayTypeCode)
                                             && d.IsLocked == true
                                                select d;

                            if (!payTypeByCode.Any())
                            {
                                var lockPayType = payType.FirstOrDefault();
                                lockPayType.PayTypeCode = objPayType.PayTypeCode;
                                lockPayType.PayType = objPayType.PayType;
                                lockPayType.IsLocked = true;
                                lockPayType.UpdatedById = currentUserId;
                                lockPayType.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();


                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Supplier Code is already taken.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Unlock PayType
        [Authorize, HttpPut, Route("api/paytype/unlock/{id}")]
        public HttpResponseMessage UnlockPayType(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var payType = from d in db.MstPayTypes
                                  where d.Id == Convert.ToInt32(id)
                                  select d;

                    if (payType.Any())
                    {
                        if (payType.FirstOrDefault().IsLocked)
                        {
                            var unlockPayType = payType.FirstOrDefault();
                            unlockPayType.IsLocked = false;
                            unlockPayType.UpdatedById = currentUserId;
                            unlockPayType.UpdatedDateTime = DateTime.Now;

                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These details are already unlocked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These details are not found in the server.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Delete PayType
        [Authorize, HttpDelete, Route("api/paytype/delete/{id}")]
        public HttpResponseMessage DeletePayType(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var payType = from d in db.MstPayTypes
                                  where d.Id == Convert.ToInt32(id)
                                  select d;

                    if (payType.Any())
                    {
                        db.MstPayTypes.DeleteOnSubmit(payType.First());

                        db.SubmitChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected record is not found in the server.");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Theres no current user logged in.");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }
    }
}
