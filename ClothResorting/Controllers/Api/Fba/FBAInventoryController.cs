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
                    //.Include(x => x.FBAPallet)
                    .Include(x => x.FBAOrderDetail)
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
                var dtos = _context.FBAPalletLocations
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Id == masterOrderId)
                    .Select(Mapper.Map<FBAPalletLocation, FBAPalletLocationDto>)
                    .ToList();

                foreach(var d in dtos)
                {
                    d.FBACartonLocations = GetCartonLocationDto(d.Id);
                }

                return Ok(dtos);
            }
            else
            {
                return Ok(_context.FBACartonLocations
                    .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                    .Where(x => x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId)
                    .Select(Mapper.Map<FBACartonLocation, FBACartonLocationDto>));
            }
        }

        // GET /api/fba/fbaiventory/?shipOrderId={shipOrderId}&container={container}&sku={sku}&amzRef={amzRef}&warehouseCode={warehouseCode}&inventoryType={inventoryType} 搜索获取可拣货列表
        [HttpGet]
        public IHttpActionResult GetFBAInventoryViaContainer([FromUri]int shipOrderId, [FromUri]string container, [FromUri]string sku, [FromUri]string amzRef, [FromUri]string warehouseCode, [FromUri]string inventoryType)
        {
            var customerCode = _context.FBAShipOrders.Find(shipOrderId).CustomerCode;

            if (inventoryType == FBAInventoryType.Pallet)
            {
                var palletInventoryInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAMasterOrder)
                    .Include(x => x.FBAPallet.FBACartonLocations)
                    .Where(x => x.AvailablePlts != 0 && x.FBAMasterOrder.CustomerCode == customerCode);

                if (container != null)
                {
                    palletInventoryInDb = palletInventoryInDb.Where(x => x.Container.Contains(container));
                }
                
                if (sku != null)
                {
                    palletInventoryInDb = palletInventoryInDb.Where(x => x.ShipmentId.Contains(sku));
                    var test = palletInventoryInDb.ToList();
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
                    dto.FBACartonLocations = GetCartonLocationDto(dto.Id);
                    // 再次筛选，如果托盘中指定的SKU箱数为0，那么就把这这个托盘除去
                    var cartons = dto.FBACartonLocations;

                    if (sku != null)
                        cartons = cartons.Where(x => x.ShipmentId.Contains(sku));

                    if (amzRef != null)
                        cartons = cartons.Where(x => x.AmzRefId.Contains(amzRef));

                    if (warehouseCode != null)
                        cartons = cartons.Where(x => x.WarehouseCode.Contains(warehouseCode));

                    if (cartons.Sum(x => x.AvailableCtns) == 0)
                        continue;

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
                    .Where(x => x.AvailableCtns != 0 || x.HoldCtns != 0)
                    .Where( x => x.Location != "Pallet" && x.FBAOrderDetail.FBAMasterOrder.CustomerCode == customerCode);

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
                        x.Id,
                        x.FBAShipOrder.Status,
                        x.FBAShipOrder.ShipOrderNumber,
                        x.Container,
                        x.OrderType,
                        x.FBAShipOrder.PlaceTime,
                        x.FBAShipOrder.CustomerCode,
                        x.ShipmentId,
                        x.AmzRefId,
                        x.WarehouseCode,
                        GrossWeight = x.ActualGrossWeight,
                        CBM = x.ActualCBM,
                        Quantity = x.PltsFromInventory,
                        x.Location,
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
                            Location = p.FBAPickDetail.Location,
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
                            Location = p.Location,
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

        // PUT /api/fbainventory/?cartonId={cartonId}&holdCtns={holdCtns}
        [HttpPut]
        public void UpdateHoldCtns([FromUri]int cartonId, [FromUri]int holdCtns)
        {
            var cartonLocationInDb = _context.FBACartonLocations.Find(cartonId);
            var totalAvailableCtns = cartonLocationInDb.AvailableCtns + cartonLocationInDb.HoldCtns;
            cartonLocationInDb.AvailableCtns = totalAvailableCtns - holdCtns;
            cartonLocationInDb.HoldCtns = holdCtns;
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
               .Include(x => x.FBAPallet.FBACartonLocations.Select(c => c.FBAOrderDetail))
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
