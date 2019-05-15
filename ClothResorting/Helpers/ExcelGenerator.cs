using ClothResorting.Models;
using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;
using System.Globalization;
using System.IO;
using System.Threading;
using ClothResorting.Models.DataTransferModels;
using org.in2bits.MyXls;

namespace ClothResorting.Helpers
{
    public class ExcelGenerator
    {
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Microsoft.Office.Interop.Excel.Workbook _wb;
        private Microsoft.Office.Interop.Excel.Worksheet _ws;

        public ExcelGenerator()
        {
            _context = new ApplicationDbContext();
        }

        public ExcelGenerator(string templatePath)
        {
            _context = new ApplicationDbContext();
            _path = templatePath;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        public void GenerateRecevingReportExcel(Container container, IList<FCReceivingReport> report)
        {
            var doc = new XlsDocument();
            doc.FileName = container.ContainerNumber + "-" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xls";
            var sheet = doc.Workbook.Worksheets.Add("Sheet1");
            var cells = sheet.Cells;

            //调整第1~4,7~15列宽度
            var col1_4 = new ColumnInfo(doc, sheet);
            var col7_15 = new ColumnInfo(doc, sheet);

            col1_4.ColumnIndexStart = 0;
            col1_4.ColumnIndexEnd = 3;
            col7_15.ColumnIndexStart = 6;
            col7_15.ColumnIndexEnd = 14;

            col1_4.Width = 16 * 256;
            col7_15.Width = 16 * 256;

            sheet.AddColumnInfo(col1_4);
            sheet.AddColumnInfo(col7_15);

            //定义合并单元格，合并从[1,1]到[2,15]的范围
            sheet.AddMergeArea(new MergeArea(1, 2, 1, 15));

            //创建题目单元格样式，垂直水平且居中
            var xfTitle = doc.NewXF();
            xfTitle.VerticalAlignment = VerticalAlignments.Centered;
            xfTitle.HorizontalAlignment = HorizontalAlignments.Centered;
            xfTitle.UseBorder = true;
            xfTitle.TopLineStyle = 1;
            xfTitle.BottomLineStyle = 1;
            xfTitle.LeftLineStyle = 1;
            xfTitle.RightLineStyle = 1;
            xfTitle.TopLineColor = Colors.Black;
            xfTitle.BottomLineColor = Colors.Black;
            xfTitle.RightLineColor = Colors.Black;
            xfTitle.LeftLineColor = Colors.Black;
            xfTitle.Font.Bold = true;
            xfTitle.Font.Height = 16 * 20;

            //创建内容单元格样式，垂直居中且水平居中
            var xf = doc.NewXF();
            xf.VerticalAlignment = VerticalAlignments.Centered;
            xf.HorizontalAlignment = HorizontalAlignments.Centered;
            xf.UseBorder = true;
            xf.TopLineStyle = 1;
            xf.BottomLineStyle = 1;
            xf.LeftLineStyle = 1;
            xf.RightLineStyle = 1;
            xf.TopLineColor = Colors.Black;
            xf.BottomLineColor = Colors.Black;
            xf.RightLineColor = Colors.Black;
            xf.LeftLineColor = Colors.Black;

            //标题
            cells.Add(1, 1, "RWL -RECEIVING REPORT-", xfTitle);

            //集装箱信息
            cells.Add(4, 1, "Vendor:", xf);
            cells.Add(5, 1, "RCVD DATE:", xf);
            cells.Add(6, 1, "CTNR:", xf);
            cells.Add(7, 1, "REFERENCE:", xf);
            cells.Add(8, 1, "RECEIPT #:", xf);
            cells.Add(9, 1, "TOTAL CTNS:", xf);
            cells.Add(10, 1, "REMARKS:", xf);

            //for (int i= 0; i < 7; i++)
            //{
            //    sheet.AddMergeArea(new MergeArea(i + 4, i + 4, 2, 3));
            //}

            cells.Add(4, 2, container.Vendor, xf);
            cells.Add(5, 2, container.ReceivedDate, xf);
            cells.Add(6, 2, container.ContainerNumber, xf);
            cells.Add(7, 2, container.Reference, xf);
            cells.Add(8, 2, container.ReceiptNumber, xf);
            cells.Add(9, 2, report.Sum(x => x.ReceivedCtns).ToString(), xf);
            cells.Add(10, 2, container.Remark, xf);

            //建立列
            var columnNames = "Sequence,Range,Cut Po,Style,Customer,Color,Size,Pcs,Receivable Qty,Inbound Qty,Receivable Ctns,Inbound Ctns,SKU,Memo,Comment";
            var index = 1;

            foreach(var columnName in columnNames.Split(','))
            {
                cells.Add(12, index, columnName, xf);
                index++;
            }

            //填充收货细节
            index = 0;
            foreach(var r in report)
            {
                cells.Add(13 + index, 1, r.Index, xf);
                cells.Add(13 + index, 2, r.CartonRange, xf);
                cells.Add(13 + index, 3, r.PurchaseOrder, xf);
                cells.Add(13 + index, 4, r.Style, xf);
                cells.Add(13 + index, 5, r.Customer, xf);
                cells.Add(13 + index, 6, r.Color, xf);
                cells.Add(13 + index, 7, r.SizeBundle, xf);
                cells.Add(13 + index, 8, r.PcsBundle, xf);
                cells.Add(13 + index, 9, r.ReceivableQty, xf);
                cells.Add(13 + index, 10, r.ReceivedQty, xf);
                cells.Add(13 + index, 11, r.ReceivableCtns, xf);
                cells.Add(13 + index, 12, r.ReceivedCtns, xf);
                cells.Add(13 + index, 13, r.SKU, xf);
                cells.Add(13 + index, 14, r.Memo, xf);
                cells.Add(13 + index, 15, r.Comment, xf);
                index++;
            }

            //表脚统计
            cells.Add(15 + index, 9, report.Sum(x => x.ReceivableQty).ToString(), xf);
            cells.Add(15 + index, 10, report.Sum(x => x.ReceivedQty).ToString(), xf);
            cells.Add(15 + index, 11, report.Sum(x => x.ReceivableCtns).ToString(), xf);
            cells.Add(15 + index, 12, report.Sum(x => x.ReceivedCtns).ToString(), xf);


            doc.Save(@"D:\ReceivingReport\");
            var fileName = doc.FileName;
            var path = @"D:\ReceivingReport\" + doc.FileName;
            var response = HttpContext.Current.Response;
            var downloadFile = new FileInfo(path);
            response.ClearHeaders();
            response.Buffer = false;
            response.ContentType = "application/octet-stream";
            response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            response.Clear();
            response.AppendHeader("Content-Length", downloadFile.Length.ToString());
            response.WriteFile(downloadFile.FullName);
            response.Flush();
            response.Close();
            response.End();

            ////字符流下载
            //var fs = new FileStream(path, FileMode.Open);
            //var bytes = new Byte[(int)fs.Length];
            //fs.Read(bytes, 0, bytes.Length);
            //fs.Close();
            //response.ContentType = "application/ms-excel";
            ////通知浏览器下载文件而不是打开
            //response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode("test", System.Text.Encoding.UTF8));
            //response.BinaryWrite(bytes);
            //response.Flush();
            //response.End();

            //response.Clear();
            //response.ClearHeaders();
            //response.ClearContent();
            //response.AddHeader("Content-Disposition", "attachment; filename=" + downloadFile.FullName);
            //response.AddHeader("Content-Length", downloadFile.Length.ToString());
            //response.ContentType = "application/ms-excel";
            //response.Flush();
            //response.TransmitFile(downloadFile.FullName);
            //response.End();

            //byte[] Content = File.ReadAllBytes(path);
            //response.ContentType = "application/octet-stream";
            //response.AddHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode("test", System.Text.Encoding.UTF8));
            //response.BufferOutput = true;
            //response.OutputStream.Write(Content, 0, Content.Length);
            //response.Flush();
            //response.End();
        }

        public void GenerateSearchResultsExcelFile(IList<RegularCartonDetail> searchResults, string container, string po, string color, string style, string customer, string size)
        {
            var doc = new XlsDocument();
            doc.FileName = "SearchResults" + "-" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xls";
            var sheet = doc.Workbook.Worksheets.Add("Sheet1");
            var cells = sheet.Cells;

            //调整第1~6,8~9,12~14列宽度
            var col1_6 = new ColumnInfo(doc, sheet);
            var col8_9 = new ColumnInfo(doc, sheet);
            var col12_14 = new ColumnInfo(doc, sheet);

            col1_6.ColumnIndexStart = 0;
            col1_6.ColumnIndexEnd = 5;
            col8_9.ColumnIndexStart = 7;
            col8_9.ColumnIndexEnd = 8;
            col12_14.ColumnIndexStart = 11;
            col12_14.ColumnIndexEnd = 13;

            col1_6.Width = 16 * 256;
            col8_9.Width = 16 * 256;
            col12_14.Width = 16 * 256;

            sheet.AddColumnInfo(col1_6);
            sheet.AddColumnInfo(col8_9);
            sheet.AddColumnInfo(col12_14);

            //定义合并单元格，合并从[1,1]到[2,14]的范围
            sheet.AddMergeArea(new MergeArea(1, 2, 1, 14));

            //创建题目单元格样式，垂直水平且居中
            var xfTitle = doc.NewXF();
            xfTitle.VerticalAlignment = VerticalAlignments.Centered;
            xfTitle.HorizontalAlignment = HorizontalAlignments.Centered;
            xfTitle.UseBorder = true;
            xfTitle.TopLineStyle = 1;
            xfTitle.BottomLineStyle = 1;
            xfTitle.LeftLineStyle = 1;
            xfTitle.RightLineStyle = 1;
            xfTitle.TopLineColor = Colors.Black;
            xfTitle.BottomLineColor = Colors.Black;
            xfTitle.RightLineColor = Colors.Black;
            xfTitle.LeftLineColor = Colors.Black;
            xfTitle.Font.Bold = true;
            xfTitle.Font.Height = 16 * 20;

            //创建内容单元格样式，垂直居中且水平居中
            var xf = doc.NewXF();
            xf.VerticalAlignment = VerticalAlignments.Centered;
            xf.HorizontalAlignment = HorizontalAlignments.Centered;
            xf.UseBorder = true;
            xf.TopLineStyle = 1;
            xf.BottomLineStyle = 1;
            xf.LeftLineStyle = 1;
            xf.RightLineStyle = 1;
            xf.TopLineColor = Colors.Black;
            xf.BottomLineColor = Colors.Black;
            xf.RightLineColor = Colors.Black;
            xf.LeftLineColor = Colors.Black;

            //标题
            cells.Add(1, 1, "Sorted by: Container=" + container + " && Cut PO=" + po + " && Style=" + style + " && Color=" + color + " && Customer=" + customer + " && Size=" + size, xfTitle);

            //建立列
            var columnNames = "Created By,Received By,Carton Range,Customer,Cut PO,Style,Color,Size Code,Pcs Code,Pack,Quantity,Received Pcs,Cartons,Received Ctns";
            var index = 1;

            foreach (var columnName in columnNames.Split(','))
            {
                cells.Add(3, index, columnName, xf);
                index++;
            }

            //填充收货细节
            index = 0;
            foreach (var r in searchResults)
            {
                cells.Add(4 + index, 1, r.Operator, xf);
                cells.Add(4 + index, 2, r.Receiver, xf);
                cells.Add(4 + index, 3, r.CartonRange, xf);
                cells.Add(4 + index, 4, r.Customer, xf);
                cells.Add(4 + index, 5, r.PurchaseOrder, xf);
                cells.Add(4 + index, 6, r.Style, xf);
                cells.Add(4 + index, 7, r.Color, xf);
                cells.Add(4 + index, 8, r.SizeBundle, xf);
                cells.Add(4 + index, 9, r.PcsBundle, xf);
                cells.Add(4 + index, 10, r.PcsPerCarton, xf);
                cells.Add(4 + index, 11, r.Quantity, xf);
                cells.Add(4 + index, 12, r.ActualPcs, xf);
                cells.Add(4 + index, 13, r.Cartons, xf);
                cells.Add(4 + index, 14, r.ActualCtns, xf);
                index++;
            }

            //表脚统计
            cells.Add(6 + index, 10, "Total:", xf);
            cells.Add(6 + index, 11, searchResults.Sum(x => x.Quantity).ToString(), xf);
            cells.Add(6 + index, 12, searchResults.Sum(x => x.ActualPcs).ToString(), xf);
            cells.Add(6 + index, 13, searchResults.Sum(x => x.Cartons).ToString(), xf);
            cells.Add(6 + index, 14, searchResults.Sum(x => x.ActualCtns).ToString(), xf);

            //下载
            doc.Save(@"D:\SearchResults\");
            var fileName = doc.FileName;
            var path = @"D:\SearchResults\" + doc.FileName;
            var response = HttpContext.Current.Response;
            var downloadFile = new FileInfo(path);
            response.ClearHeaders();
            response.Buffer = false;
            response.ContentType = "application/octet-stream";
            response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8));
            response.Clear();
            response.AppendHeader("Content-Length", downloadFile.Length.ToString());
            response.WriteFile(downloadFile.FullName);
            response.Flush();
            response.Close();
            response.End();
        }

        public string GenerateInventoryReportExcelFile(IList<InventoryReportDetail> inventoryList, string vendor)
        {
            var doc = new XlsDocument();
            doc.FileName = "InventoryReport" + "-" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xls";
            var sheet = doc.Workbook.Worksheets.Add("Sheet1");
            var cells = sheet.Cells;

            //调整第8~11列宽度
            var col1_11 = new ColumnInfo(doc, sheet);

            col1_11.ColumnIndexStart = 0;
            col1_11.ColumnIndexEnd = 10;

            col1_11.Width = 16 * 256;

            sheet.AddColumnInfo(col1_11);

            //定义合并单元格，合并从[1,1]到[2,14]的范围
            sheet.AddMergeArea(new MergeArea(1, 2, 1, 11));
            
            //创建题目单元格样式，垂直水平且居中
            var xfTitle = doc.NewXF();
            xfTitle.VerticalAlignment = VerticalAlignments.Centered;
            xfTitle.HorizontalAlignment = HorizontalAlignments.Centered;
            xfTitle.UseBorder = true;
            xfTitle.TopLineStyle = 1;
            xfTitle.BottomLineStyle = 1;
            xfTitle.LeftLineStyle = 1;
            xfTitle.RightLineStyle = 1;
            xfTitle.TopLineColor = Colors.Black;
            xfTitle.BottomLineColor = Colors.Black;
            xfTitle.RightLineColor = Colors.Black;
            xfTitle.LeftLineColor = Colors.Black;
            xfTitle.Font.Bold = true;
            xfTitle.Font.Height = 16 * 20;

            //创建内容单元格样式，垂直居中且水平居中
            var xf = doc.NewXF();
            xf.VerticalAlignment = VerticalAlignments.Centered;
            xf.HorizontalAlignment = HorizontalAlignments.Centered;
            xf.UseBorder = true;
            xf.TopLineStyle = 1;
            xf.BottomLineStyle = 1;
            xf.LeftLineStyle = 1;
            xf.RightLineStyle = 1;
            xf.TopLineColor = Colors.Black;
            xf.BottomLineColor = Colors.Black;
            xf.RightLineColor = Colors.Black;
            xf.LeftLineColor = Colors.Black;

            //标题
            cells.Add(1, 1, vendor + " Inventory Report", xfTitle);

            //建立列
            var columnNames = "Container,Status,Cut PO,Style,Color,Size,Pack,Original Ctns,Original Pcs,Available Ctns,Available Pcs,Pallet#";
            var index = 1;

            foreach (var columnName in columnNames.Split(','))
            {
                cells.Add(3, index, columnName, xf);
                index++;
            }

            //填充库存细节
            index = 0;
            foreach (var inventory in inventoryList)
            {
                cells.Add(4 + index, 1, inventory.Container ?? "N/A", xf);
                cells.Add(4 + index, 2, inventory.Status, xf);
                cells.Add(4 + index, 3, inventory.PurchaseOrder, xf);
                cells.Add(4 + index, 4, inventory.Style, xf);
                cells.Add(4 + index, 5, inventory.Color, xf);
                cells.Add(4 + index, 6, inventory.SizeBundle, xf);
                cells.Add(4 + index, 7, inventory.Pack ?? "N/A", xf);
                cells.Add(4 + index, 8, inventory.Cartons, xf);
                cells.Add(4 + index, 9, inventory.Quantity, xf);
                cells.Add(4 + index, 10, inventory.AvailableCtns, xf);
                cells.Add(4 + index, 11, inventory.AvailablePcs, xf);
                cells.Add(4 + index, 12, inventory.Batch, xf);
                index++;
            }

            //表脚统计
            cells.Add(6 + index, 7, "Total:", xf);
            cells.Add(6 + index, 8, inventoryList.Sum(x => x.Cartons).ToString(), xf);
            cells.Add(6 + index, 9, inventoryList.Sum(x => x.Quantity).ToString(), xf);
            cells.Add(6 + index, 10, inventoryList.Sum(x => x.AvailableCtns).ToString(), xf);
            cells.Add(6 + index, 11, inventoryList.Sum(x => x.AvailablePcs).ToString(), xf);

            //下载
            doc.Save(@"D:\SearchResults\");
            var fileName = doc.FileName;
            var path = @"D:\SearchResults\" + doc.FileName;

            return path;
        }

        public string GenerateInventoryReportExcelFileV2(IList<FCRegularLocationDetail> inventoryList, string sizeBundle)
        {
            inventoryList = CombineAndUnifyInventoryList(inventoryList);
            var sizeArray = sizeBundle.Split(',');

            if (sizeArray.Length <= 1)
            {
                sizeBundle = "XS,S,M,L,1X,2X,3X,4X,5X,6X,2T,3T,4T,PS,PM,PL,PXL,4,5,5/6,6,7,7/8,8,9,10,10/12,12,14,14/16,16,18/20";
                sizeArray = sizeBundle.Split(',');
            }

            _ws = _wb.Worksheets[1];

            for (int s = 0; s < sizeArray.Length; s++)
            {
                _ws.Cells[6, 5 + s] = sizeArray[s];
            }

            var poGroup = inventoryList.GroupBy(x => x.PurchaseOrder);
            var currentRow = 7;
            List<IGrouping<string, FCRegularLocationDetail>> colorGroup = new List<IGrouping<string, FCRegularLocationDetail>>();
            List<IGrouping<string, FCRegularLocationDetail>> styleGroup = new List<IGrouping<string, FCRegularLocationDetail>>();

            foreach (var p in poGroup)
            {
                var groupByStyle = p.GroupBy(x => x.Style);

                foreach(var s in groupByStyle)
                {
                    styleGroup.Add(s);
                }
            }

            foreach(var s in styleGroup)
            {
                var groupByColor = s.GroupBy(x => x.Color);

                foreach(var c in groupByColor)
                {
                    colorGroup.Add(c);
                }
            }

            foreach(var c in colorGroup)
            {
                _ws.Cells[currentRow, 1] = c.First().PurchaseOrder;
                _ws.Cells[currentRow, 2] = c.First().Style;
                _ws.Cells[currentRow, 3] = c.First().CustomerCode;
                _ws.Cells[currentRow, 4] = c.First().Color;

                foreach(var s in c)
                {
                    var size = s.SizeBundle;
                    var columnIndex = 5 + Array.IndexOf(sizeArray, size);
                    if (columnIndex == 4)
                    {
                        _ws.Cells[currentRow, 5] = "Unidentified Size:";
                        _ws.Cells[currentRow, 6] = size;
                        continue;
                    }

                    _ws.Cells[currentRow, columnIndex] = s.AvailablePcs;
                }

                currentRow += 1;
            }

            for(int i = 0; i < sizeArray.Length; i++)
            {
                var size = sizeArray[i];
                _ws.Cells[currentRow + 1, i + 5] = inventoryList.Where(x => x.SizeBundle == size).Sum(x => x.AvailablePcs);
            }

            _ws.Cells[currentRow + 3, 1] = "Total Pcs:";
            var sum3 = inventoryList.Sum(x => x.AvailablePcs);
            _ws.Cells[currentRow + 3, 2] = sum3;

            var fullPath = @"D:\InventoryReport\InventoryReport-" + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xlsx";

            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return fullPath;
        }

        private string UnifySize(string size)
        {
            if (size == "xs")
            {
                return "XS";
            }
            else if (size == "XL" || size == "1XL")
            {
                return "1X";
            }
            else if (size == "XXL" || size == "2XL" || size == "2x")
            {
                return "2X";
            }
            else if (size == "XXXL" || size == "3XL" || size == "3x")
            {
                return "3X";
            }
            else if (size == "XXXXL" || size == "4XL" || size == "4x")
            {
                return "4X";
            }
            else if (size == "L(6X)")
            {
                return "4X";
            }
            else if (size == "S(4)")
            {
                return "4";
            }
            else if (size == "M(5/6)")
            {
                return "5/6";
            }
            else if (size == "SIZE 6")
            {
                return "6";
            }
            else if (size == "SIZE 7")
            {
                return "7";
            }
            else if (size == "S(7/8)")
            {
                return "7/8";
            }
            else if (size == "SIZE 8" || size == "S(8)")
            {
                return "8";
            }
            else if (size == "SIZE 10")
            {
                return "10";
            }
            else if (size == "M(10/12)")
            {
                return "10/12";
            }
            else if (size == "SIZE 12")
            {
                return "12";
            }
            else if (size == "M(12/14)" || size == "L(12/14)")
            {
                return "12/14";
            }
            else if (size == "SIZE 14" || size == "L(14)")
            {
                return "14";
            }
            else if (size == "L(14/16)" )
            {
                return "14/16";
            }
            else if (size == "SIZE 16" || size == "XL(16)")
            {
                return "16";
            }
            else if (size == "XL(18/20)")
            {
                return "18/20";
            }
            else
            {
                return size;
            }
        }

        private List<FCRegularLocationDetail> CombineAndUnifyInventoryList(IEnumerable<FCRegularLocationDetail> inventoryList)
        {
            var newList = new List<FCRegularLocationDetail>();
            foreach (var i in inventoryList)
            {
                i.SizeBundle = UnifySize(i.SizeBundle);
                var sameItem = newList.SingleOrDefault(x => x.PurchaseOrder == i.PurchaseOrder && x.Style == i.Style && x.Color == i.Color && x.SizeBundle == i.SizeBundle);

                if ( sameItem == null)
                {
                    newList.Add(new FCRegularLocationDetail {
                        PurchaseOrder = i.PurchaseOrder,
                        Style = i.Style,
                        Color = i.Color,
                        CustomerCode = i.CustomerCode,
                        SizeBundle = i.SizeBundle,
                        AvailablePcs = i.AvailablePcs
                    });
                }
                else
                {
                    sameItem.AvailablePcs += i.AvailablePcs;
                }
            }

            return newList;
        }
    }
}