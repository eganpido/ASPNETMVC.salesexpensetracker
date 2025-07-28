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
    public class ApiMstSupplierController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Suppliers
        [Authorize, HttpGet, Route("api/supplier/list")]
        public List<Entities.MstSupplier> ListSupplier()
        {
            var rawSuppliers = from d in db.MstSuppliers.OrderByDescending(d => d.SupplierCode)
                               select new
                               {
                                   Id = d.Id,
                                   SupplierCode = d.SupplierCode,
                                   SupplierName = d.SupplierName,
                                   IsLocked = d.IsLocked,
                                   CreatedById = d.CreatedById,
                                   CreatedBy = d.MstUser.FullName,
                                   CreatedDateTime = d.CreatedDateTime, // Keep as DateTime
                                   UpdatedById = d.UpdatedById,
                                   UpdatedBy = d.MstUser1.FullName,
                                   UpdatedDateTime = d.UpdatedDateTime, // Keep as DateTime
                               };

            // Step 2: Materialize and format in memory
            var suppliers = rawSuppliers.ToList() // Execute query on database
                                      .Select(d => new Entities.MstSupplier
                                      {
                                          Id = d.Id,
                                          SupplierCode = d.SupplierCode,
                                          SupplierName = d.SupplierName,
                                          IsLocked = d.IsLocked,
                                          CreatedById = d.CreatedById,
                                          CreatedBy = d.CreatedBy,
                                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(), // Format in memory
                                          UpdatedById = d.UpdatedById,
                                          UpdatedBy = d.UpdatedBy,
                                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(), // Format in memory
                                      })
                                      .ToList();

            return suppliers;
        }

        // Detail Supplier
        [Authorize, HttpGet, Route("api/supplier/detail/{id}")]
        public Entities.MstSupplier DetailSupplier(String id)
        {
            var supplier = (from d in db.MstSuppliers
                            where d.Id == Convert.ToInt32(id)
                            select new
                            {
                                d.Id,
                                d.SupplierCode,
                                d.SupplierName,
                                d.IsLocked,
                                d.CreatedById,
                                CreatedBy = d.MstUser != null ? d.MstUser.FullName : "",
                                d.CreatedDateTime,
                                d.UpdatedById,
                                UpdatedBy = d.MstUser1 != null ? d.MstUser1.FullName : "",
                                d.UpdatedDateTime
                            }).FirstOrDefault();

            if (supplier != null)
            {
                return new Entities.MstSupplier
                {
                    Id = supplier.Id,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    IsLocked = supplier.IsLocked,
                    CreatedById = supplier.CreatedById,
                    CreatedBy = supplier.CreatedBy,
                    CreatedDateTime = supplier.CreatedDateTime.ToShortDateString(),
                    UpdatedById = supplier.UpdatedById,
                    UpdatedBy = supplier.UpdatedBy,
                    UpdatedDateTime = supplier.UpdatedDateTime.ToShortDateString()
                };
            }

            return null;

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

        // Add Supplier
        [Authorize, HttpPost, Route("api/supplier/add")]
        public HttpResponseMessage AddSupplier()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var defaultSupplierCode = "001";
                    var lastSupplier = from d in db.MstSuppliers.OrderByDescending(d => d.Id)
                                       select d;

                    if (lastSupplier.Any())
                    {
                        var supplierCode = Convert.ToInt32(lastSupplier.FirstOrDefault().SupplierCode) + 001;
                        defaultSupplierCode = FillLeadingZeroes(supplierCode, 3);
                    }

                    Data.MstSupplier newSupplier = new Data.MstSupplier
                    {
                        SupplierCode = defaultSupplierCode,
                        SupplierName = "NA",
                        IsLocked = false,
                        CreatedById = currentUserId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUserId,
                        UpdatedDateTime = DateTime.Now,
                    };

                    db.MstSuppliers.InsertOnSubmit(newSupplier);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newSupplier.Id);
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

        // Save Supplier
        [Authorize, HttpPut, Route("api/supplier/save/{id}")]
        public HttpResponseMessage SaveSupplier(Entities.MstSupplier objSupplier, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var supplier = from d in db.MstSuppliers
                                   where d.Id == Convert.ToInt32(id)
                                   select d;

                    if (supplier.Any())
                    {
                        if (!supplier.FirstOrDefault().IsLocked)
                        {
                            var saveSupplier = supplier.FirstOrDefault();
                            saveSupplier.SupplierCode = objSupplier.SupplierCode;
                            saveSupplier.SupplierName = objSupplier.SupplierName;
                            saveSupplier.UpdatedById = currentUserId;
                            saveSupplier.UpdatedDateTime = DateTime.Now;
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

        // Lock Supplier
        [Authorize, HttpPut, Route("api/supplier/lock/{id}")]
        public HttpResponseMessage LockSupplier(Entities.MstSupplier objSupplier, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var supplier = from d in db.MstSuppliers
                                   where d.Id == Convert.ToInt32(id)
                                   select d;

                    if (supplier.Any())
                    {
                        if (!supplier.FirstOrDefault().IsLocked)
                        {
                            var supplierByCode = from d in db.MstSuppliers
                                                 where d.SupplierCode.Equals(objSupplier.SupplierCode)
                                                 && d.IsLocked == true
                                                 select d;

                            if (!supplierByCode.Any())
                            {
                                var lockSupplier = supplier.FirstOrDefault();
                                lockSupplier.SupplierCode = objSupplier.SupplierCode;
                                lockSupplier.SupplierName = objSupplier.SupplierName;
                                lockSupplier.IsLocked = true;
                                lockSupplier.UpdatedById = currentUserId;
                                lockSupplier.UpdatedDateTime = DateTime.Now;

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

        // Unlock Supplier
        [Authorize, HttpPut, Route("api/supplier/unlock/{id}")]
        public HttpResponseMessage UnlockSupplier(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var supplier = from d in db.MstSuppliers
                                   where d.Id == Convert.ToInt32(id)
                                   select d;

                    if (supplier.Any())
                    {
                        if (supplier.FirstOrDefault().IsLocked)
                        {
                            var unlockSupplier = supplier.FirstOrDefault();
                            unlockSupplier.IsLocked = false;
                            unlockSupplier.UpdatedById = currentUserId;
                            unlockSupplier.UpdatedDateTime = DateTime.Now;

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

        // Delete Supplier
        [Authorize, HttpDelete, Route("api/supplier/delete/{id}")]
        public HttpResponseMessage DeleteSupplier(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var supplier = from d in db.MstSuppliers
                                  where d.Id == Convert.ToInt32(id)
                                  select d;

                    if (supplier.Any())
                    {
                        db.MstSuppliers.DeleteOnSubmit(supplier.First());

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
