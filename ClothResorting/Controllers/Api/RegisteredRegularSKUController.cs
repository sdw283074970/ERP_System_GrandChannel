using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class RegisteredRegularSKUController : ApiController
    {
        private ApplicationDbContext _context;
        private int _registeredItemAdded;
        private int _unregisteredItemAdded;
        private int _registeredItemDeducted;
        private int _unregisteredItemDeducted;
        private int _totalOrgPcs;

        public RegisteredRegularSKUController()
        {
            _context = new ApplicationDbContext();
            _registeredItemAdded = 0;
            _registeredItemDeducted = 0;
            _unregisteredItemAdded = 0;
            _unregisteredItemDeducted = 0;
        }

        // GET /api/RegisteredRegularSKU/?vendor={vendor}&upc={upc}
        [HttpGet]
        public IHttpActionResult GetUPCInfo([FromUri]string vendor, [FromUri]string upc)
        {
            var resultStr = string.Empty;
            var upcInDb = _context.RegularSKURegistrations
                .SingleOrDefault(x => x.Customer == vendor
                    && x.UPCNumber == upc);

            if (upcInDb != null)
                resultStr = upcInDb.UPCNumber + ";" + upcInDb.Style + ";" + upcInDb.Color + ";" + upcInDb.Size;
            else
                resultStr = upc + ";;;";

            return Ok(resultStr);
        }

        // POST /api/RegisteredRegularSKU/?batchId={batchId}&container={container}&cartonRange={cartonRange}&operation={operation}
        [HttpPost]
        public IHttpActionResult CreateBatchSKUItems([FromUri]int batchId, [FromUri]string container, [FromUri]string cartonRange, [FromUri]string operation, [FromBody]IEnumerable<UPCItem> items)
        {
            var poSummaryInDb = _context.POSummaries
                .Include(x => x.PreReceiveOrder.UpperVendor)
                .SingleOrDefault(x => x.Container == container
                    && x.Id == batchId);

            if (poSummaryInDb == null)
            {
                return Ok("The container # doesn't match the Batch Id.");
            }

            _totalOrgPcs = items.Count();

            var registeredSKU = _context.RegularSKURegistrations.ToList();
            var newItemList = GenerateNewItemList(poSummaryInDb, registeredSKU, cartonRange, items);

            if (operation == "Add")
            {
                var list = MergePltItems(_context, poSummaryInDb, newItemList, cartonRange, "Add");
                _context.RegularCartonDetails.AddRange(list);
                _context.SaveChanges();

                return Created(Request.RequestUri, "Successfully added " + _registeredItemAdded + " pcs registered item and " + _unregisteredItemAdded + " pcs unregistered item.");
            }
            else if (operation == "Minus")
            {
                var list = MergePltItems(_context, poSummaryInDb, newItemList, cartonRange, "Minus");
                _context.RegularCartonDetails.AddRange(list);
                _context.SaveChanges();

                return Created(Request.RequestUri, "Successfully deducted " + _registeredItemDeducted + " pcs unregistered item and " + _unregisteredItemDeducted + " pcs unregistered item." + 
                    (_totalOrgPcs - _registeredItemDeducted - _unregisteredItemDeducted) + " pcs item was not found in system.");

            }

            return Ok("No operation applied.");
        }

        // POST /api/RegisteredRegularSKU/?vendor={vendor}&bc={bc}&style={style}&color={color}&size={size};
        [HttpPost]
        public IHttpActionResult RegisterSKUItem([FromUri]string vendor, [FromUri]string bc, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var skuInDb = _context.RegularSKURegistrations.SingleOrDefault(x => x.UPCNumber == bc);

            if (skuInDb == null)
            {
                _context.RegularSKURegistrations.Add(new RegularSKURegistration
                {
                    Color = color,
                    UPCNumber = bc,
                    Size = size,
                    SizeNumber = 0,
                    Style = style,
                    Customer = vendor
                });;
                _context.SaveChanges();
                return Created(Request.RequestUri, "Success");
            }
            else
            {
                return Ok("Barcode " + bc + " has already existed in the database.");
            }
        }

        // POST /api/RegisteredRegularSKU/?vendor={vendor}&bc={bc}&style={style}&color={color}&size={size};
        [HttpPut]
        public IHttpActionResult UpdateSKUItem([FromUri]string vendor, [FromUri]string bc, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var skuInDb = _context.RegularSKURegistrations.SingleOrDefault(x => x.UPCNumber == bc);

            if (skuInDb == null)
            {
                return Ok("Barcode " + bc + " doesn't existed in the database.");
            }
            else
            {
                skuInDb.Customer = vendor;
                skuInDb.UPCNumber = bc;
                skuInDb.Style = style;
                skuInDb.Size = size;
                skuInDb.Color = color;
                skuInDb.SizeNumber = 0;

                return Ok("Success");
            }
        }

        IList<RegularCartonDetail> GenerateNewItemList(POSummary poSummaryInDb, IEnumerable<RegularSKURegistration> registeredSKU, string cartonRange, IEnumerable<UPCItem> items)
        {
            var newItemList = new List<RegularCartonDetail>();

            foreach (var i in items)
            {
                var refItem = registeredSKU.FirstOrDefault(x => x.UPCNumber == i.UPCNumber);
                var itemInList = newItemList.SingleOrDefault(x => x.UPCNumber == i.UPCNumber && x.CartonRange == cartonRange);

                if (refItem == null)
                {
                    var errorItem = newItemList.SingleOrDefault(x => x.UPCNumber == i.UPCNumber);
                    _unregisteredItemAdded += 1;

                    if (errorItem != null)
                    {
                        errorItem.PcsPerCarton += 1;
                    }
                    else
                    {
                        newItemList.Add(new RegularCartonDetail
                        {
                            UPCNumber = i.UPCNumber,
                            Receiver = "SCAN",
                            Batch = poSummaryInDb.Batch,
                            CartonRange = cartonRange,
                            Customer = "SCAN",
                            PurchaseOrder = poSummaryInDb.PurchaseOrder,
                            Style = "NA",
                            OrderType = "SCAN",
                            Color = "NA",
                            SizeBundle = "NA",
                            PcsBundle = "SCAN",
                            Container = poSummaryInDb.Container,
                            PcsPerCarton = 1,
                            POSummary = poSummaryInDb,
                            Vendor = poSummaryInDb.Vendor
                        });
                    }
                }
                else
                {
                    _registeredItemAdded += 1;

                    if (itemInList != null)
                    {
                        itemInList.PcsPerCarton += 1;
                    }
                    else
                    {
                        newItemList.Add(new RegularCartonDetail
                        {
                            UPCNumber = i.UPCNumber,
                            Receiver = "SCAN",
                            Batch = poSummaryInDb.Batch,
                            CartonRange = cartonRange,
                            Customer = "SCAN",
                            PurchaseOrder = poSummaryInDb.PurchaseOrder,
                            Style = refItem.Style,
                            Color = refItem.Color,
                            OrderType = "SCAN",
                            SizeBundle = refItem.Size,
                            Container = poSummaryInDb.Container,
                            PcsBundle = "SCAN",
                            PcsPerCarton = 1,
                            Vendor = poSummaryInDb.Vendor,
                            POSummary = poSummaryInDb
                        });
                    }
                }
            }
            return newItemList;
        }

        IList<RegularCartonDetail> MergePltItems(ApplicationDbContext context, POSummary poSummaryInDb, IList<RegularCartonDetail> newItemList, string cartonRange, string operation)
        {
            var regularCartionDetailsInDb = context.RegularCartonDetails
                .Include(x => x.POSummary)
                .Where(x => x.POSummary.Id == poSummaryInDb.Id);

            for(int i = 0; i < newItemList.Count(); i++)
            {
                var upc = newItemList[i].UPCNumber;
                var itemInDb = regularCartionDetailsInDb.SingleOrDefault(x => x.UPCNumber == upc && x.CartonRange == cartonRange);

                if (itemInDb != null)
                {
                    if (operation == "Add")
                    {
                        itemInDb.PcsPerCarton += newItemList[i].PcsPerCarton;
                        newItemList.Remove(newItemList[i]);

                    }
                    else
                    {
                        if (itemInDb.Style == "NA")
                            _unregisteredItemDeducted += newItemList[i].PcsPerCarton;
                        else
                            _registeredItemDeducted += newItemList[i].PcsPerCarton;

                        itemInDb.PcsPerCarton -= newItemList[i].PcsPerCarton;
                        newItemList.Remove(newItemList[i]);
                    }
                    i -= 1;
                }
                //没有找到已存在的item就不操作
                else { }
            }

            //foreach (var item in newItemList)
            //{
            //    var itemInDb = regularCartionDetailsInDb.SingleOrDefault(x => x.UPCNumber == item.UPCNumber);

            //    if (itemInDb != null)
            //    {
            //        if (operation == "Add")
            //        {
            //            itemInDb.PcsPerCarton += item.PcsPerCarton;
            //            newItemList.Remove(item);
            //        }
            //        else
            //        {
            //            if (itemInDb.Style == "NA")
            //                _unregisteredItemDeducted += item.PcsPerCarton;
            //            else
            //                _registeredItemDeducted += item.PcsPerCarton;

            //            itemInDb.PcsPerCarton -= item.PcsPerCarton;
            //            newItemList.Remove(item);
            //        }
            //    }
            //}

            return newItemList;
        }
    }

    public class UPCItem
    {
        public string UPCNumber { get; set; }
    }
}
