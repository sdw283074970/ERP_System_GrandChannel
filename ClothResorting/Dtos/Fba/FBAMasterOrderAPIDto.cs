using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos.Fba
{
    public class FBAMasterOrderAPIDto
    {
        [Required]
        public float TotalCBM { get; set; }

        [Required]
        public int TotalCtns { get; set; }

        public DateTime ETA { get; set; }

        public string Carrier { get; set; }

        public string Vessel { get; set; }

        public string Voy { get; set; }

        public DateTime ETD { get; set; }

        public string ETAPort { get; set; }

        public string PlaceOfReceipt { get; set; }

        public string PortOfLoading { get; set; }

        public string PortOfDischarge { get; set; }

        public string PlaceOfDelivery { get; set; }

        public string Container { get; set; }

        public string SealNumber { get; set; }

        public string ContainerSize { get; set; }

        public float TotalAmount { get; set; }

        [Required]
        public FBACustomerAPIDto Customer { get; set; }

        [Required]
        public ICollection<FBAOrderDetailAPIDto> OrderDetails { get; set; }
    }

    public class FBACustomerAPIDto
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }

        public string CustomerCode { get; set; }
    }

    public class FBAOrderDetailAPIDto
    {
        public string Container { get; set; }

        [Required]
        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public string HowToDeliver { get; set; }

        public string LotSize { get; set; }

        [Required]
        public float GrossWeight { get; set; }

        [Required]
        public float CBM { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string Remark { get; set; }
    }
}
