using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.in2bits.MyXls;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System.IO;

namespace ClothResorting.Helpers
{
    public class ExcelGenerator
    {
        private ApplicationDbContext _context;

        public ExcelGenerator()
        {
            _context = new ApplicationDbContext();
        }

        public void GenerateRecevingReportExcel(Container container, IList<FCReceivingReport> report)
        {
            var doc = new XlsDocument();
            doc.FileName = container.ContainerNumber + "-" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xls";
            var sheet = doc.Workbook.Worksheets.Add("Sheet1");
            var cells = sheet.Cells;

            //定义合并单元格，合并从[1,1]到[2,14]的范围
            sheet.AddMergeArea(new MergeArea(1, 2, 1, 14));

            //创建单元格样式，垂直居中且水平居中
            var xf = doc.NewXF();
            xf.VerticalAlignment = VerticalAlignments.Centered;
            xf.HorizontalAlignment = HorizontalAlignments.Centered;

            //标题
            cells.Add(1, 1, "RWL -RECEIVING REPORT-", xf);

            //集装箱信息
            cells.Add(4, 1, "Vendor:", xf);
            cells.Add(5, 1, "RCVD DATE:", xf);
            cells.Add(6, 1, "CTNR:", xf);
            cells.Add(7, 1, "REFERENCE:", xf);
            cells.Add(8, 1, "RECEIPT #:", xf);
            cells.Add(9, 1, "TOTAL CTNS:", xf);
            cells.Add(10, 1, "REMARKS:", xf);

            for (int i= 0; i < 7; i++)
            {
                sheet.AddMergeArea(new MergeArea(i + 4, i + 4, 2, 3));
            }

            cells.Add(4, 2, container.Vendor, xf);
            cells.Add(5, 2, container.ReceivedDate, xf);
            cells.Add(6, 2, container.ContainerNumber, xf);
            cells.Add(7, 2, container.Reference, xf);
            cells.Add(8, 2, container.ReceiptNumber, xf);
            cells.Add(9, 2, report.Sum(x => x.ReceivedCtns).ToString(), xf);
            cells.Add(10, 2, container.Remark, xf);

            //建立列
            var columnNames = "Sequence,Range,Cut Po,Style,Customer,Color,Size,Pcs,Receivable Qty,Inbound Qty,Receivable Ctns,Inbound Ctns,Memo,Comment";
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
                cells.Add(13 + index, 13, r.Memo, xf);
                cells.Add(13 + index, 14, r.Comment, xf);
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
    }
}