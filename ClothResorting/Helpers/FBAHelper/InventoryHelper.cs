using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;

namespace ClothResorting.Helpers.FBAHelper
{
    public class InventoryHelper
    {
        private ApplicationDbContext _context;

        public InventoryHelper()
        {
            _context = new ApplicationDbContext();
        }

        //输入截止日期，返回到截止日期时FBA的库存列表
        public IList<FBAResidualInventory> GetFBAInventoryList(DateTime closeDate)
        {
            var residualInventoryList = new List<FBAResidualInventory>();

            //获取在指定日期前已发出的拣货列表
            var pickDetailList = _context.FBAPickDetails
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ShipDate <= closeDate);

            //获取在指定日期之前入库的库存列表
            var inventoryInDb = _context.FBACartonLocations
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPickDetails)
                .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                .Where(x => x.FBAOrderDetail.FBAMasterOrder.InboundDate <= closeDate);

            foreach(var inventory in inventoryInDb)
            {
                if (inventory.Location == FBAStatus.InPallet)
                {
                    foreach(var pickCarton in inventory.FBAPickDetailCartons)
                    {
                        inventory.ActualQuantity -= pickCarton.PickCtns;
                    }
                }
                else
                {
                    foreach(var pickcarton in inventory.FBAPickDetails)
                    {
                        inventory.ActualQuantity -= pickcarton.ActualQuantity;
                    }
                }

                if (inventory.ActualQuantity != 0)
                {
                    residualInventoryList.Add(new FBAResidualInventory {
                        Id = inventory.Id,
                        Container = inventory.Container,
                        ShipmentId = inventory.ShipmentId,
                        AmzRefId = inventory.AmzRefId,
                        WarehouseCode = inventory.WarehouseCode,
                        GrossWeightPerCtn = inventory.GrossWeightPerCtn,
                        CBMPerCtn = inventory.CBMPerCtn,
                        ResidualCBM = inventory.CBMPerCtn * inventory.ActualQuantity,
                        ResidualQuantity = inventory.ActualQuantity,
                        Location = inventory.Location
                    });
                }
            }

            return residualInventoryList;
        }
    }

    public class FBAResidualInventory
    {
        public int Id { get; set; }

        public string Container { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public float GrossWeightPerCtn { get; set; }

        public float CBMPerCtn { get; set; }

        public float ResidualCBM { get; set; }

        public int ResidualQuantity { get; set; }

        public string Location { get; set; }
    }
}