using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Diagnostics;

namespace salesexpensetracker.Controllers
{
    [Authorize]
    public class SoftwareController : UserAccountController
    {
        // ============
        // Data Context
        // ============
        private Data.setdbDataContext db = new Data.setdbDataContext();

        // ===========
        // Page Access
        // ===========
        public String PageAccess(String page)
        {
            String form = "";

            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            if (currentUser.Any())
            {
                //var userForms = from d in db.MstUserForms
                //                where d.UserId == currentUser.FirstOrDefault().Id
                //                select new Entities.MstUserForm
                //                {
                //                    Id = d.Id,
                //                    UserId = d.UserId,
                //                    User = d.MstUser.FullName,
                //                    FormId = d.FormId,
                //                    Form = d.SysForm.FormName,
                //                    Particulars = d.SysForm.Particulars,
                //                    CanAdd = d.CanAdd,
                //                    CanEdit = d.CanEdit,
                //                    CanDelete = d.CanDelete,
                //                    CanLock = d.CanLock,
                //                    CanUnlock = d.CanUnlock,
                //                    CanCancel = d.CanCancel,
                //                    CanPrint = d.CanPrint,
                //                };

                //foreach (var userForm in userForms)
                //{
                //    if (page.Equals(userForm.Form))
                //    {
                //        ViewData.Add("CanAdd", userForm.CanAdd);
                //        ViewData.Add("CanEdit", userForm.CanEdit);
                //        ViewData.Add("CanDelete", userForm.CanDelete);
                //        ViewData.Add("CanLock", userForm.CanLock);
                //        ViewData.Add("CanUnlock", userForm.CanUnlock);
                //        ViewData.Add("CanCancel", userForm.CanCancel);
                //        ViewData.Add("CanPrint", userForm.CanPrint);

                //        form = userForm.Form;
                //        break;
                //    }
                //}
            }

            return form;
        }

        // ========================
        // Formas With Detail Pages
        // ========================
        public String AccessToDetail(String page)
        {
            String form = "";

            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            //var company = from d in db.MstCompanies where d.Id == currentUser.FirstOrDefault().CompanyId select d;
            //if (currentUser.Any())
            //{
            //    var userForms = from d in db.MstUserForms
            //                    where d.UserId == currentUser.FirstOrDefault().Id
            //                    select new Entities.MstUserForm
            //                    {
            //                        Id = d.Id,
            //                        UserId = d.UserId,
            //                        User = d.MstUser.FullName,
            //                        FormId = d.FormId,
            //                        Form = d.SysForm.FormName,
            //                        Particulars = d.SysForm.Particulars,
            //                        CanAdd = d.CanAdd,
            //                        CanEdit = d.CanEdit,
            //                        CanDelete = d.CanDelete,
            //                        CanLock = d.CanLock,
            //                        CanUnlock = d.CanUnlock,
            //                        CanCancel = d.CanCancel,
            //                        CanPrint = d.CanPrint,
            //                    };

            //    foreach (var userForm in userForms)
            //    {
            //        if (page.Equals(userForm.Form))
            //        {
            //            form = userForm.Form;
            //            break;
            //        }
            //    }
            //}

            return form;
        }

        // ===========
        // User Rights
        // ===========
        public ActionResult UserRights(String formName)
        {
            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;
            //var company = from d in db.MstCompanies where d.Id == currentUser.FirstOrDefault().CompanyId select d;
            //var branch = from d in db.MstBranches where d.Id == currentUser.FirstOrDefault().BranchId select d;
            //DateTime expiryDate = branch.FirstOrDefault().ExpiryDate;
            //Boolean isExpired = false;
            //DateTime currentDate = DateTime.Today;
            //if(expiryDate <= currentDate)
            //{
            //    isExpired = true;
            //}
            if (currentUser.Any())
            {
                return View();
                //var userForms = from d in db.MstUserForms where d.UserId == currentUser.FirstOrDefault().Id && d.SysForm.FormName.Equals(formName) select d;
                //if (userForms.Any())
                //{
                //    var userFormsRights = userForms.FirstOrDefault();
                //    var model = new Entities.MstUserForm
                //    {
                //        CanAdd = userFormsRights.CanAdd,
                //        CanEdit = userFormsRights.CanEdit,
                //        CanDelete = userFormsRights.CanDelete,
                //        CanLock = userFormsRights.CanLock,
                //        CanUnlock = userFormsRights.CanUnlock,
                //        CanCancel = userFormsRights.CanCancel,
                //        CanPrint = userFormsRights.CanPrint,
                //    };

                //    if (!isExpired)
                //    {
                //        return View(model);
                //    }
                //    else
                //    {
                //        return RedirectToAction("SubscriptionExpired", "Software");
                //    }
                //}
                //else
                //{
                //    return RedirectToAction("Forbidden", "Software");
                //}
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // ==============
        // Software Pages
        // ==============
        public ActionResult PageNotFound() { return View(); }
        public ActionResult Forbidden() { return View(); }
        public ActionResult SubscriptionExpired() { return View(); }
        public ActionResult Index() { return View(); }

        // ============
        // Master Files
        // ============
        public ActionResult Supplier() { return UserRights("SupplierList"); }
        public ActionResult SupplierDetail() { return UserRights("SupplierDetail"); }
        public ActionResult Customer() { return UserRights("CustomerList"); }
        public ActionResult CustomerDetail() { return UserRights("CustomerDetail"); }
        public ActionResult ProductList() { return View(); }
        public ActionResult ProductDetail() { return View(); }
        public ActionResult Bank() { return UserRights("BankList"); }
        public ActionResult ChartOfAccounts() { return UserRights("ChartOfAccounts"); }

        // ============
        // Transactions
        // ============
        public ActionResult PurchaseRequest() { return UserRights("PurchaseRequestList"); }
        public ActionResult PurchaseRequestDetail() { return UserRights("PurchaseRequestDetail"); }
        public ActionResult PurchaseOrder() { return UserRights("PurchaseOrderList"); }
        public ActionResult PurchaseOrderDetail() { return UserRights("PurchaseOrderDetail"); }
        public ActionResult ReceivingReceipt() { return UserRights("ReceivingReceiptList"); }
        public ActionResult ReceivingReceiptDetail() { return UserRights("ReceivingReceiptDetail"); }
        public ActionResult Disbursement() { return UserRights("DisbursementList"); }
        public ActionResult DisbursementDetail() { return UserRights("DisbursementDetail"); }
        public ActionResult Sales() { return UserRights("SalesInvoiceList"); }
        public ActionResult SalesDetail() { return UserRights("SalesInvoiceDetail"); }
        public ActionResult SalesOrder() { return UserRights("SalesOrderList"); }
        public ActionResult SalesOrderDetail() { return UserRights("SalesOrderDetail"); }
        public ActionResult Collection() { return UserRights("CollectionList"); }
        public ActionResult CollectionDetail() { return UserRights("CollectionDetail"); }
        public ActionResult JournalVoucher() { return UserRights("JournalVoucherList"); }
        public ActionResult JournalVoucherDetail() { return UserRights("JournalVoucherDetail"); }
        public ActionResult StockIn() { return UserRights("StockInList"); }
        public ActionResult StockInDetail() { return UserRights("StockInDetail"); }
        public ActionResult StockOut() { return UserRights("StockOutList"); }
        public ActionResult StockOutDetail() { return UserRights("StockOutDetail"); }
        public ActionResult StockTransfer() { return UserRights("StockTransferList"); }
        public ActionResult StockTransferDetail() { return UserRights("StockTransferDetail"); }
        public ActionResult StockCount() { return UserRights("StockCountList"); }
        public ActionResult StockCountDetail() { return UserRights("StockCountDetail"); }
        public ActionResult StockWithdrawal() { return UserRights("StockWithdrawalList"); }
        public ActionResult StockWithdrawalDetail() { return UserRights("StockWithdrawalDetail"); }
        public ActionResult ItemPrice() { return UserRights("ItemPriceList"); }
        public ActionResult ItemPriceDetail() { return UserRights("ItemPriceDetail"); }
        public ActionResult CounterReceipt() { return UserRights("CounterReceiptList"); }
        public ActionResult CounterReceiptDetail() { return UserRights("CounterReceiptDetail"); }
        public ActionResult BankReconciliation() { return UserRights("BankReconciliation"); }

        // ======
        // System
        // ======
        public ActionResult Company() { return UserRights("CompanyList"); }
        public ActionResult CompanyDetail() { return UserRights("CompanyDetail"); }
        public ActionResult Users() { return UserRights("UserList"); }
        public ActionResult UsersDetail() { return UserRights("UserDetail"); }
        public ActionResult SystemTables() { return UserRights("SystemTables"); }

        // ========================
        // Accounts Payable Reports
        // ========================
        public ActionResult AccountsPayable() { return UserRights("AccountsPayableReport"); }
        public ActionResult AccountsPayableReport() { return UserRights("ViewAccountsPayableReport"); }
        public ActionResult PurchaseOrderSummaryReport() { return UserRights("ViewPurchaseSummaryReport"); }
        public ActionResult PurchaseOrderDetailReport() { return UserRights("ViewPurchaseDetailReport"); }
        public ActionResult PurchaseRequestSummaryReport() { return UserRights("ViewPurchaseSummaryReport"); }
        public ActionResult PurchaseRequestDetailReport() { return UserRights("ViewPurchaseDetailReport"); }
        public ActionResult ReceivingReceiptSummaryReport() { return UserRights("ViewReceivingReceiptSummaryReport"); }
        public ActionResult ReceivingReceiptDetailReport() { return UserRights("ViewReceivingReceiptDetailReport"); }
        public ActionResult DisbursementSummaryReport() { return UserRights("ViewDisbursementSummaryReport"); }
        public ActionResult DisbursementDetailReport() { return UserRights("ViewDisbursementDetailReport"); }

        // ===========================
        // Accounts Receivable Reports
        // ===========================
        public ActionResult AccountsReceivable() { return UserRights("AccountsReceivableReport"); }
        public ActionResult AccountsReceivableReport() { return UserRights("ViewAccountsReceivableReport"); }
        public ActionResult AccountsReceivableSummaryReport() { return UserRights("ViewAccountsReceivableReport"); }
        public ActionResult StatementOfAccount() { return View(); ; }
        public ActionResult StatementOfAccountByDateRange() { return View(); ; }
        public ActionResult SalesOrderSummaryReport() { return UserRights("ViewSalesSummaryReport"); }
        public ActionResult SalesOrderDetailReport() { return UserRights("ViewSalesDetailReport"); }
        public ActionResult SalesSummaryReport() { return UserRights("ViewSalesSummaryReport"); }
        public ActionResult SalesDetailReport() { return UserRights("ViewSalesDetailReport"); }
        public ActionResult SalesDetailReportCost() { return UserRights("ViewSalesDetailReport"); }
        public ActionResult SalesDetailReportPerItem() { return UserRights("ViewSalesDetailReport"); }
        public ActionResult SalesDetailReportPerItemCategory() { return UserRights("ViewSalesDetailReport"); }
        public ActionResult CollectionSummaryReport() { return UserRights("ViewCollectionSummaryReport"); }
        public ActionResult CollectionDetailReport() { return UserRights("ViewCollectionDetailReport"); }
        public ActionResult ConsignmentReport() { return UserRights("ViewConsignmentReport"); }
        public ActionResult CounterReceiptReport() { return UserRights("ViewCounterReceiptReport"); }
        public ActionResult AccountReceivableOver120Days() { return View(); }

        // =================
        // Inventory Reports
        // =================
        public ActionResult Inventory() { return UserRights("InventoryReport"); }
        public ActionResult InventoryReport() { return UserRights("ViewInventoryReport"); }
        public ActionResult InventoryReportItem() { return View(); }
        public ActionResult StockCard() { return UserRights("ViewStockCard"); }
        public ActionResult InventorySerialReport() { return UserRights("ViewInventoryReport"); }
        public ActionResult StockCardSerial() { return UserRights("ViewStockCard"); }
        public ActionResult StockInDetailReport() { return UserRights("ViewStockInDetailReport"); }
        public ActionResult StockOutDetailReport() { return UserRights("ViewStockOutDetailReport"); }
        public ActionResult StockTransferDetailReport() { return UserRights("ViewStockTransferDetailReport"); }
        public ActionResult StockCountDetailReport() { return UserRights("ViewStockTransferDetailReport"); }
        public ActionResult ItemList() { return UserRights("ViewItemList"); }
        public ActionResult ItemListSupplier() { return View(); }
        public ActionResult ItemListCategory() { return View(); }
        public ActionResult ItemComponentList() { return UserRights("ViewItemComponentList"); }
        public ActionResult PhysicalCountSheet() { return UserRights("ViewPhysicalCountSheet"); }

        // ============================
        // Financial Statements Reports
        // ============================
        public ActionResult FinancialStatements() { return UserRights("FinancialStatementReport"); }
        public ActionResult BalanceSheet() { return UserRights("ViewBalanceSheet"); }
        public ActionResult BalanceSheetPerBranch() { return UserRights("ViewBalanceSheetPerBranch"); }
        public ActionResult IncomeStatement() { return UserRights("ViewIncomeStatement"); }
        public ActionResult IncomeStatementPerBranch() { return UserRights("ViewIncomeStatement"); }
        public ActionResult CashFlow() { return UserRights("ViewCashFlow"); }
        public ActionResult TrialBalance() { return UserRights("ViewTrialBalance"); }
        public ActionResult TrialBalancePerBranch() { return UserRights("ViewTrialBalance"); }
        public ActionResult AccountLedger() { return UserRights("ViewAccountLedger"); }
        public ActionResult ReceivingReceiptBook() { return UserRights("ViewReceivingReceiptBook"); }
        public ActionResult DisbursementBook() { return UserRights("ViewDisbursementBook"); }
        public ActionResult SalesBook() { return UserRights("ViewSalesBook"); }
        public ActionResult CollectionBook() { return UserRights("ViewCollectionBook"); }
        public ActionResult StockInBook() { return UserRights("ViewStockInBook"); }
        public ActionResult StockOutBook() { return UserRights("ViewStockOutBook"); }
        public ActionResult StockTransferBook() { return UserRights("ViewStockTransferBook"); }
        public ActionResult JournalVoucherBook() { return UserRights("ViewJournalVoucherBook"); }
        public ActionResult JournalEntriesDisbalance() { return UserRights("ViewJournalEntriesDisbalance"); }
        public ActionResult SalesDetailItemDouble() { return UserRights("ViewSalesDetailItemDouble"); }

        // ===============
        // BIR CAS Reports
        // ===============
        public ActionResult BIRCASReports() { return UserRights("BIRCASReport"); }
        public ActionResult BIRCASCashReceiptBook() { return UserRights("ViewBIRCASCashReceiptBook"); }
        public ActionResult BIRCASSalesJournal() { return UserRights("ViewBIRCASSalesJournal"); }
        public ActionResult BIRCASPurchaseJournal() { return UserRights("ViewBIRCASPurchaseJournal"); }
        public ActionResult BIRCASDisbursementBook() { return UserRights("ViewBIRCASDisbursementBook"); }
        public ActionResult BIRCASGeneralLedger() { return UserRights("ViewBIRCASGeneralLedger"); }
        public ActionResult BIRCASInventoryJournal() { return UserRights("ViewBIRCASInventoryJournal"); }
        public ActionResult BIRCASAuditTrail() { return UserRights("ViewBIRCASAuditTrail"); }
        public ActionResult BIRCASWithholdingTaxAlphalist() { return UserRights("BIRCASReport"); }

        // =======================
        // POS Integration Reports
        // =======================
        public ActionResult SalesDetailReportVATSales() { return View(); }
        public ActionResult SalesSummaryReportSalesNo() { return View(); }
        public ActionResult CancelledSalesSummaryReport() { return View(); }
        public ActionResult SeniorCitizenSalesSummaryReport() { return View(); }
        public ActionResult TopSellingItemReport() { return UserRights("ViewTopSellingItemsReport"); }
        public ActionResult HourlyTopSellingItemsReport() { return UserRights("ViewHourlyTopSellingItemsReport"); }
        public ActionResult SalesSummaryReportAllFields() { return View(); }
    }
}