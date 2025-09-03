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
    public class ApiTrnSalesInvoiceLineController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Sales Invoice Line
        [Authorize, HttpGet, Route("api/salesInvoiceLine/list/{salesId}")]
        public List<Entities.TrnSalesInvoiceLine> ListLine(int salesId)
        {
            var listLines = db.TrnSalesInvoiceLines
                .AsNoTracking()
                .Where(d => d.SalesId == salesId)
                .Select(d => new Entities.TrnSalesInvoiceLine
                {
                    Id = d.Id,
                    SalesId = d.SalesId,
                    ProductId = d.ProductId,
                    Product = d.MstProduct.ProductDescription,
                    Price = d.Price,
                    Quantity = d.Quantity,
                    Amount = d.Amount
                })
                .ToList();

            return listLines;
        }

        // Dropdown List Product
        [Authorize, HttpGet, Route("api/salesInvoiceLine/list/product")]
        public List<Entities.MstProduct> DropdownListProduct()
        {
            var products = db.MstProducts
                        .AsNoTracking()
                        .Where(d => d.IsLocked)
                        .OrderBy(d => d.ProductDescription)
                        .Select(d => new
                        {
                            d.Id,
                            d.ProductDescription,
                            d.Price
                        })
                        .ToList()
                        .Select(d => new Entities.MstProduct
                        {
                            Id = d.Id,
                            ProductDescription = d.ProductDescription,
                            Price = d.Price.ToString("#,##0.00")
                        })
                        .ToList();

            return products;
        }

        // Detail Sales Invoice Line
        [Authorize, HttpGet, Route("api/salesInvoiceLine/detail/{lineId}/{salesId}")]
        public Entities.TrnSalesInvoiceLine DetailLine(string lineId, string salesId)
        {
            int intLineId = Convert.ToInt32(lineId);
            int inSalesId = Convert.ToInt32(salesId);

            var line = db.TrnSalesInvoiceLines
                .AsNoTracking()
                .Where(d => d.Id == intLineId && d.SalesId == inSalesId)
                .Select(d => new Entities.TrnSalesInvoiceLine
                {
                    Id = d.Id,
                    SalesId = d.SalesId,
                    ProductId = d.ProductId,
                    Product = d.MstProduct.ProductDescription,
                    Price = d.Price,
                    Quantity = d.Quantity,
                    Amount = d.Amount
                })
                .FirstOrDefault();

            return line;
        }

        // Add Sales Invoice Line
        [Authorize, HttpPost, Route("api/salesInvoiceLine/add/{salesId}")]
        public HttpResponseMessage AddLine(Entities.TrnSalesInvoiceLine line, string salesId)
        {
            try
            {
                int intSalesId = Convert.ToInt32(salesId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnSalesInvoices
                    .FirstOrDefault(d => d.Id == intSalesId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add a new line if the current detail is locked.");
                }

                var product = db.MstProducts
                    .AsNoTracking()
                    .FirstOrDefault(d => d.Id == line.ProductId && d.IsLocked);

                if (product == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected product was not found in the server.");
                }

                var newLine = new Data.TrnSalesInvoiceLine
                {
                    SalesId = intSalesId,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    Price = line.Price,
                    Amount = line.Amount
                };

                db.TrnSalesInvoiceLines.InsertOnSubmit(newLine);
                db.SubmitChanges();

                header.SalesAmount = header.TrnSalesInvoiceLines.Sum(d => d.Amount);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Update Sales Invoice Line
        [Authorize, HttpPut, Route("api/salesInvoiceLine/update/{lineId}/{salesId}")]
        public HttpResponseMessage UpdateLine(Entities.TrnSalesInvoiceLine line, string lineId, string salesId)
        {
            try
            {
                int intSalesId = Convert.ToInt32(salesId);
                int intLineId = Convert.ToInt32(lineId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnSalesInvoices
                    .FirstOrDefault(d => d.Id == intSalesId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot edit and update record if the current detail is locked.");
                }

                var lineData = db.TrnSalesInvoiceLines
                    .FirstOrDefault(d => d.Id == intLineId);

                if (lineData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "This current detail no longer exists in the server.");
                }

                var product = db.MstProducts
                    .AsNoTracking()
                    .FirstOrDefault(d => d.Id == line.ProductId && d.IsLocked);

                if (product == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected product was not found in the server.");
                }

                lineData.SalesId = intSalesId;
                lineData.ProductId = line.ProductId;
                lineData.Quantity = line.Quantity;
                lineData.Price = line.Price;
                lineData.Amount = line.Amount;
                db.SubmitChanges();

                header.SalesAmount = header.TrnSalesInvoiceLines.Sum(d => d.Amount);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Delete Sales Invoice Line
        [Authorize, HttpDelete, Route("api/salesInvoiceLine/delete/{lineId}/{salesId}")]
        public HttpResponseMessage DeleteLine(string lineId, string salesId)
        {
            try
            {
                int intSalesId = Convert.ToInt32(salesId);
                int intLineId = Convert.ToInt32(lineId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnSalesInvoices
                    .FirstOrDefault(d => d.Id == intSalesId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete a record if the current detail is locked.");
                }

                var line = db.TrnSalesInvoiceLines
                    .FirstOrDefault(d => d.Id == intLineId);

                if (line == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "This current detail no longer exists in the server.");
                }

                db.TrnSalesInvoiceLines.DeleteOnSubmit(line);
                db.SubmitChanges();

                header.SalesAmount = header.TrnSalesInvoiceLines.Sum(d => d.Amount);
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
