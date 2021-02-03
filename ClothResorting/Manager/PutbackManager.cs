using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.FBAModels.StaticModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Manager
{
    public class PutbackManager
    {
        private ApplicationDbContext _context;

        public PutbackManager(ApplicationDbContext context)
        {
            _context = context;
        }

        // 托盘货放回方法
        public void PutbackPickedPalletItemsToNewLocation(FBAPickDetail pickDetailInDb, string newLocation)
        {
            var pickedCtns = 0;

            pickDetailInDb.FBAPalletLocation.ActualPlts -= pickDetailInDb.PltsFromInventory;
            pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.PltsFromInventory;
            pickDetailInDb.FBAPalletLocation.ActualGrossWeight -= pickDetailInDb.ActualGrossWeight;
            pickDetailInDb.FBAPalletLocation.ActualCBM -= pickDetailInDb.ActualCBM;

            pickDetailInDb.FBAPalletLocation.FBAPallet.ActualPallets -= pickDetailInDb.PltsFromInventory;
            pickDetailInDb.FBAPalletLocation.FBAPallet.ActualQuantity -= pickDetailInDb.ActualQuantity;
            pickDetailInDb.FBAPalletLocation.FBAPallet.ActualGrossWeight -= pickDetailInDb.ActualGrossWeight;
            pickDetailInDb.FBAPalletLocation.FBAPallet.ActualCBM -= pickDetailInDb.ActualCBM;
            pickDetailInDb.FBAPalletLocation.FBAPallet.ComsumedPallets -= pickDetailInDb.PltsFromInventory;

            var newFBAPallet = new FBAPallet {
                ActualCBM = pickDetailInDb.ActualCBM,
                ActualGrossWeight = pickDetailInDb.ActualGrossWeight,
                ActualPallets = pickDetailInDb.PltsFromInventory,
                ActualQuantity = pickDetailInDb.ActualQuantity,
                AmzRefId = pickDetailInDb.FBAPalletLocation.FBAPallet.AmzRefId,
                Container = pickDetailInDb.FBAPalletLocation.FBAPallet.Container,
                ShipmentId = pickDetailInDb.FBAPalletLocation.FBAPallet.ShipmentId,
                ComsumedPallets = pickDetailInDb.PltsFromInventory,
                GrandNumber = pickDetailInDb.FBAPalletLocation.FBAPallet.GrandNumber,
                HowToDeliver = pickDetailInDb.FBAPalletLocation.FBAPallet.HowToDeliver,
                LocationStatus = FBAStatus.PutBack,
                IsOverSizeOrOverwidth = pickDetailInDb.FBAPalletLocation.FBAPallet.IsOverSizeOrOverwidth,
                HasSortingMarking = pickDetailInDb.FBAPalletLocation.FBAPallet.HasSortingMarking,
                PalletSize = pickDetailInDb.FBAPalletLocation.FBAPallet.PalletSize,
                WarehouseCode = pickDetailInDb.FBAPalletLocation.FBAPallet.WarehouseCode,
                DoesAppliedLabel = pickDetailInDb.FBAPalletLocation.FBAPallet.DoesAppliedLabel,
                FBAMasterOrder = pickDetailInDb.FBAPalletLocation.FBAPallet.FBAMasterOrder,
                Memo = "Put back from shipping order: " + pickDetailInDb.FBAShipOrder.ShipOrderNumber + " on " + DateTime.Now.ToString("yyyy-MM-dd")
            };

            var newCartonLocationsList = new List<FBACartonLocation>();

            foreach (var p in pickDetailInDb.FBAPickDetailCartons)
            {
                p.FBACartonLocation.ActualQuantity -= p.PickCtns;
                p.FBACartonLocation.PickingCtns -= p.PickCtns;
                p.FBACartonLocation.ActualGrossWeight -= p.FBACartonLocation.GrossWeightPerCtn * p.PickCtns;
                p.FBACartonLocation.ActualCBM -= p.FBACartonLocation.CBMPerCtn * p.PickCtns;

                if (p.FBACartonLocation.ActualGrossWeight < 0)
                    p.FBACartonLocation.ActualGrossWeight = 0;

                if (p.FBACartonLocation.ActualCBM < 0)
                    p.FBACartonLocation.ActualCBM = 0;

                var newFBACartonLocation = new FBACartonLocation {
                    Status = FBAStatus.InPallet,
                    ActualCBM = p.FBACartonLocation.CBMPerCtn * p.PickCtns,
                    ActualGrossWeight = p.FBACartonLocation.GrossWeightPerCtn * p.PickCtns,
                    ActualQuantity = p.PickCtns,
                    LocationStatus = FBAStatus.PutBack,
                    AmzRefId = p.FBACartonLocation.AmzRefId,
                    AvailableCtns = p.PickCtns,
                    CBMPerCtn = p.FBACartonLocation.CBMPerCtn,
                    Container = p.FBACartonLocation.Container,
                    GrandNumber = p.FBACartonLocation.GrandNumber,
                    ShipmentId = p.FBACartonLocation.ShipmentId,
                    GrossWeightPerCtn = p.FBACartonLocation.GrossWeightPerCtn,
                    Location = "Pallet",
                    HowToDeliver = p.FBACartonLocation.HowToDeliver,
                    WarehouseCode = p.FBACartonLocation.WarehouseCode,
                    FBAPallet = newFBAPallet,
                    FBAOrderDetail = p.FBACartonLocation.FBAOrderDetail,
                    FBAMasterOrder = p.FBACartonLocation.FBAMasterOrder,
                    Memo = "Put back from shipping order: " + pickDetailInDb.FBAShipOrder.ShipOrderNumber + " on " + DateTime.Now.ToString("yyyy-MM-dd")
                };

                pickedCtns += p.PickCtns;
                newCartonLocationsList.Add(newFBACartonLocation);
            }

            pickDetailInDb.FBAPalletLocation.ActualQuantity -= pickedCtns;

            var newFBAPalletLocation = new FBAPalletLocation {
                Status = FBAStatus.InStock,
                ActualCBM = pickDetailInDb.ActualCBM,
                ActualGrossWeight = pickDetailInDb.ActualGrossWeight,
                ActualPlts = pickDetailInDb.PltsFromInventory,
                ActualQuantity = pickedCtns,
                AvailablePlts = pickDetailInDb.PltsFromInventory,
                AmzRefId = pickDetailInDb.AmzRefId,
                Container = pickDetailInDb.Container,
                CBMPerPlt = pickDetailInDb.FBAPalletLocation.CBMPerPlt,
                CtnsPerPlt = pickDetailInDb.FBAPalletLocation.CtnsPerPlt,
                GrandNumber = pickDetailInDb.FBAPalletLocation.GrandNumber,
                GrossWeightPerPlt = pickDetailInDb.FBAPalletLocation.GrossWeightPerPlt,
                HowToDeliver = pickDetailInDb.FBAPalletLocation.HowToDeliver,
                Location = newLocation,
                PalletSize = pickDetailInDb.FBAPalletLocation.PalletSize,
                ShipmentId = pickDetailInDb.FBAPalletLocation.ShipmentId,
                LocationStatus = FBAStatus.PutBack,
                WarehouseCode = pickDetailInDb.FBAPalletLocation.WarehouseCode,
                FBAMasterOrder = pickDetailInDb.FBAPalletLocation.FBAMasterOrder,
                FBAPallet = newFBAPallet,
                Memo = "Put back from shipping order: " + pickDetailInDb.FBAShipOrder.ShipOrderNumber + " on " + DateTime.Now.ToString("yyyy-MM-dd")
            };

            _context.FBACartonLocations.AddRange(newCartonLocationsList);
            _context.FBAPallets.Add(newFBAPallet);
            _context.FBAPalletLocations.Add(newFBAPalletLocation);

            _context.FBAPickDetailCartons.RemoveRange(pickDetailInDb.FBAPickDetailCartons);
            _context.FBAPickDetails.Remove(pickDetailInDb);

            _context.SaveChanges();
        }
    }
}