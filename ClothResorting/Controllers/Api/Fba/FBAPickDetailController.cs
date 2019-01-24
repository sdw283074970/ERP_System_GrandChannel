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
                .Where(x => x.FBAShipOrder.Id == shipOrderId)
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
                var resultsInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAPallet.FBACartonLocations)
                    .Where(x => x.AvailablePlts != 0);

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
                    pickDetail.AssembleActualDetails(r.ActualGrossWeight, r.ActualCBM, r.FBAPallet.ActualQuantity);

                    pickDetail.Status = FBAStatus.Picking;
                    pickDetail.Size = r.PalletSize;
                    pickDetail.ActualPlts = r.AvailablePlts;
                    pickDetail.CtnsPerPlt = r.CtnsPerPlt;
                    pickDetail.Location = r.Location;

                    r.PickingPlts += r.AvailablePlts;
                    r.AvailablePlts = 0;
                    r.Status = FBAStatus.Picking;

                    pickDetail.HowToDeliver = r.HowToDeliver;
                    pickDetail.FBAPalletLocation = r;
                    pickDetail.OrderType = FBAOrderType.Standard;
                    pickDetail.HowToDeliver = r.HowToDeliver;

                    pickDetail.FBAShipOrder = shipOrderInDb;

                    pickDetailList.Add(pickDetail);

                    foreach(var cartonLocation in r.FBAPallet.FBACartonLocations)
                    {
                        cartonLocation.PickingCtns += cartonLocation.AvailableCtns;
                        cartonLocation.AvailableCtns = 0;
                    }
                }
            }
            else
            {
                var resultsInDb = _context.FBACartonLocations.Where(x => x.AvailableCtns != 0);

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

                    r.PickingCtns += r.AvailableCtns;
                    r.AvailableCtns = 0;
                    r.Status = FBAStatus.Picking;

                    pickDetail.FBACartonLocation = r;
                    pickDetail.OrderType = FBAOrderType.ECommerce;
                    pickDetail.HowToDeliver = r.HowToDeliver;

                    pickDetail.FBAShipOrder = shipOrderInDb;

                    pickDetailList.Add(pickDetail);
                }
            }

            shipOrderInDb.Status = FBAStatus.Picking;

            _context.FBAPickDetails.AddRange(pickDetailList);
            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + pickDetailList.Count, Mapper.Map<IEnumerable<FBAPickDetail>, IEnumerable<FBAPickDetailsDto>>(pickDetailList));
        }

        // DELETE /api/fba/fbapickdetail/?pickDetailId={pickDetailId}
        [HttpDelete]
        public void PutBackPickDetail([FromUri]int pickDetailId)
        {
            RemovePickDetail(_context, pickDetailId);
            _context.SaveChanges();
        }

        public void RemovePickDetail(ApplicationDbContext context, int pickDetailId)
        {
            var pickDetailInDb = context.FBAPickDetails
                .Include(x => x.FBACartonLocation)
                .Include(x => x.FBAPalletLocation)
                .SingleOrDefault(x => x.Id == pickDetailId);

            //如果palletLocation不为空，则说明是standard类型运单内容，否则是ecommerce运单内容
            if (pickDetailInDb.FBAPalletLocation != null)
            {
                pickDetailInDb.FBAPalletLocation.AvailablePlts += pickDetailInDb.ActualPlts;
                pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.ActualPlts;
                pickDetailInDb.FBAPalletLocation.Status = FBAStatus.InStock;
            }
            else
            {
                pickDetailInDb.FBACartonLocation.AvailableCtns += pickDetailInDb.ActualQuantity;
                pickDetailInDb.FBACartonLocation.PickingCtns -= pickDetailInDb.ActualQuantity;
                pickDetailInDb.FBACartonLocation.Status = FBAStatus.InStock;
            }

            context.FBAPickDetails.Remove(pickDetailInDb);
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
