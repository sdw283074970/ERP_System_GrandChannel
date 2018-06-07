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
        public void SyncPurchaseOrder(SilkIconCartonDetail cartonDetailSample)
        {
            var pl = cartonDetailSample.SilkIconPackingList;
            var po = pl.PurchaseOrderNumber;
            var preId = pl.SilkIconPreReceiveOrder.Id;

            //查询preId为当前packinglist且po为当前po的cartondetail对象
            //同步ctn收货总量、库存数量
            pl.ActualReceived = _context.SilkIconCartonDetails
                .Where(s => s.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preId
                    && s.SilkIconPackingList.PurchaseOrderNumber == po)
                .Sum(s => s.ActualReceived);

            pl.Available = _context.SilkIconCartonDetails
                .Where(s => s.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preId
                    && s.SilkIconPackingList.PurchaseOrderNumber == po)
                .Sum(s => s.Available);

            //同步pcs收货总量、库存数量
            pl.ActualReceivedPcs = _context.CartonBreakDowns
                .Include(c => c.SilkIconPackingList.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preId
                    && s.SilkIconPackingList.PurchaseOrderNumber == po)
                .Sum(s => s.ActualPcs);

            pl.AvailablePcs = _context.CartonBreakDowns
                .Include(c => c.SilkIconPackingList.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preId
                    && s.SilkIconPackingList.PurchaseOrderNumber == po)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();
        }

        //每确认一次carton收取情况，同步一次该po的ctn收货总量、库存数量及pcs收货总量、库存数量
        public void SyncPreReceivedOrder(SilkIconCartonDetail cartonDetailSample)
        {
            var pl = cartonDetailSample.SilkIconPackingList;
            var po = pl.PurchaseOrderNumber;
            var preId = pl.SilkIconPreReceiveOrder.Id;
            var preReceivedOrder = pl.SilkIconPreReceiveOrder;

            //同步一次该preReceivedOrder的ctn收货总量、库存数量
            preReceivedOrder.ActualReceived = _context.SilkIconPackingLists
                .Include(s => s.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPreReceiveOrder.Id == preId)
                .Sum(s => s.ActualReceived);

            preReceivedOrder.Available = _context.SilkIconPackingLists
                .Include(s => s.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPreReceiveOrder.Id == preId)
                .Sum(s => s.Available);

            //同步一次该preReceivedOrder的pcs收货总量、库存数量
            preReceivedOrder.ActualReceivedPcs = _context.SilkIconPackingLists
                .Include(s => s.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPreReceiveOrder.Id == preId)
                .Sum(s => s.ActualReceivedPcs);

            preReceivedOrder.AvailablePcs = _context.SilkIconPackingLists
                .Include(s => s.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPreReceiveOrder.Id == preId)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();
        }
    }
}