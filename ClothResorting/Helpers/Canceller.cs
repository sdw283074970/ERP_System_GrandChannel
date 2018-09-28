using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Helpers
{
    public class Canceller
    {
        private ApplicationDbContext _context;

        public Canceller()
        {
            _context = new ApplicationDbContext();
        }

        //FreeCountry的取消订单方法
        public void CancelFreeCountryOrder(int shipOrderId, IEnumerable<PickDetail> pickDetailsInDb)
        {
            var locationDeatilsInDb = _context.FCRegularLocationDetails
                .Where(x => x.Id > 0)
                .ToList();

            foreach (var pickDetail in pickDetailsInDb)
            {
                var locationDetail = locationDeatilsInDb.SingleOrDefault(x => x.Id == pickDetail.LocationDetailId);

                locationDetail.AvailableCtns += pickDetail.PickCtns;
                locationDetail.AvailablePcs += pickDetail.PickPcs;

                locationDetail.PickingCtns -= pickDetail.PickCtns;
                locationDetail.PickingPcs -= pickDetail.PickPcs;

                if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns != 0)
                {
                    locationDetail.Status = Status.InStock;
                }
            }

            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);

            var diagnosticsInDb = _context.PullSheetDiagnostics
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == shipOrderId);

            _context.PickDetails.RemoveRange(pickDetailsInDb);
            _context.PullSheetDiagnostics.RemoveRange(diagnosticsInDb);
            _context.SaveChanges();

            _context.ShipOrders.Remove(shipOrderInDb);
            _context.SaveChanges();
        }

        //取消SilkIcon订单的方法
        public void CancelSilkIconShipOrder()
        {

        }
    }
}