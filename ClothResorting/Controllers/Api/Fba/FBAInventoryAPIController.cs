using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInventoryAPIController : ApiController
    {
        private ApplicationDbContext _context;
        private FBAexAPIValidator _validator;
        private FBAInventoryHelper _inventoryHelper;

        public FBAInventoryAPIController()
        {
            _context = new ApplicationDbContext();
            _validator = new FBAexAPIValidator();
            _inventoryHelper = new FBAInventoryHelper();
        }

        // GET /api/FBAInventoryAPI/?customerCode={customerCode}&requestId={requestId}&version={version}
        [HttpGet]
        public IHttpActionResult GetInstantInventory(string appKey, string  customerCode, string requestId, string version, string sign)
        {
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);
            var jsonResult = _validator.ValidateSign(appKey, customerInDb, requestId, version, sign);

            if (jsonResult.Code != 200)
                return Json(jsonResult);

            var info = _inventoryHelper.GetFBAInventoryResidualInfo(customerCode, new DateTime(1992, 11, 17), DateTime.Now);

            var result = new InventoryBody
            {
                CustomerCode = customerCode,
                DateRange = info.StartDate.ToString("yyyy-MM-dd") + " ~ " + info.CloseDate.ToString("yyyy-MM-dd"),
                ReportDate = DateTime.Now.ToString("yyyy-MM-dd"),
                InstockCtns = (int)info.CurrentTotalCtns,
                InstockPlts = info.CurrentTotalPlts,
                ProcessingCtns = info.TotalPickingCtns,
                ProcessingPlts = info.TotalPickingPlts
            };

            var inventoryCtns = new List<InventoryCtns>();

            foreach(var c in info.FBACtnInventories)
            {
                inventoryCtns.Add(new InventoryCtns { 
                    Container = c.Container,
                    StorageType = c.Type,
                    SubCustomer = c.SubCustomer,
                    ShipmentId = c.ShipmentId,
                    AmzRefId = c.AmzRefId,
                    WarehouseCode = c.WarehouseCode,
                    InboundDate = c.InboundDate,
                    GrossWeightPerCtn = c.GrossWeightPerCtn,
                    CBMPerCtn = c.CBMPerCtn,
                    OriginalQuantity = c.OriginalQuantity,
                    ProcessingQuantity = c.PickingCtns,
                    InstockQuantity = c.ResidualQuantity,
                    HoldCtns = c.HoldQuantity,
                    Location = c.Location
                });
            }

            result.InventoryCtns = inventoryCtns;

            jsonResult.Body = result;

            return Ok(jsonResult);
        }
    }

    public  class InventoryBody
    {
        public string CustomerCode { get; set; }

        public string DateRange { get; set; }

        public string ReportDate { get; set; }

        public int InstockCtns { get; set; }

        public int InstockPlts { get; set; }

        public int ProcessingCtns { get; set; }

        public int ProcessingPlts { get; set; }

        public IList<InventoryCtns> InventoryCtns { get; set; }
    }

    public class InventoryCtns
    {
        public string Container { get; set; }

        public string StorageType { get; set; }

        public string SubCustomer { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public DateTime InboundDate { get; set; }

        public float GrossWeightPerCtn { get; set; }

        public float CBMPerCtn { get; set; }

        public int OriginalQuantity { get; set; }

        public int ProcessingQuantity { get; set; }

        public int InstockQuantity { get; set; }

        public int HoldCtns { get; set; }

        public string Location { get; set; }
    }
}
