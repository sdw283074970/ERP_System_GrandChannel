using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using System.Web;
using ClothResorting.Models.FBAModels.StaticModels;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAEFolderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FBAEFolderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/fba/fbaefolder/?reference={reference}&orderType={orderType}
        [HttpGet]
        public IHttpActionResult GetFilesFromReference([FromUri]string reference, [FromUri]string orderType)
        {
            if (orderType == FBAOrderType.MasterOrder)
            {
                var filesDto = _context.EFiles
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Container == reference)
                    .Select(Mapper.Map<EFile, EFileDto>);

                return Ok(filesDto);
            }
            else if (orderType == FBAOrderType.ShipOrder)
            {
                var filesDto = _context.EFiles
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                    .Select(Mapper.Map<EFile, EFileDto>);

                return Ok(filesDto);
            }

            return Ok();
        }

        // GET /api/fba/fbaefolder/?fileId={fileId}
        [HttpGet]
        public void DownloadFile([FromUri]int fileId)
        {
            var downloader = new Downloader();
            var fileInfoInDb = _context.EFiles.Find(fileId);

            downloader.DownloadByFullPath(fileInfoInDb.RootPath + fileInfoInDb.FileName);
        }

        // POST /api/fba/fbaefolder/?reference={reference}&orderType={orderType}&fileName={fileName}&version={version}
        [HttpPost]
        public IHttpActionResult UploadFiles([FromUri]string reference, [FromUri]string orderType, [FromUri]string fileName, [FromUri]string version)
        {
            var fileGetter = new FilesGetter();

            var path = fileGetter.GetAndSaveSingleFileFromHttpRequest(@"E:\eFolder\");

            var newFileRecord = new EFile();

            if (version == "V1")
            {
                //newFileRecord.CustomizedFileName = fileName + "-" + DateTime.Now.ToString("hhmmss");
                newFileRecord.CustomizedFileName = path.Split('\\').Last().Split('-')[1];
                newFileRecord.FileName = path.Split('\\').Last();
                newFileRecord.RootPath = @"E:\eFolder\";
                newFileRecord.UploadBy = _userName;
                newFileRecord.UploadDate = DateTime.Now;
                newFileRecord.Status = FBAStatus.Valid;
            }
            else
            {
                throw new Exception("The system does not support version:" + version + ".");
            }

            if (orderType == FBAOrderType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);
                newFileRecord.FBAMasterOrder = masterOrderInDb;
            }
            else if (orderType == FBAOrderType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);
                newFileRecord.FBAShipOrder = shipOrderInDb;
            }
            else
            {
                throw new Exception("The system does not support order type:" + orderType + ".");
            }

            _context.EFiles.Add(newFileRecord);
            _context.SaveChanges();

            return Ok(Mapper.Map<EFile, EFileDto>(newFileRecord));
        }

        // POST /api/fba/fbaefolder/?fileId={fileId}
        [HttpPost]
        public void SendFileToCustomer([FromUri]int fileId)
        {
            var fileInDb = _context.EFiles
                .Include(x => x.FBAMasterOrder.Customer)
                .Include(x => x.FBAShipOrder)
                .SingleOrDefault(x => x.Id == fileId);

            var toEmail = string.Empty;
            var subject = fileInDb.CustomizedFileName;

            if (fileInDb.FBAMasterOrder != null)
            {
                toEmail = fileInDb.FBAMasterOrder.Customer.EmailAddress;
                subject += "-" + fileInDb.FBAMasterOrder.Container;
            }
            else
            {
                toEmail = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == fileInDb.FBAShipOrder.CustomerCode).EmailAddress;
                subject += "-" + fileInDb.FBAShipOrder.ShipOrderNumber;
            }

            var mailService = new MailServiceManager();

            mailService.SendMail(toEmail, "operator@grandchannel.us", "operator@grandchannel.us", fileInDb.RootPath + fileInDb.FileName, subject);

            fileInDb.SendDate = DateTime.Now;

            _context.SaveChanges();
        }

        // POST /api/fba/fbaefolder/?fileId={fileId}
        [HttpPut]
        public void DiscardFile([FromUri]int fileId)
        {
            var fileInDb = _context.EFiles.Find(fileId);

            fileInDb.Status = FBAStatus.Invalid;
            fileInDb.DiscardBy = _userName;

            _context.SaveChanges();
        }
    }
}
