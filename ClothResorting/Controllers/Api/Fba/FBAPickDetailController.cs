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
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

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
                    pickDetailList.Add(GetFBAPickDetailFromPalletLocation(r, shipOrderInDb, pickDetailCartonList));
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
                    pickDetailList.Add(GetFBAPickDetailFromCartonLocation(r, shipOrderInDb));
                }
            }

            shipOrderInDb.Status = FBAStatus.Picking;

            _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
            _context.FBAPickDetails.AddRange(pickDetailList);
            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + pickDetailList.Count, Mapper.Map<IEnumerable<FBAPickDetail>, IEnumerable<FBAPickDetailsDto>>(pickDetailList));
        }

        // POST /api/fba/fbapickdetail/?shipOrderId={shipOrderId}&inventoryId={inventoryId}&inventoryType={inventoryType}&operation={operation}
        [HttpPost]
        public IHttpActionResult CreateSinglePickDetails([FromUri]int shipOrderId, [FromUri]int inventoryId, [FromUri]string inventoryType, [FromUri]string operation)
        {
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            if (operation == "AllPick")
            {
                var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

                if (inventoryType == FBAInventoryType.Pallet)
                {
                    var palletLocationInDb = _context.FBAPalletLocations
                        .Include(x => x.FBAPallet.FBACartonLocations)
                        .SingleOrDefault(x => x.Id == inventoryId);

                    _context.FBAPickDetails.Add(GetFBAPickDetailFromPalletLocation(palletLocationInDb, shipOrderInDb, pickDetailCartonList));
                    _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
                }
                else
                {
                    var cartonLocationInDb = _context.FBACartonLocations
                        .SingleOrDefault(x => x.Id == inventoryId);

                    _context.FBAPickDetails.Add(GetFBAPickDetailFromCartonLocation(cartonLocationInDb, shipOrderInDb));
                }
            }

            _context.SaveChanges();

            return Created(Request.RequestUri + "/CreatedSuccess", "");
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
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Include(x => x.FBAPickDetailCartons)
                .SingleOrDefault(x => x.Id == pickDetailId);

            //如果palletLocation不为空，则说明是standard类型运单内容，否则是ecommerce运单内容
            if (pickDetailInDb.FBAPalletLocation != null)
            {
                pickDetailInDb.FBAPalletLocation.AvailablePlts += pickDetailInDb.ActualPlts;
                pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.ActualPlts;
                pickDetailInDb.FBAPalletLocation.Status = FBAStatus.InStock;

                var pickDetailCartonsInDb = _context.FBAPickDetailCartons
                    .Include(x => x.FBACartonLocation)
                    .Include(x => x.FBAPickDetail)
                    .Where(x => x.FBAPickDetail.Id == pickDetailInDb.Id);

                foreach (var c in pickDetailCartonsInDb)
                {
                    c.FBACartonLocation.AvailableCtns += c.PickCtns;
                    c.FBACartonLocation.PickingCtns -= c.PickCtns;
                }

                _context.FBAPickDetailCartons.RemoveRange(pickDetailCartonsInDb);
            }
            else
            {
                pickDetailInDb.FBACartonLocation.AvailableCtns += pickDetailInDb.ActualQuantity;
                pickDetailInDb.FBACartonLocation.PickingCtns -= pickDetailInDb.ActualQuantity;
                pickDetailInDb.FBACartonLocation.Status = FBAStatus.InStock;
            }

            context.FBAPickDetails.Remove(pickDetailInDb);
        }

        public FBAPickDetail GetFBAPickDetailFromPalletLocation(FBAPalletLocation fbaPalletLocationInDb, FBAShipOrder shipOrderInDb, IList<FBAPickDetailCarton> pickDetailCartonList)
        {
            var pickDetail = new FBAPickDetail();

            pickDetail.AssembleUniqueIndex(fbaPalletLocationInDb.Container, fbaPalletLocationInDb.GrandNumber);
            pickDetail.AssembleFirstStringPart(fbaPalletLocationInDb.ShipmentId, fbaPalletLocationInDb.AmzRefId, fbaPalletLocationInDb.WarehouseCode);
            pickDetail.AssembleActualDetails(fbaPalletLocationInDb.ActualGrossWeight, fbaPalletLocationInDb.ActualCBM, fbaPalletLocationInDb.FBAPallet.ActualQuantity);

            pickDetail.Status = FBAStatus.Picking;
            pickDetail.Size = fbaPalletLocationInDb.PalletSize;
            pickDetail.ActualPlts = fbaPalletLocationInDb.AvailablePlts;
            pickDetail.CtnsPerPlt = fbaPalletLocationInDb.CtnsPerPlt;
            pickDetail.Location = fbaPalletLocationInDb.Location;

            fbaPalletLocationInDb.PickingPlts += fbaPalletLocationInDb.AvailablePlts;
            fbaPalletLocationInDb.AvailablePlts = 0;
            fbaPalletLocationInDb.Status = FBAStatus.Picking;

            pickDetail.HowToDeliver = fbaPalletLocationInDb.HowToDeliver;
            pickDetail.FBAPalletLocation = fbaPalletLocationInDb;
            pickDetail.OrderType = FBAOrderType.Standard;
            pickDetail.HowToDeliver = fbaPalletLocationInDb.HowToDeliver;

            pickDetail.FBAShipOrder = shipOrderInDb;

            foreach (var cartonLocation in fbaPalletLocationInDb.FBAPallet.FBACartonLocations)
            {
                cartonLocation.PickingCtns += cartonLocation.AvailableCtns;

                var pickDetailCarton = new FBAPickDetailCarton
                {
                    PickCtns = cartonLocation.AvailableCtns,
                    FBAPickDetail = pickDetail,
                    FBACartonLocation = cartonLocation
                };

                cartonLocation.AvailableCtns = 0;

                pickDetailCartonList.Add(pickDetailCarton);
            }

            return pickDetail;
        }

        public FBAPickDetail GetFBAPickDetailFromCartonLocation(FBACartonLocation fbaCartonLocationInDb, FBAShipOrder shipOrderInDb)
        {
            var pickDetail = new FBAPickDetail();

            pickDetail.AssembleUniqueIndex(fbaCartonLocationInDb.Container, fbaCartonLocationInDb.GrandNumber);
            pickDetail.AssembleFirstStringPart(fbaCartonLocationInDb.ShipmentId, fbaCartonLocationInDb.AmzRefId, fbaCartonLocationInDb.WarehouseCode);
            pickDetail.AssembleActualDetails(fbaCartonLocationInDb.ActualGrossWeight, fbaCartonLocationInDb.ActualCBM, fbaCartonLocationInDb.AvailableCtns);

            pickDetail.Status = FBAStatus.Picking;
            pickDetail.Size = string.Empty;
            pickDetail.CtnsPerPlt = 0;
            pickDetail.Location = fbaCartonLocationInDb.Location;

            fbaCartonLocationInDb.PickingCtns += fbaCartonLocationInDb.AvailableCtns;
            fbaCartonLocationInDb.AvailableCtns = 0;
            fbaCartonLocationInDb.Status = FBAStatus.Picking;

            pickDetail.FBACartonLocation = fbaCartonLocationInDb;
            pickDetail.OrderType = FBAOrderType.ECommerce;
            pickDetail.HowToDeliver = fbaCartonLocationInDb.HowToDeliver;

            pickDetail.FBAShipOrder = shipOrderInDb;

            return pickDetail;
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
