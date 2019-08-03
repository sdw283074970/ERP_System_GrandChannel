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
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInventoryController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAInventoryController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbainventory/?palletLocationId={palletLocationId}
        [HttpGet]
        public IHttpActionResult GetCartonsDetailInPalletLocation([FromUri]int palletLocationId)
        {
            return Ok(GetCartonLocationDto(palletLocationId));
        }

        // GET /api/fba/fbainventory/?pickDetailId={pickDetailId}
        [HttpGet]
        public IHttpActionResult GetCartonsDetailInPickDetail([FromUri]int pickDetailId)
        {
            var palletLocationId = _context.FBAPickDetails
                .Include(x => x.FBAPalletLocation)
                .SingleOrDefault(x => x.Id == pickDetailId)
                .FBAPalletLocation
                .Id;

            return Ok(GetCartonLocationDto(palletLocationId));
        }

        // GET /api/fba/fbainventory/?palletId={palletId}
        [HttpGet]
        public IHttpActionResult GetCartonsDetailInPallet([FromUri]int palletId)
        {
            return Ok(Mapper.Map<IEnumerable<FBACartonLocation>, IEnumerable<FBACartonLocationDto>>(_context.FBAPallets
                .Include(x => x.FBACartonLocations)
                .SingleOrDefault(x => x.Id == palletId)
                .FBACartonLocations));
        }

        // GET /api/fba/fbaiventory/?grandNumber={grandNumber}&inventoryType={inventoryType}
        [HttpGet]
        public IHttpActionResult GetFBAInventory([FromUri]string grandNumber, [FromUri]string inventoryType)
        {
            if (inventoryType == FBAInventoryType.Pallet)
            {
                return Ok(_context.FBAPalletLocations
                    .Where(x => x.GrandNumber == grandNumber)
                    .Select(Mapper.Map<FBAPalletLocation, FBAPalletLocationDto>));
            }
            else
            {
                return Ok(_context.FBACartonLocations
                    .Include(x => x.FBAPallet)
                    .Where(x => x.GrandNumber == grandNumber)
                    .Select(Mapper.Map<FBACartonLocation, FBACartonLocationDto>));
            }
        }

        // GET /api/fba/fbaiventory/?masterOrderId={masterOrderId}&inventoryType={inventoryType}
        [HttpGet]
        public IHttpActionResult GetFBAInventoryByMasterOrderId([FromUri]int masterOrderId, [FromUri]string inventoryType)
        {
            if (inventoryType == FBAInventoryType.Pallet)
            {
                return Ok(_context.FBAPalletLocations
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Id == masterOrderId)
                    .Select(Mapper.Map<FBAPalletLocation, FBAPalletLocationDto>));
            }
            else
            {
                return Ok(_context.FBACartonLocations
                    .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                    .Where(x => x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId)
                    .Select(Mapper.Map<FBACartonLocation, FBACartonLocationDto>));
            }
        }

        // GET /api/fba/fbaiventory/?container={container}&sku={sku}&amzRef={amzRef}&warehouseCode={warehouseCode}&inventoryType={inventoryType} 搜索获取可拣货列表
        [HttpGet]
        public IHttpActionResult GetFBAInventoryViaContainer([FromUri]string container, [FromUri]string sku, [FromUri]string amzRef, [FromUri]string warehouseCode, [FromUri]string inventoryType)
        {
            if (inventoryType == FBAInventoryType.Pallet)
            {
                var palletInventoryInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAMasterOrder)
                    .Include(x => x.FBAPallet.FBACartonLocations)
                    .Where(x => x.AvailablePlts != 0);

                if (container != null)
                {
                    palletInventoryInDb = palletInventoryInDb.Where(x => x.Container.Contains(container));
                }
                
                if (sku != null)
                {
                    palletInventoryInDb = palletInventoryInDb.Where(x => x.ShipmentId.Contains(sku));
                }

                if (amzRef != null)
                {
                    palletInventoryInDb = palletInventoryInDb.Where(x => x.AmzRefId.Contains(amzRef));
                }

                if (warehouseCode != null)
                {
                    palletInventoryInDb = palletInventoryInDb.Where(x => x.WarehouseCode.Contains(warehouseCode));
                }

                var palletInventoryDto = new List<FBAPalletLocationDto>();

                foreach(var p in palletInventoryInDb)
                {
                    var dto = Mapper.Map<FBAPalletLocation, FBAPalletLocationDto>(p);
                    dto.InboundDate = p.FBAMasterOrder.InboundDate;
                    dto.OriginalTotalCtns = p.FBAPallet.FBACartonLocations.Sum(x => x.ActualQuantity);
                    dto.CurrentAvailableCtns = p.FBAPallet.FBACartonLocations.Sum(x => x.AvailableCtns);

                    palletInventoryDto.Add(dto);
                }

                return Ok(palletInventoryDto);
            }
            else
            {
                var cartonInventoryInDb = _context.FBACartonLocations
                    .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                    .Where(x => x.AvailableCtns != 0);

                if (container != null)
                {
                    cartonInventoryInDb = cartonInventoryInDb.Where(x => x.Container.Contains(container));
                }

                if (sku != null)
                {
                    cartonInventoryInDb = cartonInventoryInDb.Where(x => x.ShipmentId.Contains(sku));
                }

                if (amzRef != null)
                {
                    cartonInventoryInDb = cartonInventoryInDb.Where(x => x.AmzRefId.Contains(amzRef));
                }

                if (warehouseCode != null)
                {
                    cartonInventoryInDb = cartonInventoryInDb.Where(x => x.WarehouseCode.Contains(warehouseCode));
                }

                var cartonInventoryDto = new List<FBACartonLocationDto>();

                foreach(var c in cartonInventoryInDb)
                {
                    var dto = Mapper.Map<FBACartonLocation, FBACartonLocationDto>(c);
                    dto.InboundDate = c.FBAOrderDetail.FBAMasterOrder.InboundDate;
                    cartonInventoryDto.Add(dto);
                }

                return Ok(cartonInventoryDto);
            }
        }

        // GET /api/FBAinventory/?locationId={locationId}}&locationType={locationType}
        [HttpGet]
        public IHttpActionResult GetInventoryOutboundHistory([FromUri]int locationId, [FromUri]string locationType)
        {
            if (locationType == FBALocationType.Pallet)
            {
                var pickDetailsList = _context.FBAPickDetails
                    .Include(x => x.FBAShipOrder)
                    .Include(x => x.FBAPalletLocation)
                    .Where(x => x.FBAPalletLocation.Id == locationId)
                    .Select(x => new {
                        Id = x.Id,
                        Status = x.FBAShipOrder.Status,
                        ShipOrderNumber = x.FBAShipOrder.ShipOrderNumber,
                        Container = x.Container,
                        OrderType = x.OrderType,
                        PlaceTime = x.FBAShipOrder.PlaceTime,
                        CustomerCode= x.FBAShipOrder.CustomerCode,
                        ShipmentId = x.ShipmentId,
                        AmzRefId = x.AmzRefId,
                        WarehouseCode = x.WarehouseCode,
                        GrossWeight = x.ActualGrossWeight,
                        CBM = x.ActualCBM,
                        Quantity = x.PltsFromInventory,
                        Location = x.Location,
                        ShipOrderId = x.FBAShipOrder.Id
                    })
                    .ToList();

                return Ok(pickDetailsList);
            }
            else if (locationType == FBALocationType.Carton)
            {
                var cartonLocationInDb = _context.FBACartonLocations
                    .SingleOrDefault(x => x.Id == locationId);

                var historyList = new List<FBAOutboundHistory>();

                if (cartonLocationInDb.Location == "Pallet")
                {
                    var pickDetailCartonsInDb = _context.FBAPickDetailCartons
                        .Include(x => x.FBAPickDetail.FBAShipOrder)
                        .Include(x => x.FBACartonLocation)
                        .Where(x => x.FBACartonLocation.Id == locationId);

                    foreach(var p in pickDetailCartonsInDb)
                    {
                        var history = new FBAOutboundHistory
                        {
                            Id = p.FBAPickDetail.Id,
                            PlaceTime = p.FBAPickDetail.FBAShipOrder.PlaceTime,
                            ShipOrderNumber = p.FBAPickDetail.FBAShipOrder.ShipOrderNumber,
                            OrderType = p.FBAPickDetail.OrderType,
                            Status = p.FBAPickDetail.FBAShipOrder.Status,
                            Container = p.FBAPickDetail.Container,
                            CustomerCode = p.FBAPickDetail.FBAShipOrder.CustomerCode,
                            ShipmentId = p.FBACartonLocation.ShipmentId,
                            AmzRefId = p.FBACartonLocation.AmzRefId,
                            WarehouseCode = p.FBACartonLocation.WarehouseCode,
                            GrossWeight = p.FBACartonLocation.GrossWeightPerCtn * p.PickCtns,
                            CBM = p.FBACartonLocation.CBMPerCtn * p.PickCtns,
                            Quantity = p.PickCtns,
                            Location = p.FBACartonLocation.Location,
                            ShipOrderId = p.FBAPickDetail.FBAShipOrder.Id
                        };

                        historyList.Add(history);
                    }
                }
                else
                {
                    var pickDetailInDb = _context.FBAPickDetails
                        .Include(x => x.FBAShipOrder)
                        .Include(x => x.FBACartonLocation)
                        .Where(x => x.FBACartonLocation.Id == locationId);

                    foreach(var p in pickDetailInDb)
                    {
                        var history = new FBAOutboundHistory
                        {
                            Id = p.Id,
                            ShipOrderNumber = p.FBAShipOrder.ShipOrderNumber,
                            OrderType = p.OrderType,
                            PlaceTime = p.FBAShipOrder.PlaceTime,
                            Container = p.Container,
                            CustomerCode = p.FBAShipOrder.CustomerCode,
                            ShipmentId = p.ShipmentId,
                            Status = p.FBAShipOrder.Status,
                            AmzRefId = p.AmzRefId,
                            WarehouseCode = p.WarehouseCode,
                            GrossWeight = p.FBACartonLocation.GrossWeightPerCtn * p.ActualQuantity,
                            CBM = p.FBACartonLocation.CBMPerCtn * p.ActualQuantity,
                            Quantity = p.ActualQuantity,
                            Location = p.FBACartonLocation.Location,
                            ShipOrderId = p.FBAShipOrder.Id
                        };

                        historyList.Add(history);
                    }
                }

                return Ok(historyList);
            }

            return Ok();
        }

        // PUT /api/fbainventory/?locationId={locationId}&locationValue={locationValue}&locationType={locationType}
        [HttpPut]
        public void UpdateLocation([FromUri]int locationId, [FromUri]string locationValue, [FromUri]string locationType)
        {
            if (locationType == FBALocationType.Pallet)
            {
                var locationInDb = _context.FBAPalletLocations.Find(locationId);

                locationInDb.Location = locationValue;
            }
            else if (locationType == FBALocationType.Carton)
            {
                var locationInDb = _context.FBACartonLocations.Find(locationId);
                locationInDb.Location = locationValue;
            }

            _context.SaveChanges();
        }

        // DELET /api/fba/fbainventory/?locationId={locationId}&locationType={locationType}
        [HttpDelete]
        public void RelocateLocation([FromUri]int locationId, [FromUri]string locationType)
        {
            if (locationType == FBALocationType.Pallet)
            {
                var locationInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAPallet)
                    .SingleOrDefault(x => x.Id == locationId);

                locationInDb.FBAPallet.ComsumedPallets -= locationInDb.AvailablePlts;
                locationInDb.ActualPlts -= locationInDb.AvailablePlts;
                locationInDb.AvailablePlts = 0;
                locationInDb.Status = FBAStatus.Relocated;

                if (locationInDb.ShippedPlts == 0 && locationInDb.PickingPlts == 0)
                {
                    _context.FBAPalletLocations.Remove(locationInDb);
                }
            }
            else
            {
                var locationInDb = _context.FBACartonLocations
                    .Include(x => x.FBAOrderDetail)
                    .SingleOrDefault(x => x.Id == locationId);

                locationInDb.FBAOrderDetail.ComsumedQuantity -= locationInDb.AvailableCtns;
                locationInDb.ActualQuantity -= locationInDb.AvailableCtns;
                locationInDb.AvailableCtns = 0;
                locationInDb.Status = FBAStatus.Relocated;

                if (locationInDb.ShippedCtns == 0 && locationInDb.PickingCtns == 0)
                {
                    _context.FBACartonLocations.Remove(locationInDb);
                }
            }

            try
            {
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw new Exception("Cannot relocate this one becasuse some of cartons haven been shipped.");
            }
        }

        private IEnumerable<FBACartonLocationDto> GetCartonLocationDto(int palletLocationId)
        {
            return Mapper.Map<IEnumerable<FBACartonLocation>, IEnumerable<FBACartonLocationDto>>(_context.FBAPalletLocations
               .Include(x => x.FBAPallet.FBACartonLocations)
               .SingleOrDefault(x => x.Id == palletLocationId)
               .FBAPallet
               .FBACartonLocations);
        }
    }

    public class FBAOutboundHistory
    {
        public int Id { get; set; }

        public string ShipOrderNumber { get; set; }

        public string OrderType { get; set; }

        public string Container { get; set; }

        public string CustomerCode { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public float GrossWeight { get; set; }

        public string Status { get; set; }

        public float CBM { get; set; }

        public DateTime PlaceTime { get; set; }

        public int Quantity { get; set; }

        public string Location { get; set; }

        public int ShipOrderId { get; set; }
    }
}
