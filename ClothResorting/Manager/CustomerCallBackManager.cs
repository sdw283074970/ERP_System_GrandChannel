using ClothResorting.Manager.NetSuit;
using ClothResorting.Manager.ZT;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Manager
{
    public class CustomerCallbackManager
    {
        private NetSuitManager _nsManager;
        private ZTManager _ztManager;

        public CustomerCallbackManager()
        {
            _nsManager = new NetSuitManager();
            _ztManager = new ZTManager();
        }

        public void CallBackWhenInboundOrderArrrived()
        {

        }

        public void CallBackWhenInboundOrderStart()
        {

        }

        public void CallBackWhenInboundOrderCompleted(FBAMasterOrder masterOrderInDb)
        {
            try
            {
                if (masterOrderInDb.CustomerCode == "SUNVALLEY")
                {
                    if (masterOrderInDb.Agency == "NetSuit")
                    {
                        _nsManager.SendStandardOrderInboundRequest(masterOrderInDb);
                    }
                    else if (masterOrderInDb.Agency == "ZT")
                    {
                        _ztManager.SendInboundCompleteRequest(masterOrderInDb);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("API call failed. Error message: " + e.Message);
            }
        }

        public void CallBackWhenOutboundOrderApproved()
        {

        }

        public void CallBackWhenOutboundOrderStart()
        {

        }

        public void CallBackWhenOutboundOrderReleased(ApplicationDbContext _context, FBAShipOrder shipOrderInDb)
        {
            try
            {
                if (shipOrderInDb.CustomerCode == "SUNVALLEY")
                {
                    var pickedCtnDetails = _context.FBAPickDetailCartons.Include(x => x.FBAPickDetail.FBAShipOrder).Include(x => x.FBACartonLocation).Where(x => x.FBAPickDetail.FBAShipOrder.Id == shipOrderInDb.Id);
                    if (shipOrderInDb.Agency == "NetSuit" && shipOrderInDb.OrderType == FBAOrderType.Standard)
                    {
                        _nsManager.SendStandardOrderShippedRequest(shipOrderInDb, pickedCtnDetails);
                    }
                    else if (shipOrderInDb.Agency == "NetSuit" && shipOrderInDb.OrderType == FBAOrderType.DirectSell)
                    {
                        _nsManager.SendDirectSellOrderShippedRequest(shipOrderInDb, pickedCtnDetails);
                    }
                    else if (shipOrderInDb.Agency == "ZT")
                    {
                        _ztManager.SendShippedOrderRequest(shipOrderInDb);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("API call failed. Error message: " + e.Message);
            }
        }
    }
}