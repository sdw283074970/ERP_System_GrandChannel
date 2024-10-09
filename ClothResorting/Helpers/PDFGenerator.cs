﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using ClothResorting.Models;
using System.Data.Entity;
using Spire.Pdf.Tables;
using ClothResorting.Models.FBAModels;
using SautinSoft;

namespace ClothResorting.Helpers
{
    public class PDFGenerator
    {
        private ApplicationDbContext _context;

        public PDFGenerator()
        {
            _context = new ApplicationDbContext();
        }

        //生成文件到内存，并下载
        public void GeneratePickDetailPdf(int pullSheetId)
        {
            var pullSheetInDb = _context.ShipOrders
                .Include(c => c.PickDetails)
                .SingleOrDefault(x => x.Id == pullSheetId);

            var pickDetailList = pullSheetInDb.PickDetails.ToList();

            var pageCount = (int)Math.Ceiling((double)pickDetailList.Count / 20);
            var startIndex = 0;
            var totalPcs = pickDetailList.Sum(x => x.PickPcs);
            var totalCtns = pickDetailList.Sum(x => x.PickCtns);
            var currentPage = 0;

            //定义字体
            var BF_light = BaseFont.CreateFont(@"C:\Windows\Fonts\simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            //设置页面大小
            //var rec = new Rectangle(PageSize.LETTER);

            //定义内存流
            using (var ms = new MemoryStream())
            //在doc构造函数中传入页面大小和设置页边距(左，右，上，下，单位是pt)
            using (var doc = new Document(PageSize.LETTER, 0, 0, 0, 0))
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
                    ++currentPage;

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
                    var tableContent = new PdfPTable(13);
                    //tableContent.LockedWidth = true;
                    float[] columnWidth = {10f, 8f, 6f, 11f, 5f, 8f, 8f, 5f, 5f, 9f, 9f, 9f, 7f};
                    tableContent.SetWidthPercentage(columnWidth, PageSize.LETTER);
                    tableContent.WidthPercentage = 90f;

                    var headString = "Container,Cut PO,Range,Style,Color,Customer,Size,Pcs,Pack,Pick Ctns,Pick Pcs,Location,Memo";

                    foreach(var head in headString.Split(','))
                    {
                        tableContent.AddCell(new PdfPCell(new Paragraph(head, new Font(BF_light, 9))));
                    }

                    for (int i = startIndex; i < startIndex + 20 && i < pickDetailList.Count; i++)
                    {
                        if (pickDetailList[i].PickPcs == 0)
                        {
                            continue;
                        }

                        var firstCell = new PdfPCell(new Paragraph(pickDetailList[i].Container, new Font(BF_light, 9)));
                        firstCell.MinimumHeight = 25f;

                        tableContent.AddCell(firstCell);
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PurchaseOrder, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].CartonRange, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Style, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Color, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].CustomerCode, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].SizeBundle, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PcsBundle, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PcsPerCarton.ToString(), new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PickCtns.ToString(), new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].PickPcs.ToString(), new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Location, new Font(BF_light, 9))));
                        tableContent.AddCell(new PdfPCell(new Paragraph(pickDetailList[i].Memo, new Font(BF_light, 9))));
                    }

                    doc.Add(tableContent);

                    //加页脚
                    var footer = new PDFFooter();

                    footer.OnEndPageNumber(pw, doc, currentPage, pageCount);

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

        //生成Label文件至内存，并下载
        public void GenerateLabelPdf(string container)
        {
            var cartonDetails = _context.RegularCartonDetails
                .Include(x => x.POSummary.RegularCartonDetails)
                .Where(x => x.Container == container && x.Cartons != 0)
                .ToList();

            //var poSummariesInDb = _context.POSummaries
            //    .Include(x => x.RegularCartonDetails)
            //    .Where(x => x.Container == container);

            var parser = new StringParser();

            //定义字体
            var BF_light = BaseFont.CreateFont(@"C:\Windows\Fonts\simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            //设置页面大小
            var rec = new Rectangle(432, 288, 90);

            //定义内存流
            using (var ms = new MemoryStream())
            //在doc构造函数中传入页面大小和设置页边距(左，右，上，下，单位是pt)
            using (var doc = new Document(rec, 0, 0, 14, 14))
            //实际写入文件到内存流中
            using (var pw = PdfWriter.GetInstance(doc, ms))
            {
                doc.Open();     //打开文件

                foreach (var c in cartonDetails)
                {
                    var font = new Font(BF_light, 20);
                    var font2 = new Font(BF_light, 28);
                    var size = CombineSize(c);

                    var containerCell = CreateTableCell("CTNR#: " + c.Container, font2, 1);
                    containerCell.Colspan = 2;
                    containerCell.HorizontalAlignment = 1;

                    var sizeCell = CreateTableCell("SIZE: " + size, font, 0);
                    var rangeCell = CreateTableCell("CTN RANGE: " + c.CartonRange, font, 0);
                    var poCell = CreateTableCell("PO#: " + c.PurchaseOrder, font, 0);
                    var styleCell = CreateTableCell("STYLE: " + c.Style, font, 0);
                    var colorCell = CreateTableCell("COLOR: " + c.Color + "/" + c.ColorCode, font, 0);
                    var remarkCell = CreateTableCell("REMARK:", font, 0);

                    if (c.PreLocation != null)
                    {
                        var locations = parser.ParseStrToPreLoc(c.PreLocation);

                        foreach (var l in locations)
                        {
                            var infoTable = new PdfPTable(2);

                            var ctnCell = CreateTableCell("CTNS: " + (l.Plts == 1 ? l.Ctns.ToString() : l.Ctns + "X" + l.Plts), font, 0);

                            var locationCell = CreateTableCell("LOC: " + l.Location, font2, 1);
                            locationCell.Colspan = 2;
                            locationCell.HorizontalAlignment = 1;

                            //单元格顺序
                            infoTable.AddCell(containerCell);
                            infoTable.AddCell(styleCell);
                            infoTable.AddCell(poCell);
                            infoTable.AddCell(colorCell);
                            infoTable.AddCell(sizeCell);
                            infoTable.AddCell(ctnCell);
                            infoTable.AddCell(remarkCell);
                            infoTable.AddCell(locationCell);
                            //infoTable.AddCell(rangeCell);

                            infoTable.WidthPercentage = 90f;

                            doc.Add(infoTable);
                            doc.NewPage();
                        }
                    }
                    else
                    {
                        var infoTable = new PdfPTable(2);

                        var ctnCell = CreateTableCell("CTNS: " + c.Cartons, font, 0);

                        var locationCell = CreateTableCell("LOC: N/A", font2, 1);
                        locationCell.Colspan = 2;
                        locationCell.HorizontalAlignment = 1;

                        //单元格顺序
                        infoTable.AddCell(containerCell);
                        infoTable.AddCell(styleCell);
                        infoTable.AddCell(poCell);
                        infoTable.AddCell(colorCell);
                        infoTable.AddCell(sizeCell);
                        infoTable.AddCell(ctnCell);
                        infoTable.AddCell(remarkCell);
                        infoTable.AddCell(locationCell);
                        //infoTable.AddCell(rangeCell);

                        infoTable.WidthPercentage = 90f;

                        doc.Add(infoTable);
                        doc.NewPage();
                    }
                }

                //循环到这里结束
                doc.Close();
                pw.Close();
                ms.Close();

                //输出到客户端
                var response = HttpContext.Current.Response;

                response.Clear();
                response.ContentType = "Application/pdf";
                response.AddHeader("content-disposition", "attachment; filename=" + container + "-Labels.pdf");
                response.BinaryWrite(ms.ToArray());
                //ms.WriteTo(response.OutputStream);    //这样也行
                response.Flush();
                response.Close();
                response.End();
            }

        }

        //基于PDF模板生成FBA系统中的BOL
        //读取并编辑PDF文件思路为：读取PDF模板内容，将模板内容添加到新PDF对象中，再对新PDF对象进行编辑而不是直接编辑原PDF本身
        public string GenerateFBABOL(int shipOrderId, IList<FBABOLDetail> bolDetailList)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var addressBookInDb = _context.FBAAddressBooks.SingleOrDefault(x => x.WarehouseCode == shipOrderInDb.Destination);
            var address = " ";

            if (addressBookInDb != null)
            {
                address = addressBookInDb.Address;
            }

            //读取原pdf模板文件
            PdfReader reader = new PdfReader(@"E:\Template\BOL-Origin.pdf");

            //获取首页的大小
            Rectangle psize = reader.GetPageSize(1);

            float width = psize.Width;
            float heright = psize.Height;

            //定义字体
            var BF_light = BaseFont.CreateFont(@"C:\Windows\Fonts\simsun.ttc,0", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            var fileName = shipOrderInDb.ShipOrderNumber + "-" + DateTime.Now.ToString("MMddyyhhmmss") + "-BOL.pdf";

            //以上也可写为如下
            //using (var ms = new MemoryStream())
            using (var fs = new FileStream(@"E:\BOL\" + fileName, FileMode.OpenOrCreate))
            using (var doc = new Document(psize))
            using (var pw = PdfWriter.GetInstance(doc, fs))
            {

                //打开新建的pdf文档对象
                doc.Open();

                //定义新建pdf文件内容，默认为空
                PdfContentByte cb = pw.DirectContent;

                //将读取的pdf模板内容添加到新pdf文件cb中
                PdfImportedPage importedPage = pw.GetImportedPage(reader, 1);
                cb.AddTemplate(importedPage, 0, 0);     //不调节旋转

                #region 其他单项表
                //日期
                AttachBordlessSingleCellTable(BF_light, cb, shipOrderInDb.ETS.ToString("MM/dd/yyyy"), 20f, 100f, 36.5f, 805f);

                //仓库代码
                AttachBordlessSingleCellTable(BF_light, cb, shipOrderInDb.Destination, 20f, 100f, 95f, 713.5f);

                //仓库地址
                AttachBordlessSingleCellTable(BF_light, cb, address, 20f, 250f, 40f, 690f);

                //BOL号
                AttachBordlessSingleCellTable(BF_light, cb, shipOrderInDb.CustomerCode + shipOrderInDb.ETS.ToString("yyyyMMdd"), 20f, 160f, 390f, 789f);

                //承运方
                AttachBordlessSingleCellTable(BF_light, cb, shipOrderInDb.Carrier, 20f, 200f, 355f, 741f);

                //出货单编号
                AttachBordlessSingleCellTable(BF_light, cb, "Ship Order #: " + shipOrderInDb.ShipOrderNumber, 20f, 200f, 50f, 500f);
                #endregion

                #region 内容表
                var detailCount = bolDetailList.Count();
                var minHeight = 173f / detailCount;     //总高度211f, 表头12f, 列名12f, 表脚12f

                //添加表头
                AttachSingleCellTable(BF_light, cb, "CUSTOMER ORDER INFORMATION", 12f, 521.3f, 36.5f, 466f);

                //添加内容
                AttachTable(psize, BF_light, cb, bolDetailList, minHeight, 36.5f, 454f);

                //添加表脚
                AttachTableFoot(psize, BF_light, cb, bolDetailList, 36.5f, 268f);

                ////添加拣货/出货表格到doc中
                //var table = new PdfPTable(1);
                //var content = new Paragraph("TEST", new Font(BF_light, 30));
                //var cell = new PdfPCell(content);

                //content.Alignment = Element.ALIGN_CENTER;
                //cell.BorderColor = BaseColor.GRAY;
                //cell.HorizontalAlignment = 1;    //0靠左，1居中，2靠右
                //cell.MinimumHeight = 211f;

                ////添加cell到表中
                //table.AddCell(cell);

                ////添加表到页面中
                //table.TotalWidth = 521.3f;
                //table.WriteSelectedRows(0, -1, 36.5f, 466f, cb);
                #endregion

                doc.Close();
                pw.Close();
                fs.Close();
                //ms.Close();

                ////输出到客户端
                //var response = HttpContext.Current.Response;

                //response.ClearContent();
                //response.ContentType = "Application/pdf";
                //response.AddHeader("Content-Disposition", "attachment; filename=" + shipOrderInDb.CustomerCode + shipOrderInDb.ETS.ToString("yyyyMMdd") + ".pdf");
                //response.BinaryWrite(ms.ToArray());
                ////ms.WriteTo(response.OutputStream);    //这样也行
                //response.Flush();
                //response.Close();
                //response.End();
            }

            return fileName;
        }

        private void AttachBordlessSingleCellTable(BaseFont font, PdfContentByte cb, string contentText, float tableHight, float tableWidth, float xPosition, float yPosition)
        {
            //添加拣货/出货表格到doc中
            var table = new PdfPTable(1);
            var content = new Paragraph(contentText, new Font(font, 8));
            var cell = new PdfPCell(content);

            content.Alignment = Element.ALIGN_CENTER;
            cell.Border = Rectangle.NO_BORDER;
            cell.HorizontalAlignment = 0;    //0靠左，1居中，2靠右
            cell.MinimumHeight = tableHight;

            //添加cell到表中
            table.AddCell(cell);

            //添加表到页面中
            table.TotalWidth = tableWidth;
            table.WriteSelectedRows(0, -1, xPosition, yPosition, cb);
        }

        private void AttachSingleCellTable(BaseFont font, PdfContentByte cb, string contentText, float tableHight, float tableWidth, float xPosition, float yPosition)
        {
            //添加拣货/出货表格到doc中
            var table = new PdfPTable(1);
            var content = new Paragraph(contentText, new Font(font, 8));
            var cell = new PdfPCell(content);

            content.Alignment = Element.ALIGN_CENTER;
            cell.BorderColor = BaseColor.GRAY;
            cell.HorizontalAlignment = 1;    //0靠左，1居中，2靠右
            cell.MinimumHeight = tableHight;

            //添加cell到表中
            table.AddCell(cell);

            //添加表到页面中
            table.TotalWidth = tableWidth;
            table.WriteSelectedRows(0, -1, xPosition, yPosition, cb);
        }

        private void AttachTable(Rectangle size, BaseFont font, PdfContentByte cb, IList<FBABOLDetail> list, float minHeight, float xPosition, float yPosition)
        {
            var tableContent = new PdfPTable(7);
            float[] columnWidth = { 30f, 14f, 14f, 10f, 10f, 10f, 12f };
            tableContent.SetWidthPercentage(columnWidth, size);
            tableContent.TotalWidth = 521.3f;

            //表列名部分
            var headString = "Customer Order No.,Container#,Pallet/Slip,Weight,Pkg Qty,Plt Qty,Location";
            foreach (var head in headString.Split(','))
            {
                var cell = new PdfPCell(new Paragraph(head, new Font(font, 9)));
                cell.MinimumHeight = 12f;
                cell.HorizontalAlignment = 1;
                cell.BorderColor = BaseColor.GRAY;
                tableContent.AddCell(cell);
            }

            //根据行数调整字体大小
            float fontSize = 9f;
            if (list.Count > 8 || list.Count < 10)
            {
                fontSize = 7f;
            }
            else if (list.Count >= 10 || list.Count < 13)
            {
                fontSize = 5f;
            }
            else if (list.Count >= 13 || list.Count < 15)
            {
                fontSize = 3f;
            }
            else if (list.Count >= 15)
            {
                fontSize = 1f;
            }

            //表内容部分
            for (int i = 0; i < list.Count; i++)
            {
                var orderNumberCell = new PdfPCell(new Paragraph(list[i].CustomerOrderNumber, new Font(font , fontSize)));
                var containerCell = new PdfPCell(new Paragraph(list[i].Contianer, new Font(font , fontSize)));
                var palletSlipCell = new PdfPCell(new Paragraph("Y     N", new Font(font, fontSize)));
                var weightCell = new PdfPCell(new Paragraph(list[i].Weight.ToString(), new Font(font, fontSize)));
                var quantityCell = new PdfPCell(new Paragraph(list[i].CartonQuantity == 0 ? " " : list[i].CartonQuantity.ToString(), new Font(font, fontSize)));

                orderNumberCell.MinimumHeight = minHeight;
                orderNumberCell.HorizontalAlignment = 1;
                containerCell.MinimumHeight = minHeight;
                containerCell.HorizontalAlignment = 1;
                palletSlipCell.MinimumHeight = minHeight;
                palletSlipCell.HorizontalAlignment = 1;
                weightCell.MinimumHeight = minHeight;
                weightCell.HorizontalAlignment = 1;
                quantityCell.MinimumHeight = minHeight;
                quantityCell.HorizontalAlignment = 1;

                tableContent.AddCell(orderNumberCell);
                tableContent.AddCell(containerCell);
                tableContent.AddCell(palletSlipCell);
                tableContent.AddCell(weightCell);
                tableContent.AddCell(quantityCell);


                //AddNewCell(tableContent, list[i].CustoerOrderNumber, minHeight, font);
                //AddNewCell(tableContent, list[i].Contianer, minHeight, font);
                //AddNewCell(tableContent, "Y     N", minHeight, font);
                //AddNewCell(tableContent, list[i].Weight.ToString(), minHeight, font);
                //AddNewCell(tableContent, list[i].CartonQuantity == 0 ? " " : list[i].CartonQuantity.ToString(), minHeight, font);

                //判定是否手动合并同一托盘的单元格
                if (list[i].ActualPallets == 99999)
                {
                    var tableCell = new PdfPCell(new Paragraph(" ", new Font(font, fontSize)));
                    tableCell.UseVariableBorders = true;
                    tableCell.MinimumHeight = minHeight;
                    tableCell.BorderColorTop = BaseColor.WHITE;
                    tableCell.BorderColorRight = BaseColor.GRAY;
                    tableCell.BorderColorBottom = BaseColor.GRAY;
                    tableCell.BorderColorLeft = BaseColor.GRAY;
                    tableContent.AddCell(tableCell);
                }
                else
                {
                    if (i != list.Count - 1 && list[i + 1].ActualPallets == 99999)
                    {
                        var p = new Paragraph(list[i].ActualPallets.ToString(), new Font(font, fontSize));
                        var tableCell = new PdfPCell(p);
                        tableCell.UseVariableBorders = true;
                        tableCell.BorderColorTop = BaseColor.GRAY;
                        tableCell.MinimumHeight = minHeight;
                        tableCell.BorderColorRight = BaseColor.GRAY;
                        tableCell.BorderColorBottom = BaseColor.WHITE;
                        tableCell.BorderColorLeft = BaseColor.GRAY;
                        tableCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        tableCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        tableContent.AddCell(tableCell);
                    }
                    else
                    {
                        AddNewCell(tableContent, list[i].ActualPallets == 0 ? " " : list[i].ActualPallets.ToString(), minHeight, font);
                    }
                }
                var locationrCell = new PdfPCell(new Paragraph(list[i].Location, new Font(font, fontSize)));
                locationrCell.MinimumHeight = minHeight;
                locationrCell.HorizontalAlignment = 1;
                tableContent.AddCell(locationrCell);

                //AddNewCell(tableContent, list[i].Location, minHeight, font);
            }

            tableContent.WriteSelectedRows(0, -1, xPosition, yPosition, cb);
        }

        public static string RemoveLastTwoPages(string path)
        {
            var outputPath = path.Split('.')[0] + "-result.pdf";
            using (Stream resultPDFOutputStream = new FileStream(outputPath, FileMode.Create))
            {
                PdfReader pdfReader = new PdfReader(path);
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, resultPDFOutputStream);

                var totalPages = pdfReader.NumberOfPages;

                if (totalPages < 3)
                {
                    pdfReader.SelectPages("1");
                }
                else
                {
                    pdfReader.SelectPages("1-" + (totalPages - 2).ToString());
                }

                PdfStamper pdfStamper = new PdfStamper(pdfReader, resultPDFOutputStream);

                for (int i = 1; i <= totalPages - 2; i++)
                {
                    PdfContentByte contentByte = pdfStamper.GetOverContent(i);
                    PdfDocument doc = contentByte.PdfDocument;

                    float X = 0.0f;
                    float Y = -25.0f;
                    float Height = Utilities.InchesToPoints(0.50f);
                    float Width = Utilities.InchesToPoints(10.0f);

                    float llx = (doc.Left - doc.LeftMargin) + X;
                    float lly = (doc.Top - doc.TopMargin) - (Height + Y);
                    float urx = (doc.Left - doc.LeftMargin) + Width + X;
                    float ury = (doc.Top - doc.TopMargin) - Y;

                    Rectangle rectangle = new Rectangle(llx, lly, urx, ury)
                    {
                        BackgroundColor = BaseColor.WHITE
                    };

                    contentByte.Rectangle(rectangle);
                }

                pdfStamper.Close();
                pdfReader.Close();

                return outputPath;
            }
        }

        //public static string RemoveHeader(string path)
        //{

        //}

        public static string ConvertExcelAsMemoryStream(string path)
        {
            // Convert Excel to PDF in memory  
            ExcelToPdf x = new ExcelToPdf();

            // Set PDF as output format.  
            x.OutputFormat = SautinSoft.ExcelToPdf.eOutputFormat.Pdf;

            string excelFile = path;
            string pdfFile = Path.ChangeExtension(excelFile, ".pdf");
            byte[] pdfBytes = null;

            try
            {
                // Let us say, we have a memory stream with Excel data.  
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(excelFile)))
                {
                    pdfBytes = x.ConvertBytes(ms.ToArray());
                }
                // Save pdfBytes to a file for demonstration purposes.  
                File.WriteAllBytes(pdfFile, pdfBytes);
                System.Diagnostics.Process.Start(pdfFile);
                return pdfFile;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void AttachTableFoot(Rectangle size, BaseFont font, PdfContentByte cb, IList<FBABOLDetail> list, float xPosition, float yPosition)
        {
            var tableFoot = new PdfPTable(7);
            float[] columnWidth = { 30f, 14f, 14f, 10f, 10f, 10f, 12f };
            tableFoot.SetWidthPercentage(columnWidth, size);
            tableFoot.TotalWidth = 521.3f;

            //表脚部分

            AddNewCell(tableFoot, "Grand Total", 10f, font);
            AddNewCell(tableFoot, " ", 10f, font);
            AddNewCell(tableFoot, " ", 10f, font);
            AddNewCell(tableFoot, list.Sum(x => x.Weight).ToString(), 10f, font);
            AddNewCell(tableFoot, list.Sum(x => x.CartonQuantity).ToString(), 10f, font);
            AddNewCell(tableFoot, list.Where(x => x.ActualPallets != 99999).Sum(x => x.ActualPallets).ToString(), 10f, font);
            AddNewCell(tableFoot, " ", 10f, font);

            tableFoot.WriteSelectedRows(0, -1, xPosition, yPosition, cb);
        }

        private void AddNewCell(PdfPTable table, string content, float minHeight, BaseFont font)
        {
            var cell = new PdfPCell(new PdfPCell(new Paragraph(content, new Font(font, 9))));
            cell.HorizontalAlignment = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.MinimumHeight = minHeight;
            cell.BorderColor = BaseColor.GRAY;
            table.AddCell(cell);
        }

        private string CombineSize(RegularCartonDetail cartonDetail)
        {
            var summary = cartonDetail.POSummary;

            var oneBoxItems = summary.RegularCartonDetails
                .Where(x => x.CartonRange == cartonDetail.CartonRange
                    && x.Batch == cartonDetail.Batch);

            var size = string.Empty;

            foreach(var c in oneBoxItems)
            {
                size += c.SizeBundle + "/" + c.PcsBundle + " ";
            }

            return size;
        }

        private PdfPCell CreateTableCell(string str, Font font, int position)
        {
            var p = new Paragraph(str, font);
            p.Alignment = Element.ALIGN_LEFT;
            var cell = new PdfPCell(p);
            cell.HorizontalAlignment = position;    //0靠左，1居中，2靠右
            //cell.Border = Rectangle.NO_BORDER;
            //cell.Border = Rectangle.BOTTOM_BORDER;
            cell.MinimumHeight = 44f;

            return cell;
        }
    }

    //页脚类
    public class PDFFooter : PdfPageEventHelper
    {
        //在pdf文件的顶上写入y页眉
        public override void OnOpenDocument(PdfWriter pw, Document doc)
        {
            base.OnOpenDocument(pw, doc);
            PdfPTable table = new PdfPTable(new float[] { 1f });
            table.SpacingAfter = 10f;
            table.TotalWidth = 300f;
            var cell = new PdfPCell(new Phrase("Header"));
            table.AddCell(cell);
            table.WriteSelectedRows(0, -1, 150, doc.Top, pw.DirectContent);
        }

        //在pdf文件中每一页的开始处写入页眉
        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
        }

        //在pdf文件中每一页的末尾写入页脚
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            var table = new PdfPTable(1);
            table.SetWidthPercentage(new float[] { 100f }, PageSize.LETTER);
            table.WidthPercentage = 90f;
            table.DefaultCell.Border = 0;

            var cell = new PdfPCell(new Phrase("@ " + DateTime.Now.Year + ". Grand Channel Inc. All Rights Reserved."));
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            table.AddCell(cell);
            table.WriteSelectedRows(0, -1, 0, document.Bottom, writer.DirectContent);
        }

        //在pdf文件中的末尾写入页脚
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);
        }

        //在PDF文件中每一页的右下角写入页码
        public void OnEndPageNumber(PdfWriter pw, Document doc, int currentPage, int pageCount)
        {
            var table = new PdfPTable(1);
            table.SetWidthPercentage(new float[] { 100f }, PageSize.LETTER);
            table.WidthPercentage = 90f;

            var footer = new Paragraph("Page " + currentPage.ToString() + "/" + pageCount.ToString());
            footer.Alignment = Element.ALIGN_RIGHT;
            var cell = new PdfPCell(footer);
            cell.HorizontalAlignment = 1;
            cell.Border = Rectangle.NO_BORDER;
            table.AddCell(cell);
            //doc.Add(table);
            table.WriteSelectedRows(0, -1, doc.Right - 100f, doc.Bottom, pw.DirectContent);
        }
    }
}