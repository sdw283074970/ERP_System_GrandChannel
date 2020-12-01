using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.StaticClass
{
    public static class FBAPackType
    {
        public const string DetailPack = "DetailPack";

        public const string RoughPack = "RoughPack";
    }

    public static class FBAOrderType
    {
        public const string Standard = "Standard";

        public const string ECommerce = "ECommerce";

        public const string MasterOrder = "MasterOrder";

        public const string ShipOrder = "ShipOrder";

        public const string Adjustment = "Adjustment";

        public const string Outbound = "Outbound";

        public const string Inbound = "Inbound";

        public const string TransitShipment = "TRANSIT SHIPMENT";

        public const string DirectSell = "DirectSell";
    }
}