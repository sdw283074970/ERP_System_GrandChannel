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

namespace ClothResorting.Controllers.Api.Fba
{
    public class WarehouseOperationController : ApiController
    {
        private ApplicationDbContext _context;

        public WarehouseOperationController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/warehouseoperation/?grandNumber={grandNumber}
        [HttpGet]
        public IHttpActionResult GetUnlaiedObjects([FromUri]string grandNumber)
        {
            return Ok(_context.FBAOrderDetails
                .Where(x => x.GrandNumber == grandNumber
                    && x.ComsumedQuantity != x.ActualQuantity)
                .Select(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>));
        }

        // POST /api/warehouseoperation/?masterOrderId={masterOrderId}&pltQuantity={pltQuantity}&pltSize={pltSize}
        [HttpPost]
        public void CreatePallet([FromUri]string grandNumber, [FromUri]int pltQUantity, [FromUri]string pltSize, [FromUri]bool doesAppliedLabel, [FromUri]bool hasSortingMarking, [FromUri]bool isOverSizeOrOverwidth, [FromBody]IEnumerable<PalletInfoDto> objArray)
        {
            var cartonLocationList = new List<FBACartonLocation>();
            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber);
            var orderDetailsInDb = _context.FBAOrderDetails
                .Where(x => x.GrandNumber == grandNumber);

            foreach(var obj in objArray)
            {
                var orderDetailInDb = orderDetailsInDb.SingleOrDefault(x => x.Id == obj.Id);

                orderDetailInDb.ComsumedQuantity += obj.CtnsPerPlt * pltQUantity;

                if (orderDetailInDb.ComsumedQuantity > orderDetailInDb.ActualQuantity)
                {
                    throw new Exception("Not enough quantity for comsuming. Check Id:" + obj.Id);
                }

                var cartonLocation = new FBACartonLocation();
                var ctnsPerPlt = obj.CtnsPerPlt;
                var grossWeightPerCtn = (float)Math.Round((orderDetailInDb.ActualGrossWeight / orderDetailInDb.ActualQuantity), 2);
                var cbmPerCtn = (float)Math.Round((orderDetailInDb.ActualCBM / orderDetailInDb.ActualQuantity), 2);

                cartonLocation.AssembleFirstStringPart(orderDetailInDb.ShipmentId, orderDetailInDb.AmzRefId, orderDetailInDb.WarehouseCode);
                cartonLocation.AssemblePltInfo(grossWeightPerCtn, cbmPerCtn, ctnsPerPlt);

                cartonLocation.Container = orderDetailInDb.Container;
                //cartonLocation.AvaliableCtns = cartonLocation.ActualQuantity;
                cartonLocation.Location = "Pallet";
                cartonLocation.HowToDeliver = orderDetailInDb.HowToDeliver;
                cartonLocation.GrandNumber = grandNumber;
                cartonLocation.FBAOrderDetail = orderDetailInDb;

                cartonLocationList.Add(cartonLocation);
            }

            //建立FBAPallet对象
            var pallet = new FBAPallet();
            var firstId = objArray.First().Id;
            var firstOrderDetail = orderDetailsInDb.SingleOrDefault(x => x.Id == firstId);

            pallet.AssembleFirstStringPart(firstOrderDetail.ShipmentId, firstOrderDetail.AmzRefId, firstOrderDetail.WarehouseCode);
            pallet.AssembleActualDetails(cartonLocationList.Sum(x => x.GrossWeightPerCtn * x.CtnsPerPlt * pltQUantity), cartonLocationList.Sum(x => x.CBMPerCtn * x.CtnsPerPlt * pltQUantity), cartonLocationList.Sum(x => x.CtnsPerPlt * pltQUantity));
            pallet.AssembleBoolValue(doesAppliedLabel, hasSortingMarking, isOverSizeOrOverwidth);

            pallet.Container = firstOrderDetail.Container;
            pallet.HowToDeliver = firstOrderDetail.HowToDeliver;
            pallet.PltSize = pltSize;
            pallet.GrandNumber = grandNumber;
            pallet.ActualPallets = pltQUantity;

            _context.FBAPallets.Add(pallet);

            foreach(var cartonLocation in cartonLocationList)
            {
                cartonLocation.FBAPallet = pallet;
            }

            _context.FBACartonLocations.AddRange(cartonLocationList);
            _context.SaveChanges();
        }
    }

    public class PalletInfoDto
    {
        public int Id { get; set; }

        public int CtnsPerPlt { get; set; }
    }
}
