using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Dtos
{
    public class UpperVendorDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DepartmentCode { get; set; }

        public int WarningQuantityLevel { get; set; }

        public int InstockPlts { get; set; }

        public int InstockCtns { get; set; }

        public int PayableInvoices { get; set; }

        public string CustomerCode { get; set; }

        public string FirstAddressLine { get; set; }

        public string SecondAddressLine { get; set; }

        public string TelNumber { get; set; }

        public int ProcessingPlts { get; set; }

        public int ProcessingCtns { get; set; }

        public string EmailAddress { get; set; }

        public string ContactPerson { get; set; }

        public string Status { get; set; }

        public float OutboundMinCharge { get; set; }

        public float InboundMinCharge { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public string LinkedAccount { get; set; }

        //public UpperVendorDto()
        //{
        //    InstockCtns = 0;
        //    InstockPlts = 0;
        //    WarningQuantityLevel = 0;
        //    PayableInvoices = 0;
        //    ProcessingCtns = 0;
        //    ProcessingPlts = 0;
        //}
    }
}