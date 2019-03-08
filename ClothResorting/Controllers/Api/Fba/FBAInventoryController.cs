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

        // GET /api/fba/fbaiventory/?container={container}&sku={sku}&amzRef={amzRef}&warehouseCode={warehouseCode}&inventoryType={inventoryType} 搜索获取可拣货列表
        [HttpGet]
        public IHttpActionResult GetFBAInventoryViaContainer([FromUri]string container, [FromUri]string sku, [FromUri]string amzRef, [FromUri]string warehouseCode, [FromUri]string inventoryType)
        {
            if (inventoryType == FBAInventoryType.Pallet)
            {
                var palletInventoryInDb = _context.FBAPalletLocations.Where(x => x.AvailablePlts != 0);

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

                return Ok(Mapper.Map<IEnumerable<FBAPalletLocation>, IEnumerable<FBAPalletLocationDto>>(palletInventoryInDb));
            }
            else
            {
                var cartonInventoryInDb = _context.FBACartonLocations.Where(x => x.AvailableCtns != 0);

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

                return Ok(Mapper.Map<IEnumerable<FBACartonLocation>, IEnumerable<FBACartonLocationDto>>(cartonInventoryInDb));
            }
        }

        // GET /api/FBAinventory/?locationId={locationId}}&locationType={locationType}
        [HttpGet]
        public IHttpActionResult GetInventoryRelatedPickDetails([FromUri]int locationId, [FromUri]string locationType)
        {
            if (locationType == FBALocationType.Pallet)
            {
                var pickDetailsList = _context.FBAPickDetails
                    .Include(x => x.FBAShipOrder)
                    .Include(x => x.FBAPalletLocation)
                    .Where(x => x.FBAPalletLocation.Id == locationId)
                    .Select(x => new {
                        Id = x.Id,
                        ShipOrderNumber = x.FBAShipOrder.ShipOrderNumber,
                        Container = x.Container,
                        OrderType = x.OrderType,
                        CustomerCode= x.FBAShipOrder.CustomerCode,
                        ShipmentId = x.ShipmentId,
                        AmzRefId = x.AmzRefId,
                        WarehouseCode = x.WarehouseCode,
                        GrossWeight = x.ActualGrossWeight,
                        CBM = x.ActualCBM,
                        PickQuantity = x.ActualPlts,
                        Location = x.Location,
                        ShipOrderId = x.FBAShipOrder.Id
                    })
                    .ToList();

                return Ok(pickDetailsList);
            }
            else if (locationType == FBALocationType.Carton)
            {
                var pickDetailsList = _context.FBAPickDetails
                    .Include(x => x.FBAShipOrder)
                    .Include(x => x.FBAPalletLocation)
                    .Where(x => x.FBACartonLocation.Id == locationId)
                    .Select(x => new {
                        Id = x.Id,
                        ShipOrderNumber = x.FBAShipOrder.ShipOrderNumber,
                        Container = x.Container,
                        OrderType = x.OrderType,
                        CustomerCode = x.FBAShipOrder.CustomerCode,
                        ShipmentId = x.ShipmentId,
                        AmzRefId = x.AmzRefId,
                        WarehouseCode = x.WarehouseCode,
                        GrossWeight = x.ActualGrossWeight,
                        CBM = x.ActualCBM,
                        PickQuantity = x.ActualQuantity,
                        Location = x.Location,
                        ShipOrderId = x.FBAShipOrder.Id
                    })
                    .ToList();

                return Ok(pickDetailsList);
            }

            return Ok();
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
                locationInDb.AvailablePlts = 0;
                locationInDb.Status = FBAStatus.Relocated;

                if (locationInDb.ShippedPlts == 0)
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
                locationInDb.AvailableCtns = 0;
                locationInDb.Status = FBAStatus.Relocated;

                if (locationInDb.ShippedCtns == 0)
                {
                    _context.FBACartonLocations.Remove(locationInDb);
                }
            }

            _context.SaveChanges();
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
}
