using ClothResorting.Models;
using ClothResorting.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class CartonBreakdownGetter
    {
        private ICartonBreakDown _c;

        public CartonBreakdownGetter(CartonBreakDown c)
        {
            _c = c;
        }

        public string GetContainerNumber()
        {
            return _c.PurchaseOrderSummary == null ? "N/A" : _c.PurchaseOrderSummary.PreReceiveOrder.ContainerNumber;
        }

        public string GetPurchaseOrder()
        {
            return _c.PurchaseOrder;
        }

        public string GetVendor()
        {
            return _c.PurchaseOrderSummary == null ? "N/A" : _c.PurchaseOrderSummary.PreReceiveOrder.CustomerName;
        }

        public string GetStyle()
        {
            return _c.Style;
        }

        public string GetColor()
        {
            return _c.Color;
        }

        public int? GetCartonNumberFrom()
        {
            return _c.CartonNumberRangeFrom == null ? 0 : _c.CartonNumberRangeFrom;
        }

        public int? GetCartonNumberTo()
        {
            return _c.CartonNumberRangeTo == null ? 0 : _c.CartonNumberRangeTo;
        }

        public string GetRunCode()
        {
            return _c.RunCode;
        }

        public string GetSize()
        {
            return _c.Size;
        }

        public int? GetAvailablePcs()
        {
            return _c.AvailablePcs;
        }

        public int? GetReceivedPcs()
        {
            return _c.ActualPcs;
        }

        public string GetLocation()
        {
            return _c.Location;
        }

        public DateTime? GetReceivedDate()
        {
            return _c.ReceivedDate;
        }
    }
}