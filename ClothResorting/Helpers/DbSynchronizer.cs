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
            var pl = cartonDetailSample.PurchaseOrderOverview;
            var po = pl.PurchaseOrder;
            var preId = pl.PreReceiveOrder.Id;

            //查询preId为当前packinglist且po为当前po的cartondetail对象
            //同步ctn收货总量、库存数量
            pl.ActualReceived = _context.CartonDetails
                .Where(s => s.PurchaseOrderOverview.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderOverview.PurchaseOrder == po)
                .Sum(s => s.ActualReceived);

            pl.Available = _context.CartonDetails
                .Where(s => s.PurchaseOrderOverview.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderOverview.PurchaseOrder == po)
                .Sum(s => s.Available);

            //同步pcs收货总量、库存数量
            pl.ActualReceivedPcs = _context.CartonBreakDowns
                .Include(c => c.PurchaseOrderOverview.PreReceiveOrder)
                .Where(s => s.PurchaseOrderOverview.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderOverview.PurchaseOrder == po)
                .Sum(s => s.ActualPcs);

            pl.AvailablePcs = _context.CartonBreakDowns
                .Include(c => c.PurchaseOrderOverview.PreReceiveOrder)
                .Where(s => s.PurchaseOrderOverview.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderOverview.PurchaseOrder == po)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();
        }

        //每确认一次carton收取情况，同步一次该PreRecieveOrder的ctn收货总量、库存数量及pcs收货总量、
            //库存数量
        public void SyncPreReceivedOrder(CartonDetail cartonDetailSample)
        {
            var pl = cartonDetailSample.PurchaseOrderOverview;
            var po = pl.PurchaseOrder;
            var preId = pl.PreReceiveOrder.Id;
            var preReceivedOrder = pl.PreReceiveOrder;

            //同步一次该preReceivedOrder的ctn收货总量、库存数量
            preReceivedOrder.ActualReceived = _context.PurchaseOrderOverview
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.ActualReceived);

            preReceivedOrder.Available = _context.PurchaseOrderOverview
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.Available);

            //同步一次该preReceivedOrder的pcs收货总量、库存数量
            preReceivedOrder.ActualReceivedPcs = _context.PurchaseOrderOverview
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.ActualReceivedPcs);

            preReceivedOrder.AvailablePcs = _context.PurchaseOrderOverview
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preId)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();
        }
    }
}