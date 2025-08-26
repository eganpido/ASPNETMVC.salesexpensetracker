using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace salesexpensetracker.ApiControllers
{
    public class ApiTrnCollectionLineController : ApiController
    {
        // Data Context
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Collection Line
        [Authorize, HttpGet, Route("api/collectionLine/list/{collectionId}")]
        public List<Entities.TrnCollectionLine> ListLine(int collectionId)
        {
            var listLines = db.TrnCollectionLines
                .AsNoTracking()
                .Where(d => d.CollectionId == collectionId)
                .Select(d => new Entities.TrnCollectionLine
                {
                    Id = d.Id,
                    CollectionId = d.CollectionId,
                    SalesId = d.SalesId,
                    SalesNumber = d.TrnSalesInvoice.SalesNumber,
                    PayTypeId = d.PayTypeId,
                    PayType = d.MstPayType.PayType,
                    BankId = d.BankId,
                    Bank = d.MstBank.Bank,
                    CheckNumber = d.CheckNumber,
                    CheckDate = d.CheckDate.ToShortDateString(),
                    Amount = d.Amount
                })
                .ToList();

            return listLines;
        }

        // Dropdown List Sales Invoice
        [Authorize, HttpGet, Route("api/collectionLine/list/salesInvoice/{clientId}")]
        public List<Entities.TrnSalesInvoice> DropdownListSalesInvoice(string clientId)
        {
            var invoices = db.TrnSalesInvoices
                .AsNoTracking()
                .Where(d => d.IsLocked && d.ClientId == Convert.ToInt32(clientId));
            return invoices
                .OrderBy(d => d.Id)
                .Select(d => new Entities.TrnSalesInvoice
                {
                    Id = d.Id,
                    SalesNumber = d.SalesNumber,
                    SalesDate = d.SalesDate.ToShortDateString(),
                    BalanceAmount = d.BalanceAmount
                })
                .ToList();
        }
        // Dropdown List Pay Type
        [Authorize, HttpGet, Route("api/collectionLine/list/payType")]
        public List<Entities.MstPayType> DropdownListPayType()
        {
            var payTypes = db.MstPayTypes
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

            return payTypes;
        }

        // Dropdown List Bank
        [Authorize, HttpGet, Route("api/collectionLine/list/bank")]
        public List<Entities.MstBank> DropdownListBank()
        {
            var banks = db.MstBanks
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

            return banks;
        }

        // Detail Collection Line
        [Authorize, HttpGet, Route("api/collectionLine/detail/{lineId}/{collectionId}")]
        public Entities.TrnCollectionLine DetailLine(string lineId, string collectionId)
        {
            int intLineId = Convert.ToInt32(lineId);
            int inCollectionId = Convert.ToInt32(collectionId);

            var line = db.TrnCollectionLines
                .AsNoTracking()
                .Where(d => d.Id == intLineId && d.CollectionId == inCollectionId)
                .Select(d => new Entities.TrnCollectionLine
                {
                    Id = d.Id,
                    CollectionId = d.CollectionId,
                    SalesId = d.SalesId,
                    SalesNumber = d.TrnSalesInvoice.SalesNumber,
                    PayTypeId = d.PayTypeId,
                    PayType = d.MstPayType.PayType,
                    BankId = d.BankId,
                    Bank = d.MstBank.Bank,
                    CheckNumber = d.CheckNumber,
                    CheckDate = d.CheckDate.ToShortDateString(),
                    Amount = d.Amount
                })
                .FirstOrDefault();

            return line;
        }

        // Add Collection Line
        [Authorize, HttpPost, Route("api/collectionLine/add/{collectionId}")]
        public HttpResponseMessage AddLine(Entities.TrnCollectionLine line, string collectionId)
        {
            try
            {
                int intCollectionId = Convert.ToInt32(collectionId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnCollections
                    .FirstOrDefault(d => d.Id == intCollectionId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add a new line if the current detail is locked.");
                }

                var salesInvoice = db.TrnSalesInvoices
                    .AsNoTracking()
                    .FirstOrDefault(d => d.Id == line.SalesId && d.IsLocked);

                if (salesInvoice == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected sales invoice was not found in the server.");
                }

                var payType = db.MstPayTypes
                            .AsNoTracking()
                            .FirstOrDefault(d => d.Id == line.PayTypeId && d.IsLocked);

                if (payType == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected pay type was not found in the server.");
                }

                var bank = db.MstBanks
                            .AsNoTracking()
                            .FirstOrDefault(d => d.Id == line.BankId && d.IsLocked);

                if (bank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected bank was not found in the server.");
                }

                var newLine = new Data.TrnCollectionLine
                {
                    CollectionId = intCollectionId,
                    SalesId = line.SalesId,
                    PayTypeId = line.PayTypeId,
                    BankId = line.BankId,
                    CheckNumber = line.CheckNumber,
                    CheckDate = Convert.ToDateTime(line.CheckDate),
                    Amount = line.Amount
                };

                db.TrnCollectionLines.InsertOnSubmit(newLine);
                db.SubmitChanges();

                header.CollectionAmount = header.TrnCollectionLines.Sum(d => d.Amount);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Update Collection Line
        [Authorize, HttpPut, Route("api/collectionLine/update/{lineId}/{collectionId}")]
        public HttpResponseMessage UpdateLine(Entities.TrnCollectionLine line, string lineId, string collectionId)
        {
            try
            {
                int intCollectionId = Convert.ToInt32(collectionId);
                int intLineId = Convert.ToInt32(lineId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnCollections
                    .FirstOrDefault(d => d.Id == intCollectionId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot edit and update record if the current detail is locked.");
                }

                var lineData = db.TrnCollectionLines
                    .FirstOrDefault(d => d.Id == intLineId);

                if (lineData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "This current detail no longer exists in the server.");
                }

                var salesInvoice = db.TrnSalesInvoices
                    .AsNoTracking()
                    .FirstOrDefault(d => d.Id == line.SalesId && d.IsLocked);

                if (salesInvoice == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected sales invoice was not found in the server.");
                }

                var payType = db.MstPayTypes
                            .AsNoTracking()
                            .FirstOrDefault(d => d.Id == line.PayTypeId && d.IsLocked);

                if (payType == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected pay type was not found in the server.");
                }

                var bank = db.MstBanks
                            .AsNoTracking()
                            .FirstOrDefault(d => d.Id == line.BankId && d.IsLocked);

                if (bank == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected bank was not found in the server.");
                }

                lineData.SalesId = line.SalesId;
                lineData.PayTypeId = line.PayTypeId;
                lineData.BankId = line.BankId;
                lineData.CheckNumber = line.CheckNumber;
                lineData.CheckDate = Convert.ToDateTime(line.CheckDate);
                lineData.Amount = line.Amount;
                db.SubmitChanges();

                header.CollectionAmount = header.TrnCollectionLines.Sum(d => d.Amount);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Delete Collection Line
        [Authorize, HttpDelete, Route("api/collectionLine/delete/{lineId}/{collectionId}")]
        public HttpResponseMessage DeleteLine(string lineId, string collectionId)
        {
            try
            {
                int intCollectionId = Convert.ToInt32(collectionId);
                int intLineId = Convert.ToInt32(lineId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnCollections
                    .FirstOrDefault(d => d.Id == intCollectionId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete a record if the current detail is locked.");
                }

                var line = db.TrnCollectionLines
                    .FirstOrDefault(d => d.Id == intLineId);

                if (line == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "This current detail no longer exists in the server.");
                }

                db.TrnCollectionLines.DeleteOnSubmit(line);
                db.SubmitChanges();

                header.CollectionAmount = header.TrnCollectionLines.Sum(d => d.Amount);
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
