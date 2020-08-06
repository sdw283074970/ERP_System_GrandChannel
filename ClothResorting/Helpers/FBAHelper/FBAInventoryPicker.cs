using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;
using ClothResorting.Controllers.Api.Fba;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAInventoryPicker
    {
        private ApplicationDbContext _context;

        public FBAInventoryPicker()
        {
            _context = new ApplicationDbContext();
        }

        public IList<FBAPalletLocationDto> SearchPalletInventory(string customerCode, string container, string sku, string amzRef, string warehouseCode)
        {
            var palletInventoryInDb = _context.FBAPalletLocations
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBAPallet.FBACartonLocations)
                .Where(x => x.FBAMasterOrder.CustomerCode == customerCode);

            if (container != null && container != "")
            {
                palletInventoryInDb = palletInventoryInDb.Where(x => x.Container.ToUpper().Contains(container.ToUpper()));
            }

            if (sku != null && container != "")
            {
                palletInventoryInDb = palletInventoryInDb.Where(x => x.ShipmentId.ToUpper().Contains(sku.ToUpper()));
            }

            if (amzRef != null && amzRef != "")
            {
                palletInventoryInDb = palletInventoryInDb.Where(x => x.AmzRefId.ToUpper().Contains(amzRef.ToUpper()));
            }

            if (warehouseCode != null && warehouseCode != "")
            {
                palletInventoryInDb = palletInventoryInDb.Where(x => x.WarehouseCode.ToUpper().Contains(warehouseCode.ToUpper()));
            }

            var palletInventoryDto = new List<FBAPalletLocationDto>();

            foreach (var p in palletInventoryInDb)
            {
                var dto = Mapper.Map<FBAPalletLocation, FBAPalletLocationDto>(p);
                dto.FBACartonLocations = GetCartonLocationDto(dto.Id);
                // 再次筛选，如果托盘中指定的SKU箱数为0，且托盘数量为0，那么就把这这个托盘除去

                if (sku != null && sku != "")
                    dto.FBACartonLocations = dto.FBACartonLocations.Where(x => x.ShipmentId.ToUpper().Contains(sku.ToUpper()));

                if (amzRef != null && amzRef != "")
                    dto.FBACartonLocations = dto.FBACartonLocations.Where(x => x.AmzRefId.ToUpper().Contains(amzRef.ToUpper()));

                if (warehouseCode != null && warehouseCode != "")
                    dto.FBACartonLocations = dto.FBACartonLocations.Where(x => x.WarehouseCode.ToUpper().Contains(warehouseCode.ToUpper()));

                if (dto.FBACartonLocations.Sum(x => x.AvailableCtns) == 0 && dto.AvailablePlts == 0)
                    continue;

                dto.InboundDate = p.FBAMasterOrder.InboundDate;
                dto.OriginalTotalCtns = p.FBAPallet.FBACartonLocations.Sum(x => x.ActualQuantity);
                dto.CurrentAvailableCtns = p.FBAPallet.FBACartonLocations.Sum(x => x.AvailableCtns);
                palletInventoryDto.Add(dto);
            }

            var result = new List<FBAPalletLocationDto>();

            foreach (var p in palletInventoryDto)
            {
                if (p.FBACartonLocations.Count() > 0)
                    result.Add(p);
            }

            return result;
        }

        public IList<FBACartonLocationDto> SearchCartonInventory(string customerCode, string container, string sku, string amzRef, string warehouseCode)
        {
            var cartonInventoryInDb = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                .Where(x => x.AvailableCtns != 0 || x.HoldCtns != 0)
                .Where(x => x.Location != "Pallet" && x.FBAOrderDetail.FBAMasterOrder.CustomerCode == customerCode);

            if (container != null)
            {
                cartonInventoryInDb = cartonInventoryInDb.Where(x => x.Container.ToUpper().Contains(container.ToUpper()));
            }

            if (sku != null)
            {
                cartonInventoryInDb = cartonInventoryInDb.Where(x => x.ShipmentId.ToUpper().Contains(sku.ToUpper()));
            }

            if (amzRef != null)
            {
                cartonInventoryInDb = cartonInventoryInDb.Where(x => x.AmzRefId.ToUpper().Contains(amzRef.ToUpper()));
            }

            if (warehouseCode != null)
            {
                cartonInventoryInDb = cartonInventoryInDb.Where(x => x.WarehouseCode.ToUpper().Contains(warehouseCode.ToUpper()));
            }

            var cartonInventoryDto = new List<FBACartonLocationDto>();

            foreach (var c in cartonInventoryInDb)
            {
                var dto = Mapper.Map<FBACartonLocation, FBACartonLocationDto>(c);
                dto.InboundDate = c.FBAOrderDetail.FBAMasterOrder.InboundDate;
                cartonInventoryDto.Add(dto);
            }

            return cartonInventoryDto;
        }

        public FBAPickDetail CreateFBAPickDetailFromPalletLocation(FBAPalletLocation fbaPalletLocationInDb, FBAShipOrder shipOrder, int pltQuantity, int newPltQuantity, IList<FBAPickDetailCarton> pickDetailCartonList, IList<PickCartonDto> objArray, PickingStatus pickingStatus)
        {
            var pickDetail = new FBAPickDetail();

            pickDetail.AssembleUniqueIndex(fbaPalletLocationInDb.Container, fbaPalletLocationInDb.GrandNumber);
            pickDetail.AssembleFirstStringPart(fbaPalletLocationInDb.ShipmentId, fbaPalletLocationInDb.AmzRefId, fbaPalletLocationInDb.WarehouseCode);
            pickDetail.AssembleActualDetails(0, 0, objArray.Sum(x => x.PickQuantity));

            pickDetail.Status = FBAStatus.Picking;
            pickDetail.Size = fbaPalletLocationInDb.PalletSize;
            pickDetail.PickableCtns = objArray.Sum(x => x.PickQuantity);
            pickDetail.NewPlts = newPltQuantity;
            pickDetail.PltsFromInventory = pltQuantity;
            //pickDetail.ActualPlts = pltQuantity + newPltQuantity;
            //现在强行规定，实际出库托盘数量为0，防止仓库偷懒不调整
            pickDetail.ActualPlts = 0;
            pickDetail.CtnsPerPlt = fbaPalletLocationInDb.CtnsPerPlt;
            pickDetail.Location = fbaPalletLocationInDb.Location;

            fbaPalletLocationInDb.PickingPlts += pltQuantity;
            //如果需要在库存中体现新打的托盘数量，禁用上面一行，启用下面一行
            //fbaPalletLocationInDb.PickingPlts += pltQuantity + newPltQuantity;

            fbaPalletLocationInDb.AvailablePlts -= pltQuantity;
            fbaPalletLocationInDb.Status = FBAStatus.Picking;

            pickDetail.HowToDeliver = fbaPalletLocationInDb.HowToDeliver;
            pickDetail.FBAPalletLocation = fbaPalletLocationInDb;
            pickDetail.OrderType = FBAOrderType.Standard;
            pickDetail.HowToDeliver = fbaPalletLocationInDb.HowToDeliver;
            pickDetail.InboundDate = fbaPalletLocationInDb.FBAMasterOrder.InboundDate;

            pickDetail.FBAShipOrder = shipOrder;

            var cartonLocationInPalletsInDb = fbaPalletLocationInDb.FBAPallet.FBACartonLocations;

            foreach (var obj in objArray)
            {
                if (obj.PickQuantity == 0)
                {
                    continue;
                }

                var cartonInPalletInDb = cartonLocationInPalletsInDb.SingleOrDefault(x => x.Id == obj.Id);

                cartonInPalletInDb.PickingCtns += obj.PickQuantity;

                var pickDetailCarton = new FBAPickDetailCarton
                {
                    PickCtns = obj.PickQuantity,
                    FBAPickDetail = pickDetail,
                    FBACartonLocation = cartonInPalletInDb
                };

                cartonInPalletInDb.AvailableCtns -= obj.PickQuantity;
                pickDetail.ActualCBM += obj.PickQuantity * cartonInPalletInDb.CBMPerCtn;
                pickDetail.ActualGrossWeight += obj.PickQuantity * cartonInPalletInDb.GrossWeightPerCtn;

                pickDetailCartonList.Add(pickDetailCarton);

                pickingStatus.PickedCtns = obj.PickQuantity;
                pickingStatus.InstockCtns = cartonInPalletInDb.AvailableCtns;
            }

            // 如果捡完了托盘数量但是箱子还有剩余，则报错
            var availablePlts = fbaPalletLocationInDb.FBAPallet.FBAPalletLocations.Sum(x => x.AvailablePlts);
            var availableCtns = fbaPalletLocationInDb.FBAPallet.FBACartonLocations.Sum(x => x.AvailableCtns);

            if (availablePlts == 0 && availableCtns != 0)
            {
                throw new Exception("Pick failed. The pallets number of SKU " + fbaPalletLocationInDb.ShipmentId + " will be 0 after this pick but there are still many cartons inside. Please make sure there is no thing left before picking the last pallte.");
            }

            // 如果托盘中的箱子捡完了但是托盘数没捡完，则自动把所有剩下的托盘数带上

            if (availableCtns == 0 && availablePlts != 0)
            {
                pickDetail.PltsFromInventory = availablePlts;
                fbaPalletLocationInDb.PickingPlts += availablePlts;
                fbaPalletLocationInDb.AvailablePlts = 0;
            }

            pickingStatus.NewPlts = newPltQuantity;
            pickingStatus.PickedPlts = pltQuantity;

            return pickDetail;
        }

        public FBAPickDetail CreateFBAPickDetailFromCartonLocation(FBACartonLocation fbaCartonLocationInDb, FBAShipOrder shipOrderInDb, int ctnQuantity, IList<FBAPickDetailCarton> pickDetailCartonList)
        {
            var pickDetail = new FBAPickDetail();

            pickDetail.AssembleUniqueIndex(fbaCartonLocationInDb.Container, fbaCartonLocationInDb.GrandNumber);
            pickDetail.AssembleFirstStringPart(fbaCartonLocationInDb.ShipmentId, fbaCartonLocationInDb.AmzRefId, fbaCartonLocationInDb.WarehouseCode);
            pickDetail.AssembleActualDetails(fbaCartonLocationInDb.GrossWeightPerCtn * ctnQuantity, fbaCartonLocationInDb.CBMPerCtn * ctnQuantity, ctnQuantity);

            pickDetail.Status = FBAStatus.Picking;
            pickDetail.Size = FBAStatus.Na;
            pickDetail.CtnsPerPlt = 0;
            pickDetail.PickableCtns = ctnQuantity;
            pickDetail.Location = fbaCartonLocationInDb.Location;
            pickDetail.InboundDate = fbaCartonLocationInDb.FBAOrderDetail.FBAMasterOrder.InboundDate;

            fbaCartonLocationInDb.PickingCtns += ctnQuantity;
            fbaCartonLocationInDb.AvailableCtns -= ctnQuantity;
            fbaCartonLocationInDb.Status = FBAStatus.Picking;

            pickDetail.FBACartonLocation = fbaCartonLocationInDb;
            pickDetail.OrderType = FBAOrderType.ECommerce;
            pickDetail.HowToDeliver = fbaCartonLocationInDb.HowToDeliver;

            pickDetail.FBAShipOrder = shipOrderInDb;

            var pickCartonDetail = new FBAPickDetailCarton();
            pickCartonDetail.FBACartonLocation = fbaCartonLocationInDb;
            pickCartonDetail.FBAPickDetail = pickDetail;
            pickCartonDetail.PickCtns = ctnQuantity;

            pickDetailCartonList.Add(pickCartonDetail);
            return pickDetail;
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
}