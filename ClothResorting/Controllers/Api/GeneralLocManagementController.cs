using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class GeneralLocManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public GeneralLocManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/generallocmanagement/
        [HttpGet]
        public IHttpActionResult DownloadTemplate()
        {
            var downloader = new Downloader();

            downloader.DownloadFromServer("ReplenishmentLocationReport-Template.xlsx", @"D:\Template\");

            return Ok();
        }

        // GET /api/generallocmanagement/?preId={preId}
        [HttpGet]
        public IHttpActionResult GetAllGeneralLocationSummay([FromUri]int preId)
        {
            return Ok(_context.GeneralLocationSummaries
                .Where(x => x.PreReceiveOrder.Id == preId)
                .OrderByDescending(x => x.Id)
                .Select(Mapper.Map<GeneralLocationSummary, GeneralLocationSummaryDto>));
        }

        // POST /api/generallocmanagement/?preId={preId}&vendor={vendor}&inboundDate={inboundDate}&preId={preId}
        [HttpPost]
        public void CreateNewGeneralLocationSummaryAndDetail([FromUri]string vendor, [FromUri]string inboundDate, [FromUri]int preId)
        {
            var fileSavePath = "";
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.UploadReplenishimentLocationDetail(preId, vendor, inboundDate, fileSavePath.Split('\\').Last().Split('.').First());
        }

        // DELETE /api/generallocmanagement/{id}
        [HttpDelete]
        public void DeleteLocManagement([FromUri]int id)
        {
            var locationDetailsInDb = _context.ReplenishmentLocationDetails
                .Include(x => x.GeneralLocationSummary)
                .Include(x => x.PurchaseOrderInventory)
                .Include(x => x.SpeciesInventory)
                .Include(x => x.PickDetails)
                .Where(x => x.GeneralLocationSummary.Id == id);

            foreach(var location in locationDetailsInDb)
            {
                location.PurchaseOrderInventory.AvailablePcs -= location.AvailablePcs;
                location.SpeciesInventory.AvailablePcs -= location.AvailablePcs;
                location.SpeciesInventory.OrgPcs -= location.AvailablePcs;
                location.SpeciesInventory.AdjPcs -= location.AvailablePcs;

                //生成删除历史，放在调整记录中
                _context.AdjustmentRecords.Add(new AdjustmentRecord {
                    SpeciesInventory = location.SpeciesInventory,
                    AdjustDate = DateTime.Now,
                    Adjustment = (-location.AvailablePcs).ToString(),
                    Balance = (location.SpeciesInventory.AvailablePcs - location.AvailablePcs).ToString(),
                    PurchaseOrder = location.PurchaseOrder,
                    Style = location.Style,
                    Size = location.Size,
                    Color = location.Color,
                    Memo = Status.Delete
                });
            }

            _context.ReplenishmentLocationDetails.RemoveRange(locationDetailsInDb);
            _context.GeneralLocationSummaries.Remove(_context.GeneralLocationSummaries.Find(id));
            try
            {
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                var pickDetailId = locationDetailsInDb.First().PickDetails.First().Id;
                var shipOrderId = _context.PickDetails.Include(x => x.ShipOrder).SingleOrDefault(x => x.Id == pickDetailId).Id;
                throw new Exception("Please cancel all related ship order before deleting. Ship Order Id:" + shipOrderId);
            }
        }
    }
}
