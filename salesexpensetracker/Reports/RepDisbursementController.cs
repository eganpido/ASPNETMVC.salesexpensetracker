using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace salesexpensetracker.Reports
{
    // Disbursement - PDF
    [Authorize]
    public class RepDisbursementController : Controller
    {
        private Data.setdbDataContext db = new Data.setdbDataContext();

        public ActionResult Disbursement(int DisbursementId)
        {
            var currentUser = from d in db.MstUsers where d.UserId == User.Identity.GetUserId() select d;

            MemoryStream memoryStream = new MemoryStream();
            Rectangle halfLetterCrosswise = new Rectangle(612f, 396f);

            Document document = new Document(halfLetterCrosswise, 10f, 10f, 65f, 80f);

            PdfWriter pdfWriter = PdfWriter.GetInstance(document, memoryStream);
            pdfWriter.PageEvent = new DisbursementHeaderFooter(currentUser.FirstOrDefault().Id, DisbursementId);

            document.Open();

            Font fontArial09 = FontFactory.GetFont("Arial", 9);
            Font fontArial09Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial09Italic = FontFactory.GetFont("Arial", 09, Font.ITALIC);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);

            var header = from d in db.TrnDisbursements where d.Id == Convert.ToInt32(DisbursementId) && d.IsLocked == true select d;
            if (header.Any())
            {
                String disbursementNumber = header.FirstOrDefault().DisbursementNumber;
                String supplier = header.FirstOrDefault().MstSupplier.SupplierName;
                String disbursementDate = header.FirstOrDefault().DisbursementDate.ToShortDateString();
                String remarks = header.FirstOrDefault().Remarks;
                String payType = header.FirstOrDefault().MstPayType.PayType;
                String checkNumber = header.FirstOrDefault().CheckNumber;
                String checkDate = checkNumber != "NA" ? header.FirstOrDefault().CheckDate.ToShortDateString() : " ";
                String checkBank = checkNumber != "NA" ? header.FirstOrDefault().MstBank.BankCode : " ";

                PdfPTable table = new PdfPTable(4);
                table.SetWidths(new float[] { 65f, 150f, 70f, 80f });
                table.WidthPercentage = 100;
                table.AddCell(new PdfPCell(new Phrase("CV No. : ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase(disbursementNumber, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase("CV Date :", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                table.AddCell(new PdfPCell(new Phrase(disbursementDate, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                table.AddCell(new PdfPCell(new Phrase("Supplier :  ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase(supplier, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase("Pay Type : ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                table.AddCell(new PdfPCell(new Phrase(payType, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                table.AddCell(new PdfPCell(new Phrase("Remarks :  ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase(remarks, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase("Check No.: ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                table.AddCell(new PdfPCell(new Phrase(checkNumber, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                table.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase(" ", fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase("Check Date.: ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                table.AddCell(new PdfPCell(new Phrase(checkDate, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                table.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase(" ", fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                table.AddCell(new PdfPCell(new Phrase("Check Bank.: ", fontArial09Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                table.AddCell(new PdfPCell(new Phrase(checkBank, fontArial09)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(table);

                PdfPTable spaceTable = new PdfPTable(1);
                spaceTable.SetWidths(new float[] { 100f });
                spaceTable.WidthPercentage = 100;
                spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f });
                document.Add(spaceTable);

                var lines = from d in header.FirstOrDefault().TrnDisbursementLines select d;
                if (lines.Any())
                {
                    PdfPTable tableLines = new PdfPTable(3);
                    tableLines.SetWidths(new float[] { 40f, 100f, 40f });
                    tableLines.WidthPercentage = 100;
                    tableLines.AddCell(new PdfPCell(new Phrase("Expense", fontArial09Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableLines.AddCell(new PdfPCell(new Phrase("Particulars", fontArial09Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });
                    tableLines.AddCell(new PdfPCell(new Phrase("Amount", fontArial09Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 7f });

                    Decimal totalAmount = 0;

                    foreach (var _line in lines)
                    {
                        tableLines.AddCell(new PdfPCell(new Phrase(_line.MstExpense.ExpenseName, fontArial09)) { Border = 0, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLines.AddCell(new PdfPCell(new Phrase(_line.Particulars, fontArial09)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLines.AddCell(new PdfPCell(new Phrase(_line.Amount.ToString("#,##0.00"), fontArial09)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 6f, PaddingLeft = 5f, PaddingRight = 5f });

                        totalAmount += _line.Amount;
                    }

                    tableLines.AddCell(new PdfPCell(new Phrase("Total : ", fontArial09Bold)) { Colspan = 2, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableLines.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial09Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f });

                    document.Add(tableLines);
                }
                document.Add(spaceTable);

                Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0F, 100.0F, BaseColor.BLACK, Element.ALIGN_MIDDLE, 2F)));
                PdfPTable tableUsers = new PdfPTable(4);
                tableUsers.SetWidths(new float[] { 100f, 100f, 100f, 100f });
                tableUsers.WidthPercentage = 100;

                tableUsers.AddCell(new PdfPCell(new Phrase("Received by : ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial09Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 9f, PaddingLeft = 5f, PaddingRight = 5f });

                document.Add(tableUsers);
                document.Add(spaceTable);
            }

            document.Close();

            byte[] bytesStream = memoryStream.ToArray();

            memoryStream = new MemoryStream();
            memoryStream.Write(bytesStream, 0, bytesStream.Length);
            memoryStream.Position = 0;

            return new FileStreamResult(memoryStream, "application/pdf");
        }
    }

    // =================
    // Header and Footer
    // =================
    class DisbursementHeaderFooter : PdfPageEventHelper
    {
        public Int32 disbursementId = 0;
        public Data.setdbDataContext db;

        public DisbursementHeaderFooter(Int32 currentUserId, Int32 currentSalesId)
        {
            disbursementId = currentSalesId;

            db = new Data.setdbDataContext();
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            Font fontArial09 = FontFactory.GetFont("Arial", 09);
            Font fontArial09Italic = FontFactory.GetFont("Arial", 09, Font.ITALIC);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);

            Paragraph headerLine = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1F, 100.0F, BaseColor.BLACK, Element.ALIGN_MIDDLE, 5F)));
            Paragraph footerLine = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0F, 100.0F, BaseColor.BLACK, Element.ALIGN_MIDDLE, 5F)));

            var title = "CASH/CHECK VOUCHER";

            string imagePath = "~/Images/logo/streetsmartLogo.png";
            string fullPath = HttpContext.Current.Server.MapPath(imagePath);

            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(fullPath);
            img.ScaleToFit(40f, 40f);

            PdfPTable tableHeader = new PdfPTable(2);
            tableHeader.SetWidths(new float[] { 1f, 70f });
            tableHeader.DefaultCell.Border = 0;
            tableHeader.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;

            tableHeader.AddCell(new PdfPCell(img) { Border = 0 });
            tableHeader.AddCell(new PdfPCell(new Phrase(title, fontArial12Bold)) { Border = 0, HorizontalAlignment = 1, PaddingTop = 10f });
            tableHeader.AddCell(new PdfPCell(new Phrase(headerLine)) { Border = 0, Colspan = 2 });
            tableHeader.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetTop(document.TopMargin) + 50, writer.DirectContent);

            PdfPTable tableFooter = new PdfPTable(2);
            tableFooter.SetWidths(new float[] { 70f, 50f });
            tableFooter.DefaultCell.Border = 0;
            tableFooter.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            tableFooter.AddCell(new PdfPCell(new Phrase("(Printed " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt") + ") Page " + writer.PageNumber, fontArial09)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, Colspan = 2 });
            tableFooter.WriteSelectedRows(0, -1, document.LeftMargin, writer.PageSize.GetBottom(document.BottomMargin) - 50f, writer.DirectContent);
        }
    }
}