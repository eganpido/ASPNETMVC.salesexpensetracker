using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace salesexpensetracker.ApiControllers
{
    public class ApiMstExpenseController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Expenses
        [Authorize, HttpGet, Route("api/expense/list")]
        public List<Entities.MstExpense> ListExpense()
        {
            var rawExpenses = from d in db.MstExpenses.OrderByDescending(d => d.ExpenseCode)
                               select new
                               {
                                   Id = d.Id,
                                   ExpenseCode = d.ExpenseCode,
                                   ExpenseName = d.ExpenseName,
                                   IsLocked = d.IsLocked,
                                   CreatedById = d.CreatedById,
                                   CreatedBy = d.MstUser.FullName,
                                   CreatedDateTime = d.CreatedDateTime, // Keep as DateTime
                                   UpdatedById = d.UpdatedById,
                                   UpdatedBy = d.MstUser1.FullName,
                                   UpdatedDateTime = d.UpdatedDateTime, // Keep as DateTime
                               };

            // Step 2: Materialize and format in memory
            var expenses = rawExpenses.ToList() // Execute query on database
                                      .Select(d => new Entities.MstExpense
                                      {
                                          Id = d.Id,
                                          ExpenseCode = d.ExpenseCode,
                                          ExpenseName = d.ExpenseName,
                                          IsLocked = d.IsLocked,
                                          CreatedById = d.CreatedById,
                                          CreatedBy = d.CreatedBy,
                                          CreatedDateTime = d.CreatedDateTime.ToShortDateString(), // Format in memory
                                          UpdatedById = d.UpdatedById,
                                          UpdatedBy = d.UpdatedBy,
                                          UpdatedDateTime = d.UpdatedDateTime.ToShortDateString(), // Format in memory
                                      })
                                      .ToList();

            return expenses;
        }

        // Detail Expense
        [Authorize, HttpGet, Route("api/expense/detail/{id}")]
        public Entities.MstExpense DetailExpense(String id)
        {
            var expense = (from d in db.MstExpenses
                           where d.Id == Convert.ToInt32(id)
                            select new
                            {
                                d.Id,
                                d.ExpenseCode,
                                d.ExpenseName,
                                d.IsLocked,
                                d.CreatedById,
                                CreatedBy = d.MstUser != null ? d.MstUser.FullName : "",
                                d.CreatedDateTime,
                                d.UpdatedById,
                                UpdatedBy = d.MstUser1 != null ? d.MstUser1.FullName : "",
                                d.UpdatedDateTime
                            }).FirstOrDefault();

            if (expense != null)
            {
                return new Entities.MstExpense
                {
                    Id = expense.Id,
                    ExpenseCode = expense.ExpenseCode,
                    ExpenseName = expense.ExpenseName,
                    IsLocked = expense.IsLocked,
                    CreatedById = expense.CreatedById,
                    CreatedBy = expense.CreatedBy,
                    CreatedDateTime = expense.CreatedDateTime.ToShortDateString(),
                    UpdatedById = expense.UpdatedById,
                    UpdatedBy = expense.UpdatedBy,
                    UpdatedDateTime = expense.UpdatedDateTime.ToShortDateString()
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

        // Add Expense
        [Authorize, HttpPost, Route("api/expense/add")]
        public HttpResponseMessage AddExpense()
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var defaultExpenseCode = "001";
                    var lastExpense = from d in db.MstExpenses.OrderByDescending(d => d.Id)
                                       select d;

                    if (lastExpense.Any())
                    {
                        var expenseCode = Convert.ToInt32(lastExpense.FirstOrDefault().ExpenseCode) + 001;
                        defaultExpenseCode = FillLeadingZeroes(expenseCode, 3);
                    }

                    Data.MstExpense newExpense = new Data.MstExpense
                    {
                        ExpenseCode = defaultExpenseCode,
                        ExpenseName = "NA",
                        IsLocked = false,
                        CreatedById = currentUserId,
                        CreatedDateTime = DateTime.Now,
                        UpdatedById = currentUserId,
                        UpdatedDateTime = DateTime.Now,
                    };

                    db.MstExpenses.InsertOnSubmit(newExpense);
                    db.SubmitChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, newExpense.Id);
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

        // Save Expense
        [Authorize, HttpPut, Route("api/expense/save/{id}")]
        public HttpResponseMessage SaveExpense(Entities.MstExpense objExpense, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var expense = from d in db.MstExpenses
                                  where d.Id == Convert.ToInt32(id)
                                   select d;

                    if (expense.Any())
                    {
                        if (!expense.FirstOrDefault().IsLocked)
                        {
                            var saveExpense = expense.FirstOrDefault();
                            saveExpense.ExpenseCode = objExpense.ExpenseCode;
                            saveExpense.ExpenseName = objExpense.ExpenseName;
                            saveExpense.UpdatedById = currentUserId;
                            saveExpense.UpdatedDateTime = DateTime.Now;
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

        // Lock Expense
        [Authorize, HttpPut, Route("api/expense/lock/{id}")]
        public HttpResponseMessage LockExpense(Entities.MstExpense objExpense, String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var expense = from d in db.MstExpenses
                                  where d.Id == Convert.ToInt32(id)
                                   select d;

                    if (expense.Any())
                    {
                        if (!expense.FirstOrDefault().IsLocked)
                        {
                            var expenseByCode = from d in db.MstExpenses
                                                where d.ExpenseCode.Equals(objExpense.ExpenseCode)
                                                 && d.IsLocked == true
                                                 select d;

                            if (!expenseByCode.Any())
                            {
                                var lockExpense = expense.FirstOrDefault();
                                lockExpense.ExpenseCode = objExpense.ExpenseCode;
                                lockExpense.ExpenseName = objExpense.ExpenseName;
                                lockExpense.IsLocked = true;
                                lockExpense.UpdatedById = currentUserId;
                                lockExpense.UpdatedDateTime = DateTime.Now;

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

        // Unlock Expense
        [Authorize, HttpPut, Route("api/expense/unlock/{id}")]
        public HttpResponseMessage UnlockExpense(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var expense = from d in db.MstExpenses
                                  where d.Id == Convert.ToInt32(id)
                                   select d;

                    if (expense.Any())
                    {
                        if (expense.FirstOrDefault().IsLocked)
                        {
                            var unlockExpense = expense.FirstOrDefault();
                            unlockExpense.IsLocked = false;
                            unlockExpense.UpdatedById = currentUserId;
                            unlockExpense.UpdatedDateTime = DateTime.Now;

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

        // Delete Expense
        [Authorize, HttpDelete, Route("api/expense/delete/{id}")]
        public HttpResponseMessage DeleteExpense(String id)
        {
            try
            {
                var currentUser = from d in db.MstUsers
                                  where d.UserId == User.Identity.GetUserId()
                                  select d;

                if (currentUser.Any())
                {
                    var currentUserId = currentUser.FirstOrDefault().Id;

                    var expense = from d in db.MstExpenses
                                  where d.Id == Convert.ToInt32(id)
                                  select d;

                    if (expense.Any())
                    {
                        db.MstExpenses.DeleteOnSubmit(expense.First());

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
