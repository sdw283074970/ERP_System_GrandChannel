using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using ClothResorting.Models;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class PDFGenerator
    {
        private ApplicationDbContext _context;

        public PDFGenerator()
        {
            _context = new ApplicationDbContext();
        }

        //生成文件到内存，并不真正输出
        public void GeneratePickDetailPdf(int pullSheetId)
        {
            var pullSheetInDb = _context.PullSheets
                .Include(c => c.PickDetails)
                .SingleOrDefault(x => x.Id == pullSheetId);

            var pickDetailList = pullSheetInDb.PickDetails.ToList();

            var pageCount = (int)Math.Ceiling((double)pickDetailList.Count / 20);
            var startIndex = 0;
            var totalPcs = pickDetailList.Sum(x => x.PickPcs);
            var totalCtns = pickDetailList.Sum(x => x.PickCtns);

            //定义字体
            var BF_light = BaseFont.CreateFont(@"C:\Windows\Fonts\simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            //设置页面大小
            //var rec = new Rectangle(PageSize.LETTER);

            //定义内存流
            using (var ms = new MemoryStream())
            //在doc构造函数中传入页面大小和设置页边距(左，右，上，下，单位是pt)
            using (var doc = new Document(PageSize.LETTER, 0, 0, 24, 24))
            //实际写入文件到内存流中
            using (var pw = PdfWriter.GetInstance(doc, ms))
            {
                doc.Open();     //打开文件

                var pickDetailCount = pickDetailList.Count;

                do
                {
                    //定义标题表，写入标题数据，头部包括名称，打印时间，拣货票范围，总箱数和总件数，总共两列三行，其中第一行和第二行为合并
                    #region
                    var tableTitle = new PdfPTable(2);

                    //定义标题段落
                    var title = new Paragraph("Picking List", new Font(BF_light, 30));
                    //设置段落的对齐方式(居中)
                    title.Alignment = Element.ALIGN_CENTER;
                    //将title段落放到cell中去
                    var titleCell = new PdfPCell(title);
                    titleCell.HorizontalAlignment = 1;    //0靠左，1居中，2靠右
                    titleCell.Colspan = 2;    //合并两列
                    titleCell.Border = Rectangle.NO_BORDER;
                    titleCell.MinimumHeight = 40f;

                    //添加titleCell到表中
                    tableTitle.AddCell(titleCell);

                    //添加打印时间到表中
                    var printTime = new Paragraph("Print Time: " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"), new Font(BF_light, 13));
                    printTime.Alignment = Element.ALIGN_CENTER;
                    var printTimeCell = new PdfPCell(printTime);
                    printTimeCell.HorizontalAlignment = 1;
                    printTimeCell.Colspan = 2;
                    printTimeCell.Border = Rectangle.NO_BORDER;
                    printTimeCell.MinimumHeight = 30f;

                    tableTitle.AddCell(printTimeCell);

                    //添加拣票范围到表中
                    var pickTicketsRange = new Paragraph("Pick Tickets: " + pullSheetInDb.PickTicketsRange, new Font(BF_light, 13));
                    pickTicketsRange.Alignment = Element.ALIGN_LEFT;
                    var pickTicketsCell = new PdfPCell(pickTicketsRange);
                    pickTicketsCell.HorizontalAlignment = 0;
                    pickTicketsCell.Border = Rectangle.NO_BORDER;
                    pickTicketsCell.MinimumHeight = 20f;

                    tableTitle.AddCell(pickTicketsCell);

                    //添加总件数、总箱数到表中
                    var total = new Paragraph("Total: " + totalPcs.ToString() + " Pcs" + "  " + totalCtns.ToString() + " Ctns", new Font(BF_light, 13));
                    total.Alignment = Element.ALIGN_RIGHT;
                    var totalCell = new PdfPCell(total);
                    totalCell.HorizontalAlignment = 2;
                    totalCell.Border = Rectangle.NO_BORDER;
                    totalCell.MinimumHeight = 20f;

                    tableTitle.AddCell(totalCell);

                    //添加表到页面中
                    tableTitle.WidthPercentage = 90f;
                    doc.Add(tableTitle);
                    #endregion

                    //定义正式内容表
                    var tableContent = new PdfPTable(11);
                    //tableContent.LockedWidth = true;
                    float[] columnWidth = {11f, 9f, 15f, 8f, 8f, 9f, 9f, 5f, 9f, 9f, 8f};
                    tableContent.SetWidthPercentage(columnWidth, PageSize.LETTER);
                    tableContent.WidthPercentage = 90f;

                    var headString = "Container,Cut PO,Style,Color,Customer,Size,Pcs,Pack,Pick Ctns,Pick Pcs,Location";

                    foreach(var head in headString.Split(','))
                    {
                        tableContent.AddCell(new PdfPCell(new Paragraph(head, new Font(BF_light, 9))));
                    }

                    for (int i = startIndex; i < startIndex + 20 && i < pickDetailList.Count; i++)
                    {
                        var firstCell = new PdfPCell(new Paragraph(pickDetailList[i].Container, new Font(BF_light, 9)));
                        firstCell.MinimumHeight = 25f;

                        tableContent.AddCell(firstCell);
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PurchaseOrder, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Style, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Color, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].CustomerCode, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].SizeBundle, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PcsBundle, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PcsPerCarton.ToString(), new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PickCtns.ToString(), new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PickPcs.ToString(), new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Location, new Font(BF_light, 9))));
                    }

                    doc.Add(tableContent);

                    pickDetailCount -= 20;
                    startIndex += 20;

                    //如果还有没放完的表内容，则新建一页
                    if (pickDetailCount > 0)
                    {
                        doc.NewPage();
                    }

                } while (pickDetailCount > 0);


                //循环到这里结束
                doc.Close();
                pw.Close();
                ms.Close();

                //输出到客户端
                //var bytes = ms.ToArray();
                //result = Convert.ToBase64String(bytes);
                var response = HttpContext.Current.Response;

                response.Clear();
                response.ContentType = "Application/pdf";
                response.AddHeader("content-disposition", "attachment; filename=" + pullSheetInDb.PickTicketsRange + ".pdf");
                response.BinaryWrite(ms.ToArray());
                //ms.WriteTo(response.OutputStream);    //这样也行
                response.Flush();
                response.Close();
                response.End();
            }
        }
    }
}