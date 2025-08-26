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
    public class ApiTrnDisbursementLineController : ApiController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // List Disbursement Line
        [Authorize, HttpGet, Route("api/disbursementLine/list/{disbursementId}")]
        public List<Entities.TrnDisbursementLine> ListLine(int disbursementId)
        {
            var listLines = db.TrnDisbursementLines
                .AsNoTracking()
                .Where(d => d.DisbursementId == disbursementId)
                .Select(d => new Entities.TrnDisbursementLine
                {
                    Id = d.Id,
                    DisbursementId = d.DisbursementId,
                    ExpenseId = d.ExpenseId,
                    Expense = d.MstExpense.ExpenseName,
                    Particulars = d.Particulars,
                    Amount = d.Amount
                })
                .ToList();

            return listLines;
        }

        // Dropdown List Expense
        [Authorize, HttpGet, Route("api/disbursementLine/list/expense")]
        public List<Entities.MstExpense> DropdownListExpense()
        {
            var expenses = db.MstExpenses
                        .AsNoTracking()
                        .Where(d => d.IsLocked)
                        .OrderBy(d => d.ExpenseName)
                        .Select(d => new
                        {
                            d.Id,
                            d.ExpenseName
                        })
                        .ToList()
                        .Select(d => new Entities.MstExpense
                        {
                            Id = d.Id,
                            ExpenseName = d.ExpenseName
                        })
                        .ToList();

            return expenses;
        }

        // Detail Disbursement Line
        [Authorize, HttpGet, Route("api/disbursementLine/detail/{lineId}/{disbursementId}")]
        public Entities.TrnDisbursementLine DetailLine(string lineId, string disbursementId)
        {
            int intLineId = Convert.ToInt32(lineId);
            int inDisbursementId = Convert.ToInt32(disbursementId);

            var line = db.TrnDisbursementLines
                .AsNoTracking()
                .Where(d => d.Id == intLineId && d.DisbursementId == inDisbursementId)
                .Select(d => new Entities.TrnDisbursementLine
                {
                    Id = d.Id,
                    DisbursementId = d.DisbursementId,
                    ExpenseId = d.ExpenseId,
                    Expense = d.MstExpense.ExpenseName,
                    Particulars = d.Particulars,
                    Amount = d.Amount
                })
                .FirstOrDefault();

            return line;
        }

        // Add Disbursement Line
        [Authorize, HttpPost, Route("api/disbursementLine/add/{disbursementId}")]
        public HttpResponseMessage AddLine(Entities.TrnDisbursementLine line, string disbursementId)
        {
            try
            {
                int intDisbursementId = Convert.ToInt32(disbursementId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnDisbursements
                    .FirstOrDefault(d => d.Id == intDisbursementId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot add a new line if the current detail is locked.");
                }

                var expense = db.MstExpenses
                    .AsNoTracking()
                    .FirstOrDefault(d => d.Id == line.ExpenseId && d.IsLocked);

                if (expense == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected expense was not found in the server.");
                }

                var newLine = new Data.TrnDisbursementLine
                {
                    DisbursementId = intDisbursementId,
                    ExpenseId = line.ExpenseId,
                    Particulars = line.Particulars,
                    Amount = line.Amount
                };

                db.TrnDisbursementLines.InsertOnSubmit(newLine);
                db.SubmitChanges();

                header.DisbursementAmount = header.TrnDisbursementLines.Sum(d => d.Amount);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Update Disbursement Line
        [Authorize, HttpPut, Route("api/disbursementLine/update/{lineId}/{disbursementId}")]
        public HttpResponseMessage UpdateLine(Entities.TrnDisbursementLine line, string lineId, string disbursementId)
        {
            try
            {
                int intDisbursementId = Convert.ToInt32(disbursementId);
                int intLineId = Convert.ToInt32(lineId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnDisbursements
                    .FirstOrDefault(d => d.Id == intDisbursementId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot edit and update record if the current detail is locked.");
                }

                var lineData = db.TrnDisbursementLines
                    .FirstOrDefault(d => d.Id == intLineId);

                if (lineData == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "This current detail no longer exists in the server.");
                }

                var product = db.MstExpenses
                    .AsNoTracking()
                    .FirstOrDefault(d => d.Id == line.ExpenseId && d.IsLocked);

                if (product == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "The selected product was not found in the server.");
                }

                lineData.DisbursementId = intDisbursementId;
                lineData.ExpenseId = line.ExpenseId;
                lineData.Particulars = line.Particulars;
                lineData.Amount = line.Amount;
                db.SubmitChanges();

                header.DisbursementAmount = header.TrnDisbursementLines.Sum(d => d.Amount);
                db.SubmitChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something's went wrong from the server.");
            }
        }

        // Delete Disbursement Line
        [Authorize, HttpDelete, Route("api/disbursementLine/delete/{lineId}/{disbursementId}")]
        public HttpResponseMessage DeleteLine(string lineId, string disbursementId)
        {
            try
            {
                int intDisbursementId = Convert.ToInt32(disbursementId);
                int intLineId = Convert.ToInt32(lineId);
                string currentUserName = User.Identity.GetUserId();

                var currentUser = db.MstUsers
                    .AsNoTracking()
                    .FirstOrDefault(d => d.UserId == currentUserName);

                if (currentUser == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "There is no current user logged in.");
                }

                var header = db.TrnDisbursements
                    .FirstOrDefault(d => d.Id == intDisbursementId);

                if (header == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "These current details are not found in the server. Please add a new record first before proceeding.");
                }

                if (header.IsLocked)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You cannot delete a record if the current detail is locked.");
                }

                var line = db.TrnDisbursementLines
                    .FirstOrDefault(d => d.Id == intLineId);

                if (line == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "This current detail no longer exists in the server.");
                }

                db.TrnDisbursementLines.DeleteOnSubmit(line);
                db.SubmitChanges();

                header.DisbursementAmount = header.TrnDisbursementLines.Sum(d => d.Amount);
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
