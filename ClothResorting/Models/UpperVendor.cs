﻿using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class UpperVendor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CustomerCode { get; set; }

        public string DepartmentCode { get; set; }

        public string FirstAddressLine { get; set; }

        public string SecondAddressLine { get; set; }

        public string TelNumber { get; set; }

        public string EmailAddress { get; set; }

        public string ContactPerson { get; set; }

        public string Status { get; set; }

        public ICollection<PreReceiveOrder> WorkOrders { get; set; }

        public ICollection<ChargingItem> ChargingItems { get; set; }

        public ICollection<Invoice> Invoices { get; set; }

        public ICollection<FBAMasterOrder> MasterOrders { get; set; }
    }
}