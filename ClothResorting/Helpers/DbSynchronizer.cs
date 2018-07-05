using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class DbSynchronizer
    {
        private ApplicationDbContext _context;

        public DbSynchronizer()
        {
            _context = new ApplicationDbContext();
        }

        //每确认一次carton收取情况，同步一次该po的ctn收货总量、库存数量及pcs收货总量、库存数量
        public void SyncPurchaseOrder(CartonDetail cartonDetailSample)
        {
            var pl = cartonDetailSample.PurchaseOrderSummary;
            var po = pl.PurchaseOrder;
            var preId = pl.PreReceiveOrder.Id;

            //查询preId为当前packinglist且po为当前po的cartondetail对象
            //同步ctn收货总量、库存数量
            pl.ActualReceived = _context.CartonDetails
                .Where(s => s.PurchaseOrderSummary.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderSummary.PurchaseOrder == po)
                .Sum(s => s.ActualReceived);

            pl.Available = _context.CartonDetails
                .Where(s => s.PurchaseOrderSummary.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderSummary.PurchaseOrder == po)
                .Sum(s => s.Available);

            //同步pcs收货总量、库存数量
            pl.ActualReceivedPcs = _context.CartonBreakDowns
                .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                .Where(s => s.PurchaseOrderSummary.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderSummary.PurchaseOrder == po)
                .Sum(s => s.ActualPcs);

            pl.AvailablePcs = _context.CartonBreakDowns
                .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                .Where(s => s.PurchaseOrderSummary.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderSummary.PurchaseOrder == po)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();
        }

        //每确认一次carton收取情况，同步一次该PreRecieveOrder的ctn收货总量、库存数量及pcs收货总量、
            //库存数量
        public void SyncPreReceivedOrder(CartonDetail cartonDetailSample)
        {
            var pl = cartonDetailSample.PurchaseOrderSummary;
            var po = pl.PurchaseOrder;
            var preId = pl.PreReceiveOrder.Id;
            var preReceivedOrder = pl.PreReceiveOrder;

            //同步一次该preReceivedOrder的ctn收货总量、库存数量
            preReceivedOrder.ActualReceived = _context.PurchaseOrderSummaries
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.ActualReceived);

            preReceivedOrder.Available = _context.PurchaseOrderSummaries
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.Available);

            //同步一次该preReceivedOrder的pcs收货总量、库存数量
            preReceivedOrder.ActualReceivedPcs = _context.PurchaseOrderSummaries
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.ActualReceivedPcs);

            preReceivedOrder.AvailablePcs = _context.PurchaseOrderSummaries
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();
        }

        //重新计算(刷新)一次某一具体SpeciesInventory的数据
        public void SyncSpeciesInvenory(int speciesId)
        {
            var id = speciesId;
            var locationInv = 0;
            var permanentInv = 0;

            var speciesInDb = _context.SpeciesInventories.Find(id);

            //查询当前种类在普通库存剩余的总件数
            locationInv = _context.LocationDetails.Where(c => c.PurchaseOrder == speciesInDb.PurchaseOrder
                && c.Style == speciesInDb.Style
                && c.Color == speciesInDb.Color
                && c.Size == speciesInDb.Size)
                .Select(c => c.InvPcs)
                .Sum();

            //查询当前种类在永久库位中剩余的件数
            permanentInv = _context.PermanentLocations.Where(c => c.PurchaseOrder == speciesInDb.PurchaseOrder
                && c.Style == speciesInDb.Style
                && c.Color == speciesInDb.Color
                && c.Size == speciesInDb.Size)
                .Select(c => c.Quantity)
                .Sum();

            //重新计算该种类在数据库的库存数据
            speciesInDb.InvPcs = locationInv + permanentInv;

            //重新计算该种类在数据库的起始数据(调整前数据)
            speciesInDb.OrgPcs = _context.LocationDetails.Where(c => c.PurchaseOrder == speciesInDb.PurchaseOrder
                && c.Style == speciesInDb.Style
                && c.Color == speciesInDb.Color
                && c.Size == speciesInDb.Size)
                .Select(c => c.OrgPcs)
                .Sum();

            _context.SaveChanges();
        }

        //重新计算(刷新)一次某一具体PurchaseOrderSummary的数据
        public void SyncPurchaseOrderInventory(int poId)
        {
            var poInDb = _context.PurchaseOrderInventories.Find(poId);

            //重新计算该po下的所有种类的件数之和
            poInDb.InvPcs = _context.SpeciesInventories.Where(c => c.PurchaseOrder == poInDb.PurchaseOrder)
                .Select(c => c.InvPcs)
                .Sum();

            _context.SaveChanges();
        }
    }
}