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
                if (masterOrderInDb.CustomerCode == "SUNVALLEY" || masterOrderInDb.CustomerCode == "TEST")
                {
                    if (masterOrderInDb.Agency == "NetSuite")
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

        public void CallBackWhenOutboundOrderReady(FBAShipOrder shipOrderInDb)
        {
            try
            {
                if (shipOrderInDb.CustomerCode == "SUNVALLEY" || shipOrderInDb.CustomerCode == "TEST")
                {
                    //var pickedCtnDetails = _context.FBAPickDetailCartons.Include(x => x.FBAPickDetail.FBAShipOrder).Include(x => x.FBACartonLocation).Where(x => x.FBAPickDetail.FBAShipOrder.Id == shipOrderInDb.Id);
                    if (shipOrderInDb.Agency == "ZT")
                    {
                        _ztManager.UpdateOunboundOrderRequest(shipOrderInDb);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("API call failed. Error message: " + e.Message);
            }
        }

        public void CallBackWhenOutboundOrderReleased(ApplicationDbContext _context, FBAShipOrder shipOrderInDb)
        {
            try
            {
                if (shipOrderInDb.CustomerCode == "SUNVALLEY" || shipOrderInDb.CustomerCode == "TEST")
                {
                    var pickedCtnDetails = _context.FBAPickDetailCartons.Include(x => x.FBAPickDetail.FBAShipOrder).Include(x => x.FBACartonLocation).Where(x => x.FBAPickDetail.FBAShipOrder.Id == shipOrderInDb.Id);
                    if (shipOrderInDb.Agency == "NetSuite" && shipOrderInDb.OrderType == FBAOrderType.Standard)
                    {
                        _nsManager.SendStandardOrderShippedRequest(shipOrderInDb, pickedCtnDetails);
                    }
                    else if (shipOrderInDb.Agency == "NetSuite" && shipOrderInDb.OrderType == FBAOrderType.DirectSell)
                    {
                        _nsManager.SendDirectSellOrderShippedRequest(shipOrderInDb, pickedCtnDetails);
                    }
                    else if (shipOrderInDb.Agency == "ZT")
                    {
                        _ztManager.UpdateOunboundOrderRequest(shipOrderInDb);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("API call failed. Error message: " + e.Message);
            }
        }

        public void CallBackWhenOutboundOrderCancelled(FBAShipOrder shipOrderInDb)
        {
            try
            {
                if (shipOrderInDb.CustomerCode == "SUNVALLEY" || shipOrderInDb.CustomerCode == "TEST")
                {
                    if (shipOrderInDb.Agency == "ZT")
                    {
                        _ztManager.UpdateOunboundOrderRequest(shipOrderInDb);
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