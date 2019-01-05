using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels.BaseClass
{
    public class BaseFBAMasterOrder
    {
        public string ETA { get; set; }

        public string Carrier { get; set; }

        public string Vessel { get; set; }

        public string Voy { get; set; }

        public string ETD { get; set; }

        public string ETAPort { get; set; }

        public string PlaceOfReceipt { get; set; }

        public string PortOfLoading { get; set; }

        public string PortOfDischarge { get; set; }

        public string PlaceOfDelivery { get; set; }

        public string Container { get; set; }

        public string SealNumber { get; set; }

        public string ContainerSize { get; set; }

        public void AssembleFirstPart(string eta, string carrier, string vessel, string voy, string etd)
        {
            ETA = eta;
            Carrier = carrier;
            Vessel = vessel;
            Voy = voy;
            ETD = etd;
        }

        public void AssembeSecondPart(string etaPort, string placeOfReceipt, string portOfLoading, string portOfDischarge, string placeOfDelivery)
        {
            ETAPort = etaPort;
            PlaceOfReceipt = placeOfReceipt;
            PortOfLoading = portOfLoading;
            PortOfDischarge = portOfDischarge;
            PlaceOfDelivery = placeOfDelivery;
        }

        public void AssembeThirdPart(string sealNumber, string containerSize, string container)
        {
            SealNumber = sealNumber;
            Container = container;
            ContainerSize = containerSize;
        }
    }
}