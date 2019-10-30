using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.FBAModels.Interfaces;
using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAPickDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAPickDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbapickdetail/?shipOrderId={shipOrderId}&operation={Download}
        [HttpGet]
        public IHttpActionResult DownloadPickingList([FromUri]int shipOrderId, [FromUri]string operation)
        {
            var generator = new FBAExcelGenerator(@"D:\Template\PickingList-Template.xlsx");
            var path = generator.GenerateShippingWOAndPickingList(shipOrderId);

            return Ok(path);
        }

        // GET /api/fba/fbapickdetail/?shipOrderId={shipOrderId}
        [HttpGet]
        public IHttpActionResult GetPickDetail([FromUri]int shipOrderId)
        {
            var result = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBACartonLocation.FBAOrderDetail)
                .Include(x => x.FBAPickDetailCartons
                    .Select(c => c.FBACartonLocation.FBAOrderDetail))
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations
                    .Select(c => c.FBAOrderDetail))
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var resultDto = new List<FBAPickDetailsDto>();

            foreach(var r in result)
            {
                var dto = Mapper.Map<FBAPickDetail, FBAPickDetailsDto>(r);

                if (r.FBACartonLocation == null)
                {
                    dto.Barcode = r.FBAPickDetailCartons.Count == 1 
                        ? dto.Barcode = r.FBAPickDetailCartons.First().FBACartonLocation.FBAOrderDetail.Barcode 
                        : "MIX";

                    var cartonsDtoList = new List<FBACartonLocationDto>();

                    foreach (var c in r.FBAPickDetailCartons)
                    {
                        c.FBACartonLocation.PickingCtns = c.PickCtns;
                        cartonsDtoList.Add(Mapper.Map<FBACartonLocation, FBACartonLocationDto>(c.FBACartonLocation));
                    }

                    dto.FBACartonLocations = cartonsDtoList;
                }
                else
                {
                    dto.Barcode = r.FBACartonLocation.FBAOrderDetail.Barcode;
                }
                resultDto.Add(dto);
            }

            return Ok(resultDto);
        }

        // GET /api/FBAPickDetail/?pickDetailId={pickDetailId}
        [HttpGet]
        public IHttpActionResult GetPickedItemsInPallet([FromUri]int pickDetailId)
        {
            var pickDetailInDb = _context.FBAPickDetails
                .Include(x => x.FBAPickDetailCartons
                    .Select(c => c.FBACartonLocation.FBAOrderDetail))
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations
                    .Select(c => c.FBAOrderDetail))
                .SingleOrDefault(x => x.Id == pickDetailId);

            var resultDto = new List<FBACartonLocationDto>();

            foreach(var c in pickDetailInDb.FBAPickDetailCartons)
            {
                c.FBACartonLocation.PickingCtns = c.PickCtns;
                resultDto.Add(Mapper.Map<FBACartonLocation,FBACartonLocationDto>(c.FBACartonLocation));
            }

            return Ok(resultDto);
        }

        // POST /api/fba/fbapickdetail/?shipOrderId=?shipOrderId={shipOrderId}&orderType={orderType}
        [HttpPost]
        public IHttpActionResult CreatePickDetails([FromUri]int shipOrderId, [FromUri]string orderType, [FromBody]PickOrderDto obj)
        {
            var pickDetailList = new List<FBAPickDetail>();
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

            if (orderType == FBAOrderType.Standard)
            {
                var resultsInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAPallet.FBACartonLocations)
                    .Include(x => x.FBAMasterOrder)
                    .Include(x => x.FBAPallet.FBAPalletLocations)
                    .Where(x => x.AvailablePlts != 0);

                var objArray = new List<PickCartonDto>();

                if (obj.Container != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.Container == obj.Container);
                }

                if (obj.CustomerCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.GrandNumber.Contains(obj.CustomerCode));
                }

                if (obj.ShipmentId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.ShipmentId == obj.ShipmentId);
                }

                if (obj.AmzRefId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.AmzRefId == obj.AmzRefId);
                }

                if (obj.HowToDeliver != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.HowToDeliver == obj.HowToDeliver);
                }

                if (obj.WarehouseCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.WarehouseCode == obj.WarehouseCode);
                }

                foreach (var r in resultsInDb)
                {
                    objArray.Clear();

                    foreach (var carton in r.FBAPallet.FBACartonLocations)
                    {
                        objArray.Add(new PickCartonDto
                        {
                            Id = carton.Id,
                            Quantity = carton.AvailableCtns
                        });
                    }

                    pickDetailList.Add(CreateFBAPickDetailFromPalletLocation(r, shipOrderInDb, r.AvailablePlts, 0, pickDetailCartonList, objArray));
                }
            }
            else if (orderType == FBAOrderType.ECommerce)
            {
                var resultsInDb = _context.FBACartonLocations
                    .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                    .Where(x => x.AvailableCtns != 0);

                if (obj.Container != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.Container == obj.Container);
                }

                if (obj.CustomerCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.GrandNumber.Contains(obj.CustomerCode));
                }

                if (obj.ShipmentId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.ShipmentId == obj.ShipmentId);
                }

                if (obj.AmzRefId != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.AmzRefId == obj.AmzRefId);
                }

                if (obj.HowToDeliver != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.HowToDeliver == obj.HowToDeliver);
                }

                if (obj.WarehouseCode != "NULL")
                {
                    resultsInDb = resultsInDb.Where(x => x.WarehouseCode == obj.WarehouseCode);
                }

                foreach (var r in resultsInDb)
                {
                    pickDetailList.Add(CreateFBAPickDetailFromCartonLocation(r, shipOrderInDb, r.AvailableCtns));
                }
            }

            shipOrderInDb.Status = FBAStatus.Picking;

            _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
            _context.FBAPickDetails.AddRange(pickDetailList);
            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + pickDetailList.Count, Mapper.Map<IEnumerable<FBAPickDetail>, IEnumerable<FBAPickDetailsDto>>(pickDetailList));
        }

        // POST /api/fba/fbapickdetail/?shipOrderId={shipOrderId}&inventoryId={inventoryId}&inventoryType={inventoryType}&operation={operation}
        [HttpPost]
        public IHttpActionResult CreateSinglePickDetails([FromUri]int shipOrderId, [FromUri]int inventoryId, [FromUri]string inventoryType, [FromUri]string operation)
        {
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            if (operation == "AllPick")
            {
                var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

                if (inventoryType == FBAInventoryType.Pallet)
                {
                    var palletLocationInDb = _context.FBAPalletLocations
                        .Include(x => x.FBAPallet.FBACartonLocations)
                        .Include(x => x.FBAMasterOrder)
                        .Include(x => x.FBAPallet.FBAPalletLocations)
                        .SingleOrDefault(x => x.Id == inventoryId);

                    var objArray = new List<PickCartonDto>();

                    foreach (var carton in palletLocationInDb.FBAPallet.FBACartonLocations)
                    {
                        objArray.Add(new PickCartonDto {
                            Id = carton.Id,
                            Quantity = carton.AvailableCtns
                        });
                    }

                    _context.FBAPickDetails.Add(CreateFBAPickDetailFromPalletLocation(palletLocationInDb, shipOrderInDb, palletLocationInDb.AvailablePlts, 0, pickDetailCartonList, objArray));
                    _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
                }
                else
                {
                    var cartonLocationInDb = _context.FBACartonLocations
                        .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                        .SingleOrDefault(x => x.Id == inventoryId);

                    _context.FBAPickDetails.Add(CreateFBAPickDetailFromCartonLocation(cartonLocationInDb, shipOrderInDb, cartonLocationInDb.AvailableCtns));
                }

                shipOrderInDb.Status = FBAStatus.Picking;
            }
            _context.SaveChanges();

            return Created(Request.RequestUri + "/CreatedSuccess", "");
        }

        // POST /api/fba/fbapickdetail/?shipOrderId={shipOrderId}&quantity={quantity}&newQuantity={newQuantity}&inventoryType={inventoryType}
        [HttpPost]
        public IHttpActionResult CreateBatchPickDetail([FromUri]int shipOrderId, [FromUri]int inventoryLocationId, [FromUri]int quantity, [FromUri]int newQuantity, [FromUri]string inventoryType, [FromBody]IEnumerable<PickCartonDto> objArray)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            if (inventoryType == FBAInventoryType.Pallet)
            {
                var palletLocationInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAPallet.FBACartonLocations)
                    .Include(x => x.FBAMasterOrder)
                    .Include(x => x.FBAPallet.FBAPalletLocations)
                    .SingleOrDefault(x => x.Id == inventoryLocationId);

                _context.FBAPickDetails.Add(CreateFBAPickDetailFromPalletLocation(palletLocationInDb, shipOrderInDb, quantity, newQuantity, pickDetailCartonList, objArray));
                _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
            }
            else
            {
                var firstId = objArray.First().Id;
                var container = _context.FBACartonLocations.Find(firstId).Container;
                var cartonLocationsInDb = _context.FBACartonLocations
                    .Where(x => x.Container == container);
                var pickDetailList = new List<FBAPickDetail>();

                foreach(var obj in objArray)
                {
                    var cartonLocationInDb = cartonLocationsInDb
                        .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                        .SingleOrDefault(x => x.Id == obj.Id);

                    pickDetailList.Add(CreateFBAPickDetailFromCartonLocation(cartonLocationInDb, shipOrderInDb, obj.Quantity));
                }

                _context.FBAPickDetails.AddRange(pickDetailList);
            }

            shipOrderInDb.Status = FBAStatus.Picking;
            _context.SaveChanges();

            return Created(Request.RequestUri + "/CreatedSuccess", "");
        }

        // POST /api/fba/fbapickdetail/?shipOrderId={shipOderId}&operation={operation}
        [HttpPost]
        public IHttpActionResult UploadTemplate([FromUri]int shipOrderId, [FromUri]string operation)
        {
            var fileGetter = new FilesGetter();

            var path = fileGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (path == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var extracter = new FBAExcelExtracter(path);

            if (operation == FBAOperation.UploadBOL)
            {
                var bolDetailList = extracter.ExtractBOLTemplate();

                var generator = new PDFGenerator();

                var fileName = generator.GenerateFBABOL(shipOrderId, bolDetailList);

                return Ok(fileName);
            }
            else if (operation == FBAOperation.UploadShipOrder)
            {
                extracter.ExtractFBAPickingListTemplate(shipOrderId);

                return Ok("Create success!");
            }

            return Ok();
        }

        // POST /api/fba/fbapickdetail/?pickDetailId={pickDetailId}&pltsAdjust={pltsAdjust}&newPltsAdjust={newPltsAdjust}&outboundAdjust={outboundAdjust}
        [HttpPut]
        public void AdjustPickDetail([FromUri]int pickDetailId, [FromUri]int pltsAdjust, [FromUri]int newPltsAdjust, [FromUri]int outboundAdjust)
        {
            var pickDetailInDb = _context.FBAPickDetails
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPalletLocation.FBAPallet.FBAPalletLocations)
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .SingleOrDefault(x => x.Id == pickDetailId);

            //如果该对象中的pallet对像为空，则说明是从cartonLocation拣货，直接调整该拣货记录的outbound记录
            if (pickDetailInDb.FBAPalletLocation == null)
            {
                pickDetailInDb.NewPlts += newPltsAdjust;
                pickDetailInDb.ActualPlts += outboundAdjust;
                _context.SaveChanges();
                return;
            }

            //调整托盘数量小于已捡托盘数量的情况
            if (pltsAdjust + pickDetailInDb.PltsFromInventory < 0)
            {
                throw new Exception("There is no enough plts to adjust. You have picked " + pickDetailInDb.PltsFromInventory + " plts from inventory but you want to adjust " + pltsAdjust + " plts.");
            }

            //调整托盘数量小于库存可用托盘数量的情况
            if (pickDetailInDb.FBAPalletLocation.AvailablePlts - pltsAdjust < 0)
            {
                throw new Exception("There is no enough plts to adjust. There are only " + pickDetailInDb.FBAPalletLocation.AvailablePlts + " plts available right now but you want to adjust " + pltsAdjust + " plts.");
            }

            //调整新托盘数量小于已捡新托盘数量的情况
            if (pickDetailInDb.NewPlts + newPltsAdjust < 0)
            {
                throw new Exception("There is no enough plts to adjust. There are only " + pickDetailInDb.NewPlts + " new plts but you want to adjust " + newPltsAdjust + " plts.");
            }

            pickDetailInDb.PltsFromInventory += pltsAdjust;
            pickDetailInDb.NewPlts += newPltsAdjust;

            pickDetailInDb.FBAPalletLocation.AvailablePlts -= pltsAdjust;
            pickDetailInDb.FBAPalletLocation.PickingPlts += pltsAdjust;

            pickDetailInDb.ActualPlts += outboundAdjust;

            //如果捡完了托盘数量但是箱子还有剩余，则报错
            var availablePlts = pickDetailInDb.FBAPalletLocation.FBAPallet.FBAPalletLocations.Sum(x => x.AvailablePlts);
            var availableCtns = pickDetailInDb.FBAPalletLocation.FBAPallet.FBACartonLocations.Sum(x => x.AvailableCtns);
            if (availablePlts <= 0 && availableCtns != 0)
            {
                throw new Exception("Cannot adjust because the pallets number of SKU " + pickDetailInDb.FBAPalletLocation.ShipmentId + "will be 0 after this pick but there are still many cartons inside. Please make sure there is no thing left before picking the last pallte.");
            }

            _context.SaveChanges();
        }

        // DELETE /api/fba/fbapickdetail/?pickDetailId={pickDetailId}
        [HttpDelete]
        public void PutBackPickDetail([FromUri]int pickDetailId)
        {
            RemovePickDetail(_context, pickDetailId);
            _context.SaveChanges();
        }

        public void RemovePickDetail(ApplicationDbContext context, int pickDetailId)
        {
            var pickDetailInDb = context.FBAPickDetails
                .Include(x => x.FBACartonLocation)
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Include(x => x.FBAPickDetailCartons)
                .SingleOrDefault(x => x.Id == pickDetailId);

            //如果palletLocation不为空，则说明是standard类型运单内容，否则是ecommerce运单内容
            if (pickDetailInDb.FBAPalletLocation != null)
            {
                pickDetailInDb.FBAPalletLocation.AvailablePlts += pickDetailInDb.PltsFromInventory;
                pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.PltsFromInventory;
                if (pickDetailInDb.FBAPalletLocation.PickingPlts == 0 )
                {
                    pickDetailInDb.FBAPalletLocation.Status = FBAStatus.InStock;
                }

                var pickDetailCartonsInDb = context.FBAPickDetailCartons
                    .Include(x => x.FBACartonLocation)
                    .Include(x => x.FBAPickDetail)
                    .Where(x => x.FBAPickDetail.Id == pickDetailInDb.Id);

                foreach (var c in pickDetailCartonsInDb)
                {
                    c.FBACartonLocation.AvailableCtns += c.PickCtns;
                    c.FBACartonLocation.PickingCtns -= c.PickCtns;
                }

                context.FBAPickDetailCartons.RemoveRange(pickDetailCartonsInDb);
                context.FBAPickDetails.Remove(pickDetailInDb);
            }
            else if (pickDetailInDb.FBAPalletLocation == null)
            {
                pickDetailInDb.FBACartonLocation.AvailableCtns += pickDetailInDb.ActualQuantity;
                pickDetailInDb.FBACartonLocation.PickingCtns -= pickDetailInDb.ActualQuantity;
                if(pickDetailInDb.FBACartonLocation.PickingCtns == 0 && pickDetailInDb.FBACartonLocation.Location != "Pallet")
                {
                    pickDetailInDb.FBACartonLocation.Status = FBAStatus.InStock;
                }
                else if (pickDetailInDb.FBACartonLocation.PickingCtns == 0 && pickDetailInDb.FBACartonLocation.Location == "Pallet")
                {
                    pickDetailInDb.FBACartonLocation.Status = FBAStatus.InPallet;
                }
                context.FBAPickDetails.Remove(pickDetailInDb);
            }
        }

        private FBAPickDetail CreateFBAPickDetailFromPalletLocation(FBAPalletLocation fbaPalletLocationInDb, FBAShipOrder shipOrderInDb, int pltQuantity, int newPltQuantity, IList<FBAPickDetailCarton> pickDetailCartonList, IEnumerable<PickCartonDto> objArray)
        {
            var pickDetail = new FBAPickDetail();

            pickDetail.AssembleUniqueIndex(fbaPalletLocationInDb.Container, fbaPalletLocationInDb.GrandNumber);
            pickDetail.AssembleFirstStringPart(fbaPalletLocationInDb.ShipmentId, fbaPalletLocationInDb.AmzRefId, fbaPalletLocationInDb.WarehouseCode);
            pickDetail.AssembleActualDetails(0, 0, objArray.Sum(x => x.Quantity));

            pickDetail.Status = FBAStatus.Picking;
            pickDetail.Size = fbaPalletLocationInDb.PalletSize;
            pickDetail.PickableCtns = objArray.Sum(x => x.Quantity);
            pickDetail.NewPlts = newPltQuantity;
            pickDetail.PltsFromInventory = pltQuantity;
            //pickDetail.ActualPlts = pltQuantity + newPltQuantity;
            //现在强行规定，实际出库托盘数量为0，防止仓库偷懒不调整
            pickDetail.ActualPlts = 0;
            pickDetail.CtnsPerPlt = fbaPalletLocationInDb.CtnsPerPlt;
            pickDetail.Location = fbaPalletLocationInDb.Location;

            fbaPalletLocationInDb.PickingPlts += pltQuantity;
            //如果需要在库存中体现新打的托盘数量，禁用上面一行，启用下面一行
            //fbaPalletLocationInDb.PickingPlts += pltQuantity + newPltQuantity;

            fbaPalletLocationInDb.AvailablePlts -= pltQuantity;
            fbaPalletLocationInDb.Status = FBAStatus.Picking;

            pickDetail.HowToDeliver = fbaPalletLocationInDb.HowToDeliver;
            pickDetail.FBAPalletLocation = fbaPalletLocationInDb;
            pickDetail.OrderType = FBAOrderType.Standard;
            pickDetail.HowToDeliver = fbaPalletLocationInDb.HowToDeliver;
            pickDetail.InboundDate = fbaPalletLocationInDb.FBAMasterOrder.InboundDate;

            pickDetail.FBAShipOrder = shipOrderInDb;

            var cartonLocationInPalletsInDb = fbaPalletLocationInDb.FBAPallet.FBACartonLocations;

            foreach (var obj in objArray)
            {
                if (obj.Quantity == 0)
                {
                    continue;
                }

                var cartonInPalletInDb = cartonLocationInPalletsInDb.SingleOrDefault(x => x.Id == obj.Id);

                cartonInPalletInDb.PickingCtns += obj.Quantity;

                var pickDetailCarton = new FBAPickDetailCarton
                {
                    PickCtns = obj.Quantity,
                    FBAPickDetail = pickDetail,
                    FBACartonLocation = cartonInPalletInDb
                };

                cartonInPalletInDb.AvailableCtns -= obj.Quantity;
                pickDetail.ActualCBM += obj.Quantity * cartonInPalletInDb.CBMPerCtn;
                pickDetail.ActualGrossWeight += obj.Quantity * cartonInPalletInDb.GrossWeightPerCtn;

                pickDetailCartonList.Add(pickDetailCarton);

            }

            //如果捡完了托盘数量但是箱子还有剩余，则报错
            var availablePlts = fbaPalletLocationInDb.FBAPallet.FBAPalletLocations.Sum(x => x.AvailablePlts);
            var availableCtns = fbaPalletLocationInDb.FBAPallet.FBACartonLocations.Sum(x => x.AvailableCtns);
            if (availablePlts == 0 && availableCtns != 0)
            {
                throw new Exception("Cannot pick because the pallets number of SKU " + fbaPalletLocationInDb.ShipmentId + "will be 0 after this pick but there are still many cartons inside. Please make sure there is no thing left before picking the last pallte.");
            }

            return pickDetail;
        }

        private FBAPickDetail CreateFBAPickDetailFromCartonLocation(FBACartonLocation fbaCartonLocationInDb, FBAShipOrder shipOrderInDb, int ctnQuantity)
        {
            var pickDetail = new FBAPickDetail();

            pickDetail.AssembleUniqueIndex(fbaCartonLocationInDb.Container, fbaCartonLocationInDb.GrandNumber);
            pickDetail.AssembleFirstStringPart(fbaCartonLocationInDb.ShipmentId, fbaCartonLocationInDb.AmzRefId, fbaCartonLocationInDb.WarehouseCode);
            pickDetail.AssembleActualDetails(fbaCartonLocationInDb.GrossWeightPerCtn * ctnQuantity, fbaCartonLocationInDb.CBMPerCtn * ctnQuantity, ctnQuantity);

            pickDetail.Status = FBAStatus.Picking;
            pickDetail.Size = string.Empty;
            pickDetail.CtnsPerPlt = 0;
            pickDetail.PickableCtns = ctnQuantity;
            pickDetail.Location = fbaCartonLocationInDb.Location;
            pickDetail.InboundDate = fbaCartonLocationInDb.FBAOrderDetail.FBAMasterOrder.InboundDate;

            fbaCartonLocationInDb.PickingCtns += ctnQuantity;
            fbaCartonLocationInDb.AvailableCtns -= ctnQuantity;
            fbaCartonLocationInDb.Status = FBAStatus.Picking;

            pickDetail.FBACartonLocation = fbaCartonLocationInDb;
            pickDetail.OrderType = FBAOrderType.ECommerce;
            pickDetail.HowToDeliver = fbaCartonLocationInDb.HowToDeliver;

            pickDetail.FBAShipOrder = shipOrderInDb;

            return pickDetail;
        }
    }

    public class PickOrderDto
    {
        public string Container { get; set; }

        public string CustomerCode { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string HowToDeliver { get; set; }

        public string WarehouseCode { get; set; }
    }

    public class PickCartonDto
    {
        public int Id { get; set; }

        public int Quantity { get; set; }
    }
}
