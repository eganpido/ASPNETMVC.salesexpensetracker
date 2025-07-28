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
    public class ApiMstClientController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Clients
        [Authorize, HttpGet, Route("api/client/list")]
        public List<Entities.MstClient> ListClient()
        {
            var rawClients = from d in db.MstClients.OrderByDescending(d => d.ClientCode)
                              select new
                              {
                                  Id = d.Id,
                                  ClientCode = d.ClientCode,
                                  ClientName = d.ClientName,
                                  ClientAddress = d.ClientAddress,
                                  ContactNumber = d.ContactNumber,
                                  ContactPerson = d.ContactPerson,
                                  IsLocked = d.IsLocked,
                                  CreatedById = d.CreatedById,
                                  CreatedBy = d.MstUser.FullName,
                                  CreatedDateTime = d.CreatedDateTime, // Keep as DateTime
                                  UpdatedById = d.UpdatedById,
                                  UpdatedBy = d.MstUser1.FullName,
                                  UpdatedDateTime = d.UpdatedDateTime, // Keep as DateTime
                              };

            // Step 2: Materialize and format in memory
            var clients = rawClients.ToList() // Execute query on database
                                      .Select(d => new Entities.MstClient
                                      {
                                          Id = d.Id,
                                          ClientCode = d.ClientCode,
                                          ClientName = d.ClientName,
                                          ClientAddress = d.ClientAddress,
                                          ContactNumber = d.ContactNumber,
                                          ContactPerson = d.ContactPerson,
                                          IsLocked = d.IsLocked,
                                          CreatedById = d.CreatedById,
                                          CreatedBy = d.CreatedBy,
                                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(), // Format in memory
                                          UpdatedById = d.UpdatedById,
                                          UpdatedBy = d.UpdatedBy,
                                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(), // Format in memory
                                      })
                                      .ToList();

            return clients;
        }

        // Detail Clients
        [Authorize, HttpGet, Route("api/client/detail/{id}")]
        public Entities.MstClient DetailClient(String id)
        {
            var client = (from d in db.MstClients
                           where d.Id == Convert.ToInt32(id)
                           select new
                           {
                               d.Id,
                               d.ClientCode,
                               d.ClientName,
                               d.ClientAddress,
                               d.ContactNumber,
                               d.ContactPerson,
                               d.IsLocked,
                               d.CreatedById,
                               CreatedBy = d.MstUser != null ? d.MstUser.FullName : "",
                               d.CreatedDateTime,
                               d.UpdatedById,
                               UpdatedBy = d.MstUser1 != null ? d.MstUser1.FullName : "",
                               d.UpdatedDateTime
                           }).FirstOrDefault();

            if (client != null)
            {
                return new Entities.MstClient
                {
                    Id = client.Id,
                    ClientCode = client.ClientCode,
                    ClientName = client.ClientName,
                    ClientAddress = client.ClientAddress,
                    ContactNumber = client.ContactNumber,
                    ContactPerson = client.ContactPerson,
                    IsLocked = client.IsLocked,
                    CreatedById = client.CreatedById,
                    CreatedBy = client.CreatedBy,
                    CreatedDateTime = client.CreatedDateTime.ToShortDateString(),
                    UpdatedById = client.UpdatedById,
                    UpdatedBy = client.UpdatedBy,
                    UpdatedDateTime = client.UpdatedDateTime.ToShortDateString()
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

        // Add Client
        [Authorize, HttpPost, Route("api/client/add")]
        public HttpResponseMessage AddClient()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var defaultClientCode = "0001";
                    var lastClient = from d in db.MstClients.OrderByDescending(d => d.Id)
                                   select d;

                    if (lastClient.Any())
                    {
                        var clientCode = Convert.ToInt32(lastClient.FirstOrDefault().ClientCode) + 0001;
                        defaultClientCode = FillLeadingZeroes(clientCode, 4);
                    }

                    Data.MstClient newClient = new Data.MstClient
                    {
                        ClientCode = defaultClientCode,
                        ClientName = "NA",
                        ClientAddress = "NA",
                        ContactNumber = "NA",
                        ContactPerson = "NA",
                        IsLocked = false,
                        CreatedById = currentUserId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUserId,
                        UpdatedDateTime = DateTime.Now,
                    };

                    db.MstClients.InsertOnSubmit(newClient);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newClient.Id);
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

        // Save Client
        [Authorize, HttpPut, Route("api/client/save/{id}")]
        public HttpResponseMessage SaveClient(Entities.MstClient objClient, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var client = from d in db.MstClients
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (client.Any())
                    {
                        if (!client.FirstOrDefault().IsLocked)
                        {
                            var saveClient = client.FirstOrDefault();
                            saveClient.ClientCode = objClient.ClientCode;
                            saveClient.ClientName = objClient.ClientName;
                            saveClient.ClientAddress = objClient.ClientAddress;
                            saveClient.ContactNumber = objClient.ContactNumber;
                            saveClient.ContactPerson = objClient.ContactPerson;
                            saveClient.UpdatedById = currentUserId;
                            saveClient.UpdatedDateTime = DateTime.Now;
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

        // Lock Client
        [Authorize, HttpPut, Route("api/client/lock/{id}")]
        public HttpResponseMessage LockClient(Entities.MstClient objClient, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var client = from d in db.MstClients
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (client.Any())
                    {
                        if (!client.FirstOrDefault().IsLocked)
                        {
                            var clientByCode = from d in db.MstClients
                                                   where d.ClientCode.Equals(objClient.ClientCode)
                                                   && d.IsLocked == true
                                                   select d;

                            if (!clientByCode.Any())
                            {
                                var lockClient = client.FirstOrDefault();
                                lockClient.ClientCode = objClient.ClientCode;
                                lockClient.ClientName = objClient.ClientName;
                                lockClient.ClientAddress = objClient.ClientAddress;
                                lockClient.ContactNumber = objClient.ContactNumber;
                                lockClient.ContactPerson = objClient.ContactPerson;
                                lockClient.IsLocked = true;
                                lockClient.UpdatedById = currentUserId;
                                lockClient.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();


                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Client Code is already taken.");
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

        // Unlock Client
        [Authorize, HttpPut, Route("api/client/unlock/{id}")]
        public HttpResponseMessage UnlockClient(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var client = from d in db.MstClients
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (client.Any())
                    {
                        if (client.FirstOrDefault().IsLocked)
                        {
                            var unlockClient = client.FirstOrDefault();
                            unlockClient.IsLocked = false;
                            unlockClient.UpdatedById = currentUserId;
                            unlockClient.UpdatedDateTime = DateTime.Now;

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

        // Delete Client
        [Authorize, HttpDelete, Route("api/client/delete/{id}")]
        public HttpResponseMessage DeleteClient(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var client = from d in db.MstClients
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (client.Any())
                    {
                        db.MstClients.DeleteOnSubmit(client.First());

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
