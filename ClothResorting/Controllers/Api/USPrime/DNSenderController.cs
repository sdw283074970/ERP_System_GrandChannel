using Aspose.Cells;
using AutoMapper;
using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.USPrime
{
    //[RoutePrefix("Api/usprime")]
    public class DNSenderController : ApiController
    {
        private ApplicationDbContext _context;

        public DNSenderController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/dnsender/
        [HttpPost]
        public IHttpActionResult UploadAndReadXlsxFile()
        {
            var orders = new List<USPrimeOrder>();

            #region Variable Declaration  
            string message = "";
            HttpResponseMessage ResponseMessage = null;
            var httpRequest = HttpContext.Current.Request;
            DataSet dsexcelRecords = new DataSet();
            IExcelDataReader reader = null;
            HttpPostedFile Inputfile = null;
            Stream FileStream = null;
            #endregion

            if (httpRequest.Files.Count > 0)
            {
                Inputfile = httpRequest.Files[0];
                FileStream = Inputfile.InputStream;

                if (Inputfile != null && FileStream != null)
                {
                    if (Inputfile.FileName.EndsWith(".xls"))
                        reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                    else if (Inputfile.FileName.EndsWith(".xlsx"))
                        reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);
                    else
                        message = "The file format is not supported.";

                    dsexcelRecords = reader.AsDataSet();
                    reader.Close();

                    if (dsexcelRecords != null && dsexcelRecords.Tables.Count > 0)
                    {
                        DataTable dtOrderRecords = dsexcelRecords.Tables[0];
                        for (int i = 1; i < dtOrderRecords.Rows.Count; i++)
                        {
                            var order = new USPrimeOrder();
                            order.hblNumber = Convert.ToString(dtOrderRecords.Rows[i][0]);
                            order.mblNumber = Convert.ToString(dtOrderRecords.Rows[i][1]);
                            order.totalCC = (float)Convert.ToDouble(dtOrderRecords.Rows[i][37]);
                            order.truckingFee = (float)Convert.ToDouble(dtOrderRecords.Rows[i][39]);
                            order.profitShare = (float)Convert.ToDouble(dtOrderRecords.Rows[i][40]);
                            order.handlingFee = (float)Convert.ToDouble(dtOrderRecords.Rows[i][41]);
                            order.balanceToOrigin = (float)Convert.ToDouble(dtOrderRecords.Rows[i][42]);
                            order.OriginNote = Convert.ToString(dtOrderRecords.Rows[i][43]);
                            //Student objStudent = new Student();
                            //objStudent.RollNo = Convert.ToInt32(dtStudentRecords.Rows[i][0]);
                            //objStudent.EnrollmentNo = Convert.ToString(dtStudentRecords.Rows[i][1]);
                            //objStudent.Name = Convert.ToString(dtStudentRecords.Rows[i][2]);
                            //objStudent.Branch = Convert.ToString(dtStudentRecords.Rows[i][3]);
                            //objStudent.University = Convert.ToString(dtStudentRecords.Rows[i][4]);
                            //objEntity.Students.Add(objStudent);
                            orders.Add(order);

                        }

                        //int output = objEntity.SaveChanges();
                        //if (output > 0)
                        //    message = "The Excel file has been successfully uploaded.";
                        //else
                        //    message = "Something Went Wrong!, The Excel file uploaded has fiald.";
                    }
                    else
                        message = "Selected file is empty.";
                }
                else
                    message = "Invalid File.";
            }
            else
                ResponseMessage = Request.CreateResponse(HttpStatusCode.BadRequest);

            return Ok(Mapper.Map<IEnumerable<USPrimeOrder>, IEnumerable<USPrimeOrderDto>>(orders));
        }

        [HttpPost]
        public IHttpActionResult DownloadPdf([FromUri]string operation, [FromBody]USPrimeOrderDto order)
        {
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == order.customerCode);

            order.customerName = customerInDb.Name;
            order.address_1 = customerInDb.FirstAddressLine;
            order.address_2 = customerInDb.SecondAddressLine;

            var templatePath = @"D:\Template\PRIME-DN-TEMPLATE.xlsx";
            var g = new FBAInvoiceHelper(templatePath);
            var path = g.GenerateSingleDN(order);

            if (operation == "EMAIL")
            {
                var destMail = customerInDb.EmailAddress;
                //var mailService = new MailServiceManager();
                //mailService.SendMail(destMail, null, order.cc, path, "USPRIME-DN");

                var mailService = new MailServiceManager("no-reply@usaprimeagency.com", "#lxX.Py#h,9s");
                mailService.SendMailFromUSPRIME(destMail, null, order.cc, path, $"USPRIME-DN-{order.hblNumber.Substring(6)}");

            }

            return Ok(path);
        }

        public string GenerateSingleDN(string templatePath, USPrimeOrderDto order)
        {
            var asposeWb = new Workbook(templatePath);
            var asposeWs = asposeWb.Worksheets[0];

            asposeWs.Cells[2, 11].PutValue(order.hblNumber);
            asposeWs.Cells[10, 3].PutValue(order.customerName);
            asposeWs.Cells[11, 3].PutValue(order.address_1);
            asposeWs.Cells[12, 3].PutValue(order.address_2);
            asposeWs.Cells[10, 11].PutValue(order.dnDate.ToString("yyyy-MM-dd"));
            asposeWs.Cells[13, 11].PutValue(order.by);

            asposeWs.Cells[15, 2].PutValue("HB/L# " + order.hblNumber.Substring(6));
            asposeWs.Cells[16, 2].PutValue("MB/L# " + order.mblNumber);

            asposeWs.Cells[18, 3].PutValue("Trucking Fee");
            asposeWs.Cells[18, 9].PutValue(1);
            asposeWs.Cells[18, 10].PutValue(order.truckingFee);

            asposeWs.Cells[19, 3].PutValue("Handling Fee");
            asposeWs.Cells[19, 9].PutValue(1);
            asposeWs.Cells[19, 10].PutValue(order.handlingFee);

            if (order.profitShare != 0)
            {
                asposeWs.Cells[20, 3].PutValue("Profit Share");
                asposeWs.Cells[20, 9].PutValue(1);
                asposeWs.Cells[20, 10].PutValue(order.profitShare);
            }

            asposeWs.Cells[38, 2].PutValue(order.OriginNote);

            var fullPath = @"D:\usprime\DN\DN-" + order.customerName + "-" + order.dnDate.ToString("yyyyMMdd") + ".pdf";

            asposeWb.Save(fullPath, SaveFormat.Xlsx);

            var wb = new Workbook(fullPath);

            fullPath = @"D:\usprime\DN\DN-" + order.customerName + "-" + order.dnDate.ToString("yyyyMMdd") + ".pdf";

            wb.Save(fullPath, SaveFormat.Pdf);

            return fullPath;
        }
    }

    public class USPrimeOrder
    {
        public string customerCode { get; set; }

        public string customerName { get; set; }

        public string address_1 { get; set; }

        public string address_2 { get; set; }

        public DateTime dnDate { get; set; }

        public string by { get; set; }

        public string hblNumber { get; set; }

        public string mblNumber { get; set; }

        public float totalCC { get; set; }

        public float truckingFee { get; set; }

        public float profitShare { get; set; }

        public float handlingFee { get; set; }

        public float balanceToOrigin { get; set; }

        public string OriginNote { get; set; }

        public string cc { get; set; }

        public USPrimeOrder()
        {
            dnDate = DateTime.Today;
        }
    }

    public class USPrimeOrderDto
    {
        public string customerCode { get; set; }

        public string customerName { get; set; }

        public string address_1 { get; set; }

        public string address_2 { get; set; }

        public DateTime dnDate { get; set; }

        public string by { get; set; }

        public string hblNumber { get; set; }

        public string mblNumber { get; set; }

        public float totalCC { get; set; }

        public float truckingFee { get; set; }

        public float profitShare { get; set; }

        public float handlingFee { get; set; }

        public float balanceToOrigin { get; set; }

        public string OriginNote { get; set; }

        public string cc { get; set; }

        public USPrimeOrderDto()
        {
            dnDate = DateTime.Today;
        }
    }
}
