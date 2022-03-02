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
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.USPrime
{
    public class SOAController : ApiController
    {
        private ApplicationDbContext _context;

        public SOAController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/soa/
        [HttpPost]
        public IHttpActionResult UploadAndReadXlsxFile()
        {
            var entries = new List<SOAEntry>();

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
                        DataTable soaRecords = dsexcelRecords.Tables[0];
                        for (int i = 1; i < soaRecords.Rows.Count; i++)
                        {
                            var entry = new SOAEntry();
                            entry.hblNumber = Convert.ToString(soaRecords.Rows[i][0]);
                            entry.mblNumber = Convert.ToString(soaRecords.Rows[i][1]);
                            entry.etd = Convert.ToString(soaRecords.Rows[i][2]);
                            entry.releasedDate = Convert.ToString(soaRecords.Rows[i][4]);
                            entry.balanceToOrigin = (float)Convert.ToDouble(soaRecords.Rows[i][3]);
                            entry.note = Convert.ToString(soaRecords.Rows[i][5]);
                            //Student objStudent = new Student();
                            //objStudent.RollNo = Convert.ToInt32(dtStudentRecords.Rows[i][0]);
                            //objStudent.EnrollmentNo = Convert.ToString(dtStudentRecords.Rows[i][1]);
                            //objStudent.Name = Convert.ToString(dtStudentRecords.Rows[i][2]);
                            //objStudent.Branch = Convert.ToString(dtStudentRecords.Rows[i][3]);
                            //objStudent.University = Convert.ToString(dtStudentRecords.Rows[i][4]);
                            //objEntity.Students.Add(objStudent);
                            entries.Add(entry);

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

            return Ok(Mapper.Map<IEnumerable<SOAEntry>, IEnumerable<SOAEntryDto>>(entries));
        }

        // POST /api/soa/?operation=DOWNLOAD
        [HttpPost]
        public IHttpActionResult DownloadPdf([FromUri]string operation, [FromBody]SOA soa)
        {
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == soa.customerCode);

            soa.customerName = customerInDb.Name;
            soa.address_1 = customerInDb.FirstAddressLine;
            soa.address_2 = customerInDb.SecondAddressLine;

            var templatePath = @"D:\Template\PRIME-SOA-TEMPLATE.xlsx";
            var g = new FBAInvoiceHelper(templatePath);
            var path = g.GenerateSOA(soa);

            if (operation == "EMAIL")
            {
                var destMail = customerInDb.EmailAddress;
                //var mailService = new MailServiceManager();
                //mailService.SendMail(destMail, null, order.cc, path, "USPRIME-DN");

                var mailService = new MailServiceManager("no-reply@usaprimeagency.com", "#lxX.Py#h,9s");
                mailService.SendMailFromUSPRIME(destMail, null, soa.cc, path, $"USPRIME-SOA-{soa.fromDate.ToString("yyyyMMdd")}-{soa.fromDate.ToString("yyyyMMdd")}");
            }

            return Ok(path);
        }
    }

    public class SOAEntry
    {
        public string hblNumber { get; set; }
        public string mblNumber { get; set; }
        public string etd { get; set; }
        public string releasedDate { get; set; }
        public float balanceToOrigin { get; set; }
        public string note { get; set; }
    }

    public class SOAEntryDto
    {
        public string hblNumber { get; set; }
        public string mblNumber { get; set; }
        public string etd { get; set; }
        public string releasedDate { get; set; }
        public float balanceToOrigin { get; set; }
        public string note { get; set; }
    }

    public class SOA
    {
        public string customerCode { get; set; }

        public DateTime fromDate { get; set; }

        public DateTime toDate { get; set; }

        public DateTime reportDate { get; set; }

        public string customerName { get; set; }

        public string cc { get; set; }

        public string by { get; set; }

        public string address_1 { get; set; }

        public string address_2 { get; set; }

        public IEnumerable<SOAEntryDto> entries { get; set; }
    }
}
