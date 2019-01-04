using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels.BaseClass
{
    public class BaseFBAMasterOrder
    {
        string ETA { get; set; }

        string Carrier { get; set; }

        string Vessel { get; set; }

        string Voy { get; set; }

        string ETD { get; set; }

        string Port { get; set; }

        string PlaceOfReceipt { get; set; }

        string PortOfLoading { get; set; }

        string PlaceOfDelivery { get; set; }

        string Container { get; set; }

        string SealNumber { get; set; }

        string ContainerSize { get; set; }
    }
}