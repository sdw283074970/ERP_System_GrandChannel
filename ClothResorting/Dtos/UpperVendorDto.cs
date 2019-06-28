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

        public string CustomerCode { get; set; }

        public string FirstAddressLine { get; set; }

        public string SecondAddressLine { get; set; }

        public string TelNumber { get; set; }

        public string EmailAddress { get; set; }

        public string ContactPerson { get; set; }

        public string Status { get; set; }

        public float OutboundMinCharge { get; set; }

        public float InboundMinCharge { get; set; }

        public DateTime LastUpdatedTime { get; set; }

    }
}