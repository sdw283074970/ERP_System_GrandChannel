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
    public class UploadBatchScanResultsController : ApiController
    {
        private ApplicationDbContext _context;
        private int _registeredItemAdded;
        private int _unregisteredItemAdded;
        private int _registeredItemDeducted;
        private int _unregisteredItemDeducted;
        private int _totalOrgPcs;

        public UploadBatchScanResultsController()
        {
            _context = new ApplicationDbContext();
            _registeredItemAdded = 0;
            _registeredItemDeducted = 0;
            _unregisteredItemAdded = 0;
            _unregisteredItemDeducted = 0;
        }

        // POST /api/uploadbatchScanResults/?batchId={batchId}&container={container}&operation={operation}
        [HttpPost]
        public IHttpActionResult CreateBatchSKUItems([FromUri]int batchId, [FromUri]string container, [FromUri]string operation, [FromBody]IEnumerable<UPCItem> items)
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
            var newItemList = GenerateNewItemList(poSummaryInDb, registeredSKU, items);

            if (operation == "Add")
            {
                var list = MergePltItems(_context, poSummaryInDb, newItemList, "Add");
                _context.RegularCartonDetails.AddRange(list);
                _context.SaveChanges();

                return Created(Request.RequestUri, "Successfully added " + _registeredItemAdded + " pcs registered item and " + _unregisteredItemAdded + " pcs unregistered item.");
            }
            else if (operation == "Minus")
            {
                var list = MergePltItems(_context, poSummaryInDb, newItemList, "Minus");
                _context.RegularCartonDetails.AddRange(list);
                _context.SaveChanges();

                return Created(Request.RequestUri, "Successfully deducted " + _registeredItemDeducted + " pcs unregistered item and " + _unregisteredItemDeducted + " pcs unregistered item." + 
                    (_totalOrgPcs - _registeredItemDeducted - _unregisteredItemDeducted) + " pcs item was not found in system.");

            }

            return Ok("No operation applied.");
        }

        IList<RegularCartonDetail> GenerateNewItemList(POSummary poSummaryInDb, IEnumerable<RegularSKURegistration> registeredSKU, IEnumerable<UPCItem> items)
        {
            var newItemList = new List<RegularCartonDetail>();

            foreach (var i in items)
            {
                var refItem = registeredSKU.FirstOrDefault(x => x.UPCNumber == i.UPCNumber);
                var itemInList = newItemList.SingleOrDefault(x => x.UPCNumber == i.UPCNumber);

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
                            CartonRange = poSummaryInDb.PoLine.ToString(),
                            Customer = "SCAN",
                            PurchaseOrder = poSummaryInDb.PurchaseOrder,
                            Style = "NA",
                            OrderType = "SCAN",
                            Color = "NA",
                            SizeBundle = "NA",
                            PcsBundle = "SCAN",
                            Container = poSummaryInDb.Container,
                            PcsPerCarton = 1,
                            POSummary = poSummaryInDb
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
                            CartonRange = poSummaryInDb.PoLine.ToString(),
                            Customer = "SCAN",
                            PurchaseOrder = poSummaryInDb.PurchaseOrder,
                            Style = refItem.Style,
                            Color = refItem.Color,
                            OrderType = "SCAN",
                            SizeBundle = refItem.Size,
                            Container = poSummaryInDb.Container,
                            PcsBundle = "SCAN",
                            PcsPerCarton = 1,
                            POSummary = poSummaryInDb
                        });
                    }
                }
            }
            return newItemList;
        }

        IList<RegularCartonDetail> MergePltItems(ApplicationDbContext context, POSummary poSummaryInDb, IList<RegularCartonDetail> newItemList, string operation)
        {
            var regularCartionDetailsInDb = context.RegularCartonDetails
                .Include(x => x.POSummary)
                .Where(x => x.POSummary.Id == poSummaryInDb.Id);

            for(int i = 0; i < newItemList.Count(); i++)
            {
                var upc = newItemList[i].UPCNumber;
                var itemInDb = regularCartionDetailsInDb.SingleOrDefault(x => x.UPCNumber == upc);

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
