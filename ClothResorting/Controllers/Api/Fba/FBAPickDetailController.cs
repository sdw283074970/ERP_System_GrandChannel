using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.FBAModels.Interfaces;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAPickDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAPickDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbapickdetail/?shipOrderId={shipOrderId}&orderType={orderType}
        [HttpGet]
        public IHttpActionResult GetPickDetail([FromUri]int shipOrderId, [FromUri]string orderType)
        {
            return Ok(_context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId && x.OrderType == orderType)
                .Select(Mapper.Map<FBAPickDetail, FBAPickDetailsDto>));
        }

        // POST /api/fba/fbapickdetail/?shipOrderId=?shipOrderId={shipOrderId}&orderType={orderType}
        [HttpPost]
        public IHttpActionResult CreatePickDetails([FromUri]int shipOrderId, [FromUri]string orderType, [FromBody]PickOrderDto obj)
        {
            var pickDetailList = new List<FBAPickDetail>();
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

            if (orderType == FBAOrderType.Standard)
            {
                var resultsInDb = _context.FBAPalletLocations.Where(x => x.Status != FBAStatus.Shipped);

                if (obj.Container != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.Container == obj.Container);
                }

                if (obj.CustomerCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.GrandNumber.Contains(obj.CustomerCode));
                }

                if (obj.ShipmentId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.ShipmentId == obj.ShipmentId);
                }

                if (obj.AmzRefId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.AmzRefId == obj.AmzRefId);
                }

                if (obj.HowToDeliver != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.HowToDeliver == obj.HowToDeliver);
                }

                if (obj.WarehouseCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.WarehouseCode == obj.WarehouseCode);
                }

                foreach (var r in resultsInDb)
                {
                    var pickDetail = new FBAPickDetail();

                    pickDetail.AssembleUniqueIndex(obj.Container, r.GrandNumber);
                    pickDetail.AssembleFirstStringPart(r.ShipmentId, r.AmzRefId, r.WarehouseCode);
                    pickDetail.AssembleActualDetails(r.ActualGrossWeight, r.ActualCBM, r.ActualQuantity);

                    pickDetail.Status = FBAStatus.Picking;
                    pickDetail.Size = r.PalletSize;
                    pickDetail.ActualPlts = r.AvailablePlts;
                    pickDetail.CtnsPerPlt = r.CtnsPerPlt;
                    pickDetail.Location = r.Location;

                    r.PickingPlts = r.AvailablePlts;
                    r.AvailablePlts = 0;

                    pickDetail.FBAPalletLocation = r;
                    pickDetail.FBAShipOrder = shipOrderInDb;

                    pickDetailList.Add(pickDetail);
                }
            }
            else
            {
                var resultsInDb = _context.FBACartonLocations.Where(x => x.Status != FBAStatus.Shipped);

                if (obj.Container != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.Container == obj.Container);
                }

                if (obj.CustomerCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.GrandNumber.Contains(obj.CustomerCode));
                }

                if (obj.ShipmentId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.ShipmentId == obj.ShipmentId);
                }

                if (obj.AmzRefId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.AmzRefId == obj.AmzRefId);
                }

                if (obj.HowToDeliver != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.HowToDeliver == obj.HowToDeliver);
                }

                if (obj.WarehouseCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.WarehouseCode == obj.WarehouseCode);
                }

                foreach (var r in resultsInDb)
                {
                    var pickDetail = new FBAPickDetail();

                    pickDetail.AssembleUniqueIndex(obj.Container, r.GrandNumber);
                    pickDetail.AssembleFirstStringPart(r.ShipmentId, r.AmzRefId, r.WarehouseCode);
                    pickDetail.AssembleActualDetails(r.ActualGrossWeight, r.ActualCBM, r.AvailableCtns);

                    pickDetail.Status = FBAStatus.Picking;
                    pickDetail.Size = string.Empty;
                    pickDetail.CtnsPerPlt = 0;
                    pickDetail.Location = r.Location;

                    r.PickingCtns = r.AvailableCtns;
                    r.AvailableCtns = 0;

                    pickDetail.FBACartonLocation = r;
                    pickDetail.FBAShipOrder = shipOrderInDb;

                    pickDetailList.Add(pickDetail);
                }
            }

            shipOrderInDb.Status = FBAStatus.Picking;

            _context.FBAPickDetails.AddRange(pickDetailList);
            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + pickDetailList.Count, pickDetailList);
        }
    }

    public class PickOrderDto
    {
        public string Container { get; set; }

        public string CustomerCode { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string HowToDeliver { get; set; }

        public string WarehouseCode { get; set; }
    }
}
