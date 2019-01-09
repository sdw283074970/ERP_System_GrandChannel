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

        // GET /api/warehouseoperation/{id}
        [HttpGet]
        public IHttpActionResult GetUnlaiedObjects([FromUri]int id)
        {
            return Ok(_context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == id
                    && x.ComsumedQuantity != x.ActualQuantity)
                .Select(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>));
        }

        // POST /api/warehouseoperation/?masterOrderId={masterOrderId}&pltQuantity={pltQuantity}&pltSize={pltSize}
        [HttpPost]
        public void CreatePallet([FromUri]int masterOrderId, [FromUri]int pltQUantity, [FromUri]string pltSize, [FromBody]IEnumerable<PalletInfoDto> objArray)
        {
            var cartonLocationList = new List<FBACartonLocation>();
            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);
            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            foreach(var obj in objArray)
            {
                var orderDetailInDb = orderDetailsInDb.SingleOrDefault(x => x.Id == obj.Id);

                orderDetailInDb.ComsumedQuantity += obj.CtnsPerPlt * pltQUantity;

                if (orderDetailInDb.ComsumedQuantity > orderDetailInDb.ActualQuantity)
                {
                    throw new Exception("Not enough quantity for comsuming. Check Id:" + obj.Id);
                }

                var cartonLocation = new FBACartonLocation();
                var locationActualQuantity = obj.CtnsPerPlt;
                var locationActualGrossWeight = (float)Math.Round((orderDetailInDb.ActualGrossWeight / orderDetailInDb.ActualQuantity), 2);
                var locationActualCBM = (float)Math.Round((orderDetailInDb.ActualCBM / orderDetailInDb.ActualQuantity), 2);


                cartonLocation.AssembleFirstStringPart(orderDetailInDb.ShipmentId, orderDetailInDb.AmzRefId, orderDetailInDb.WarehouseCode);
                cartonLocation.AssembleActualDetails(locationActualGrossWeight, locationActualCBM, locationActualQuantity);

                cartonLocation.Container = orderDetailInDb.Container;
                //cartonLocation.AvaliableCtns = cartonLocation.ActualQuantity;
                cartonLocation.Location = "Pallet";
                cartonLocation.HowToDeliver = orderDetailInDb.HowToDeliver;
                cartonLocation.FBAOrderDetail = orderDetailInDb;

                cartonLocationList.Add(cartonLocation);
            }

            //建立FBAPallet对象
            var pallet = new FBAPallet();
            var firstId = objArray.First().Id;
            var firstOrderDetail = orderDetailsInDb.SingleOrDefault(x => x.Id == firstId);

            pallet.AssembleFirstStringPart(firstOrderDetail.ShipmentId, firstOrderDetail.AmzRefId, firstOrderDetail.WarehouseCode);
            pallet.AssembleActualDetails(cartonLocationList.Sum(x => x.ActualGrossWeight), cartonLocationList.Sum(x => x.ActualCBM), cartonLocationList.Sum(x => x.ActualQuantity));
            pallet.OriginalPallets = pltQUantity;
            pallet.AvailablePalltes = pltQUantity;
            pallet.PltSize = pltSize;
            pallet.HowToDeliver = firstOrderDetail.HowToDeliver;
            pallet.Container = firstOrderDetail.Container;

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
