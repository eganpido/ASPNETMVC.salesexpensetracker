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
    public class ApiMstProductController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Products
        [Authorize, HttpGet, Route("api/product/list")]
        public List<Entities.MstProduct> ListProduct()
        {
            var rawProducts = from d in db.MstProducts.OrderByDescending(d => d.ProductCode)
                              select new
                              {
                                  Id = d.Id,
                                  ProductCode = d.ProductCode,
                                  ProductDescription = d.ProductDescription,
                                  Cost = d.Cost, // Keep as decimal
                                  Price = d.Price, // Keep as decimal
                                  IsLocked = d.IsLocked,
                                  CreatedById = d.CreatedById,
                                  CreatedBy = d.MstUser.FullName,
                                  CreatedDateTime = d.CreatedDateTime, // Keep as DateTime
                                  UpdatedById = d.UpdatedById,
                                  UpdatedBy = d.MstUser1.FullName,
                                  UpdatedDateTime = d.UpdatedDateTime, // Keep as DateTime
                              };

            // Step 2: Materialize and format in memory
            var products = rawProducts.ToList() // Execute query on database
                                      .Select(d => new Entities.MstProduct
                                      {
                                          Id = d.Id,
                                          ProductCode = d.ProductCode,
                                          ProductDescription = d.ProductDescription,
                                          Cost = d.Cost.ToString("#,##0.00"), // Format in memory
                                          Price = d.Price.ToString("#,##0.00"), // Format in memory
                                          IsLocked = d.IsLocked,
                                          CreatedById = d.CreatedById,
                                          CreatedBy = d.CreatedBy,
                                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(), // Format in memory
                                          UpdatedById = d.UpdatedById,
                                          UpdatedBy = d.UpdatedBy,
                                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(), // Format in memory
                                      })
                                      .ToList();

            return products;
        }

        // Detail Product
        [Authorize, HttpGet, Route("api/product/detail/{id}")]
        public Entities.MstProduct DetailProduct(String id)
        {
            var product = (from d in db.MstProducts
                           where d.Id == Convert.ToInt32(id)
                           select new
                           {
                               d.Id,
                               d.ProductCode,
                               d.ProductDescription,
                               d.Cost,
                               d.Price,
                               d.IsLocked,
                               d.CreatedById,
                               CreatedBy = d.MstUser != null ? d.MstUser.FullName : "",
                               d.CreatedDateTime,
                               d.UpdatedById,
                               UpdatedBy = d.MstUser1 != null ? d.MstUser1.FullName : "",
                               d.UpdatedDateTime
                           }).FirstOrDefault();

            if (product != null)
            {
                return new Entities.MstProduct
                {
                    Id = product.Id,
                    ProductCode = product.ProductCode,
                    ProductDescription = product.ProductDescription,
                    Cost = product.Cost.ToString("#,##0.00"),
                    Price = product.Price.ToString("#,##0.00"),
                    IsLocked = product.IsLocked,
                    CreatedById = product.CreatedById,
                    CreatedBy = product.CreatedBy,
                    CreatedDateTime = product.CreatedDateTime.ToShortDateString(),
                    UpdatedById = product.UpdatedById,
                    UpdatedBy = product.UpdatedBy,
                    UpdatedDateTime = product.UpdatedDateTime.ToShortDateString()
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

        // Add Product
        [Authorize, HttpPost, Route("api/product/add")]
        public HttpResponseMessage AddProduct()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var defaultProductCode = "0000000001";
                    var lastProduct = from d in db.MstProducts.OrderByDescending(d => d.Id)
                                   select d;

                    if (lastProduct.Any())
                    {
                        var productCode = Convert.ToInt32(lastProduct.FirstOrDefault().ProductCode) + 0000000001;
                        defaultProductCode = FillLeadingZeroes(productCode, 10);
                    }

                    Data.MstProduct newProduct = new Data.MstProduct
                    {
                        ProductCode = defaultProductCode,
                        ProductDescription = "NA",
                        Cost = 0,
                        Price = 0,
                        IsLocked = false,
                        CreatedById = currentUserId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUserId,
                        UpdatedDateTime = DateTime.Now,
                    };

                    db.MstProducts.InsertOnSubmit(newProduct);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newProduct.Id);
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

        // Save Product
        [Authorize, HttpPut, Route("api/product/save/{id}")]
        public HttpResponseMessage SaveItem(Entities.MstProduct objProduct, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var product = from d in db.MstProducts
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (product.Any())
                    {
                        if (!product.FirstOrDefault().IsLocked)
                        {
                            var saveProduct = product.FirstOrDefault();
                            saveProduct.ProductCode = objProduct.ProductCode;
                            saveProduct.ProductDescription = objProduct.ProductDescription;
                            saveProduct.Price = Convert.ToDecimal(objProduct.Price);
                            saveProduct.Cost = Convert.ToDecimal(objProduct.Cost);
                            saveProduct.UpdatedById = currentUserId;
                            saveProduct.UpdatedDateTime = DateTime.Now;
                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Saving Error. These item details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These item details are not found in the server.");
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

        // Lock Product
        [Authorize, HttpPut, Route("api/product/lock/{id}")]
        public HttpResponseMessage LockItem(Entities.MstProduct objProduct, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var product = from d in db.MstProducts
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (product.Any())
                    {
                        if (!product.FirstOrDefault().IsLocked)
                        {
                            var productByCode = from d in db.MstProducts
                                                   where d.ProductCode.Equals(objProduct.ProductCode)
                                                   && d.IsLocked == true
                                                   select d;

                            if (!productByCode.Any())
                            {
                                var lockProduct = product.FirstOrDefault();
                                lockProduct.ProductCode = objProduct.ProductCode;
                                lockProduct.ProductDescription = objProduct.ProductDescription;
                                lockProduct.Cost = Convert.ToDecimal(objProduct.Cost);
                                lockProduct.Price = Convert.ToDecimal(objProduct.Price);
                                lockProduct.IsLocked = true;
                                lockProduct.UpdatedById = currentUserId;
                                lockProduct.UpdatedDateTime = DateTime.Now;

                                db.SubmitChanges();


                                return Request.CreateResponse(HttpStatusCode.OK);
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, "Product Code is already taken.");
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Locking Error. These product details are already locked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These product details are not found in the server.");
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

        // Unlock Product
        [Authorize, HttpPut, Route("api/product/unlock/{id}")]
        public HttpResponseMessage UnlockProduct(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var product = from d in db.MstProducts
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (product.Any())
                    {
                        if (product.FirstOrDefault().IsLocked)
                        {
                            var unlockProduct = product.FirstOrDefault();
                            unlockProduct.IsLocked = false;
                            unlockProduct.UpdatedById = currentUserId;
                            unlockProduct.UpdatedDateTime = DateTime.Now;

                            db.SubmitChanges();

                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Unlocking Error. These item details are already unlocked.");
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. These product details are not found in the server.");
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

        // Delete Product
        [Authorize, HttpDelete, Route("api/product/delete/{id}")]
        public HttpResponseMessage DeleteProduct(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var product = from d in db.MstProducts
                               where d.Id == Convert.ToInt32(id)
                               select d;

                    if (product.Any())
                    {
                        db.MstProducts.DeleteOnSubmit(product.First());

                        db.SubmitChanges();

                        return Request.CreateResponse(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Data not found. This selected item is not found in the server.");
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
