using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.FBAModels.StaticModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAAllocatingController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAAllocatingController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbaallocating/?grandNumber={grandNumber}
        [HttpGet]
        public IHttpActionResult GetAllocatablePallets([FromUri]string grandNumber)
        {
            return Ok(_context.FBAPallets.Where(x => x.GrandNumber == grandNumber
                && x.ActualPallets > x.ComsumedPallets).Select(Mapper.Map<FBAPallet, FBAPalletDto>));
        }

        // GET /api/fba/fbaallocating/?masterOrderId={masterOrderId}
        [HttpGet]
        public IHttpActionResult GetAllocatablePalletsByMasterOrderId([FromUri]int masterOrderId)
        {
            var dtos = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId
                && x.ActualPallets > x.ComsumedPallets).Select(Mapper.Map<FBAPallet, FBAPalletDto>);

            var cartonLocationList = new List<FBACartonLocationDto>();

            foreach(var d in dtos)
            {
                d.FBACartonLocations = Mapper.Map<IEnumerable<FBACartonLocation>, IEnumerable<FBACartonLocationDto>>(_context.FBAPallets
                .Include(x => x.FBACartonLocations)
                .SingleOrDefault(x => x.Id == d.Id)
                .FBACartonLocations);
            }

            return Ok(dtos);
        }

        // POST /api/fba/fbaallocating/?grandNumber={grandNumber}&inventoryType={inventoryType}
        [HttpPost]
        public void CreateLocationObjects([FromUri]string grandNumber, [FromUri]string inventoryType, [FromBody]IEnumerable<FBALocationDto> objArray)
        {
            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber);
            if (inventoryType == FBAInventoryType.Pallet)
            {
                var palletLocationList = new List<FBAPalletLocation>();
                var palletsInDb = _context.FBAPallets
                    .Include(x => x.FBACartonLocations)
                    .Where(x => x.GrandNumber == grandNumber
                    && x.ActualPallets - x.ComsumedPallets > 0);

                if (palletsInDb.Count() == 0)
                {
                    throw new Exception("No quantity for allocating.");
                }

                foreach (var obj in objArray)
                {
                    var palletInDb = palletsInDb
                        .Include(x => x.FBACartonLocations)
                        .SingleOrDefault(x => x.Id == obj.Id);

                    //如果这是一个rough packed pallets，那么默认分配所有货物
                    //if (palletInDb.FBACartonLocations.Sum(x => x.CtnsPerPlt) == 0)
                    //{
                    //    obj.Quantity = palletInDb.ActualPallets;
                    //}

                    //所有类型的pallets现在不允许分开入库
                    obj.Quantity = palletInDb.ActualPallets - palletInDb.ComsumedPallets;

                    palletInDb.ComsumedPallets += obj.Quantity;

                    if (palletInDb.ComsumedPallets > palletInDb.ActualPallets)
                    {
                        throw new Exception("Not enough quantity for comsuming. Check Id:" + obj.Id);
                    }

                    var palletLocation = new FBAPalletLocation();

                    palletLocation.Status = FBAStatus.InStock;
                    palletLocation.HowToDeliver = palletInDb.HowToDeliver;
                    //palletLocation.GrossWeightPerPlt = palletInDb.ActualGrossWeight / palletInDb.ActualPallets;
                    palletLocation.GrossWeightPerPlt = palletInDb.FBACartonLocations.Sum(x => x.GrossWeightPerCtn * x.ActualQuantity) / obj.Quantity;
                    //palletLocation.CBMPerPlt = palletInDb.ActualCBM / palletInDb.ActualPallets;
                    palletLocation.CBMPerPlt = palletInDb.FBACartonLocations.Sum(x => x.CBMPerCtn * x.ActualQuantity) / obj.Quantity;
                    palletLocation.CtnsPerPlt = palletInDb.FBACartonLocations.Sum(x => x.CtnsPerPlt) == 0 ? 0 : palletInDb.ActualQuantity / palletInDb.ActualPallets;
                    palletLocation.AvailablePlts = obj.Quantity;
                    palletLocation.Location = obj.Location;
                    palletLocation.PalletSize = palletInDb.PalletSize;

                    palletLocation.AssembleFirstStringPart(palletInDb.ShipmentId, palletInDb.AmzRefId, palletInDb.WarehouseCode);
                    //PalletLocation的Actualquantity指内含cartons的总数量
                    palletLocation.AssembleActualDetails(palletLocation.GrossWeightPerPlt * obj.Quantity, palletLocation.CBMPerPlt * obj.Quantity, palletInDb.FBACartonLocations.Sum(x => x.ActualQuantity));
                    palletLocation.ActualPlts = obj.Quantity;
                    palletLocation.AssembleUniqueIndex(palletInDb.Container, palletInDb.GrandNumber);

                    palletLocation.FBAMasterOrder = masterOrderInDb;
                    palletLocation.FBAPallet = palletInDb;

                    palletLocationList.Add(palletLocation);
                }
                _context.FBAPalletLocations.AddRange(palletLocationList);
            }
            else
            {
                var cartonLocationList = new List<FBACartonLocation>();
                var orderDetailsInDb = _context.FBAOrderDetails
                    .Where(x => x.GrandNumber == grandNumber
                        && x.ActualQuantity - x.ComsumedQuantity > 0);

                if (orderDetailsInDb.Count() == 0)
                {
                    throw new Exception("No quantity for allocating.");
                }

                foreach (var obj in objArray)
                {
                    var orderDetailInDb = orderDetailsInDb.SingleOrDefault(x => x.Id == obj.Id);
                    orderDetailInDb.ComsumedQuantity += obj.Quantity;

                    if (orderDetailInDb.ComsumedQuantity > orderDetailInDb.ActualQuantity)
                    {
                        throw new Exception("Not enough quantity for comsuming. Check Id:" + obj.Id);
                    }

                    if (orderDetailInDb.Container == "NULL" || orderDetailInDb.Container == "")
                    {
                        throw new Exception("Please assign container number first.");
                    }

                    var cartonLocation = new FBACartonLocation();

                    cartonLocation.Status = FBAStatus.InStock;
                    cartonLocation.HowToDeliver = orderDetailInDb.HowToDeliver;
                    cartonLocation.GrossWeightPerCtn = (float)Math.Round((orderDetailInDb.ActualGrossWeight / orderDetailInDb.ActualQuantity), 2);
                    cartonLocation.CBMPerCtn = (float)Math.Round((orderDetailInDb.ActualCBM / orderDetailInDb.ActualQuantity), 2);
                    cartonLocation.AvailableCtns = obj.Quantity;
                    cartonLocation.Location = obj.Location;

                    cartonLocation.AssembleFirstStringPart(orderDetailInDb.ShipmentId, orderDetailInDb.AmzRefId, orderDetailInDb.WarehouseCode);
                    cartonLocation.AssembleActualDetails(cartonLocation.GrossWeightPerCtn * obj.Quantity, cartonLocation.CBMPerCtn * obj.Quantity, obj.Quantity);
                    cartonLocation.AssembleUniqueIndex(orderDetailInDb.Container, orderDetailInDb.GrandNumber);

                    cartonLocation.FBAOrderDetail = orderDetailInDb;

                    cartonLocationList.Add(cartonLocation);
                }

                _context.FBACartonLocations.AddRange(cartonLocationList);
            }
            _context.SaveChanges();
        }

        // DELETE /api/fba/fbaallocating/?palletId={palletId}
        [HttpDelete]
        public void RemovePalletAndRelatedCartonLocation([FromUri]int palletId)
        {
            var palletInDb = _context.FBAPallets
                .Include(x => x.FBACartonLocations)
                .SingleOrDefault(x => x.Id == palletId);

            var container = palletInDb.Container;
            var cartonLocationIds = palletInDb.FBACartonLocations.Select(x => x.Id).ToList();

            var cartonLocationsInDb = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail)
                .Where(x => x.Container == container && x.Status == FBAStatus.InPallet);

            foreach(var id in cartonLocationIds)
            {
                var cartonLocationInDb = cartonLocationsInDb.SingleOrDefault(x => x.Id == id);
                if (cartonLocationInDb.CtnsPerPlt == 0)    //如果ccp为0，说明是rough打托
                {
                    cartonLocationInDb.FBAOrderDetail.ComsumedQuantity -= cartonLocationInDb.AvailableCtns;
                }
                else    //否则是精细打托
                {
                    cartonLocationInDb.FBAOrderDetail.ComsumedQuantity -= cartonLocationInDb.CtnsPerPlt * (palletInDb.ActualPallets - palletInDb.ComsumedPallets);

                }
                _context.FBACartonLocations.Remove(cartonLocationInDb);
            }

            _context.FBAPallets.Remove(palletInDb);
            _context.SaveChanges();
        }
    }

    public class FBALocationDto
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public string Location { get; set; }
    }
}
