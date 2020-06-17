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
using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models.FBAModels.BaseClass;
using ClothResorting.Models.FBAModels.StaticModels;
using System.Threading.Tasks;
using ClothResorting.Models.StaticClass;
using Newtonsoft.Json;
using System.IO;
using System.Web;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAOrderDetailController : ApiController
    {
        private ApplicationDbContext _context;
        private Logger _logger;

        public FBAOrderDetailController()
        {
            _context = new ApplicationDbContext();
            _logger = new Logger(_context);
        }

        // GET /api/fba/FBAOrderDetail/?grandNumber={grandNumber}&operation={operation}
        [HttpGet]
        public IHttpActionResult GetByOperation([FromUri]string grandNumber, [FromUri]string operation)
        {
            if (operation == FBAOperation.Download)
            {
                var masterOrderId = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber).Id;

                var generator = new FBAExcelGenerator(@"D:\Template\Receipt-template.xlsx");

                var fullPath = generator.GenerateReceipt(masterOrderId);

                return Ok(fullPath);
            }

            return Ok("Invalid operation.");

        }

        // GET /api/fba/FBAOrderDetail/?masterOrderId={masterOrderId}&operation={operation}
        [HttpGet]
        public IHttpActionResult GetByOperationById([FromUri]int masterOrderId, [FromUri]string operation)
        {
            if (operation == FBAOperation.Download)
            {
                var generator = new FBAExcelGenerator(@"D:\Template\Receipt-template.xlsx");

                var fullPath = generator.GenerateReceipt(masterOrderId);

                return Ok(fullPath);
            }

            return Ok("Invalid operation.");

        }

        // GET /api/fba/FBAOrderDetail/?grandNumber={grandNumber}
        [HttpGet]
        public IHttpActionResult GetFBAOrderDetailsByGrandNumber([FromUri]string grandNumber)
        {
            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBACartonLocations)
                .Where(x => x.GrandNumber == grandNumber);

            foreach (var o in orderDetailsInDb)
            {
                if (o.FBACartonLocations.Count != 0)
                    o.Status = "Locked";
                else
                    o.Status = "Open";
            }

            return Ok(Mapper.Map<IEnumerable<FBAOrderDetail>, IEnumerable<FBAOrderDetailDto>>(orderDetailsInDb));
        }

        // GET /api/fba/FBAOrderDetail/?masterOrderId={masterOrderId}
        [HttpGet]
        public IHttpActionResult GetFBAOrderDetailsByMasterOrderId([FromUri]int masterOrderId)
        {
            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBACartonLocations)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            foreach (var o in orderDetailsInDb)
            {
                if (o.FBACartonLocations.Count != 0)
                    o.Status = "Locked";
                else
                    o.Status = "Open";
            }

            return Ok(Mapper.Map<IEnumerable<FBAOrderDetail>, IEnumerable<FBAOrderDetailDto>>(orderDetailsInDb));
        }

        // GET /api/fba/FBAOrderDetail/?orderDetailId={orderDetailId}
        [HttpGet]
        public IHttpActionResult GetOrderDetail([FromUri]int orderDetailId)
        {
            return Ok(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>(_context.FBAOrderDetails.Find(orderDetailId)));
        }

        // GET /api/fba/FBAOrderDetail/?orderDetailId={orderDetailId}&operation={operation}
        [HttpGet]
        public IHttpActionResult GetOrderDetailLabelFileList([FromUri]int orderDetailId, [FromUri]string operation)
        {
            var orderDetailInDb = _context.FBAOrderDetails.SingleOrDefault(x => x.Id == orderDetailId);

            if (operation == "Labels")
            {
                var list = DeserializeLabelFilesString(orderDetailInDb.LabelFiles);

                return Ok(list);
            }

            return Ok("No operation applied.");
        }

        // POST /api/fbva/FBAOrderDetail/?masterOrderId={masterOrderId}&operation=Add
        [HttpPost]
        public async Task<IHttpActionResult> AddNewSKUByMasterOrderId([FromUri]int masterOrderId, [FromBody]ManifestItem item)
        {
            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);

            var newItem = new FBAOrderDetail
            {
                ShipmentId = item.ShipmentId,
                Barcode = item.Barcode,
                Container = masterOrderInDb.Container,
                AmzRefId = item.AmzRefId,
                WarehouseCode = item.WarehouseCode,
                HowToDeliver = item.Deliver,
                GrossWeight = item.GrossWeight,
                CBM = item.CBM,
                Quantity = item.Quantity,
                Remark = item.Remark,
                FBAMasterOrder = masterOrderInDb,
                GrandNumber = masterOrderInDb.GrandNumber
            };

            _context.FBAOrderDetails.Add(newItem);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>(_context.FBAOrderDetails.OrderByDescending(x => x.Id).First());

            await _logger.AddCreatedLogAsync<FBAOrderDetail>(null, resultDto, "Added a new manifest item", null, OperationLevel.Normal);

            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // POST /api/fbva/FBAOrderDetail/?grandNumber={grandNumber}&operation=Add
        [HttpPost]
        public async Task<IHttpActionResult> CreateNewManifestItem([FromUri]string grandNumber, [FromUri]string operation, [FromBody]ManifestItem item)
        {
            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber);

            var newItem = new FBAOrderDetail {
                ShipmentId = item.ShipmentId,
                Barcode = item.Barcode,
                Container = masterOrderInDb.Container,
                AmzRefId = item.AmzRefId,
                WarehouseCode = item.WarehouseCode,
                HowToDeliver = item.Deliver,
                GrossWeight = item.GrossWeight,
                CBM = item.CBM,
                Quantity = item.Quantity,
                Remark = item.Remark,
                FBAMasterOrder = masterOrderInDb,
                GrandNumber = grandNumber
            };

            _context.FBAOrderDetails.Add(newItem);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>(_context.FBAOrderDetails.OrderByDescending(x => x.Id).First());

            await _logger.AddCreatedLogAsync<FBAOrderDetail>(null, resultDto, "Added a new manifest item", null, OperationLevel.Normal);

            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // POST /api/fba/FBAOrderDetail/?grandNumber={grandNumber}
        [HttpPost]
        public void UploadFBATemplate([FromUri]string grandNumber)
        {
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            var fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new FBAExcelExtracter(fileSavePath);
            var killer = new ExcelKiller();

            excel.ExtractFBAPackingListTemplate(grandNumber, 0);
            killer.Dispose();
        }

        // POST /api/fba/FBAOrderDetail/?masterOrderId={masterOrderId}&operation={operation}
        [HttpPost]
        public IHttpActionResult UploadFBATemplateByMasterOrderId([FromUri]int masterOrderId, [FromUri]string operation)
        {
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            var fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new FBAExcelExtracter(fileSavePath);
            var killer = new ExcelKiller();
            var result = new List<FBAOrderDetail>();

            if (operation == "Upload")
            {
                try
                {
                    result = excel.ExtractFBAPackingListTemplate("", masterOrderId);
                }
                catch(Exception e)
                {
                    killer.Dispose();
                    throw new Exception(e.Message);
                }
            }

            killer.Dispose();

            return Ok(Mapper.Map<IList<FBAOrderDetail>, IList<FBAOrderDetailDto>>(result));
        }

        // POST /api/fba/FBAOrderDetail/?orderDetailIn={orderDetailId}
        [HttpPost]
        public IHttpActionResult UploadLabelFiles([FromUri]int orderDetailId)
        {
            // 将label文件反序列化，添加新的label文件，再序列化
            var orderDetailInDb = _context.FBAOrderDetails.Find(orderDetailId);
            var deList = DeserializeLabelFilesString(orderDetailInDb.LabelFiles);
            var list = new List<LabelFile>();
            list.AddRange(deList);

            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();
            var filePathList = filesGetter.GetAndSaveMultipleFileFromHttpRequest(@"D:\Labels\");

            if (filePathList.ToList().Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            foreach (var f in filePathList)
            {
                var labelFile = new LabelFile();
                labelFile.NameInSystem = f.Split('\\').Last();
                labelFile.OriginalName = labelFile.NameInSystem.Split('-').Last();
                labelFile.RootPath = f;
                labelFile.UploadDate = DateTime.Now;

                list.Add(labelFile);
            }

            orderDetailInDb.LabelFiles = SerializeLabelFiles(list);

            _context.SaveChanges();
            return Ok(new { OrderDetailId = orderDetailId, list.Count });
        }

        // PUT /api/fba/FBAOrderDetail/?orderDetailId={orderDetailId}?operation={operation}
        [HttpPut]
        public IHttpActionResult UpdateInfo([FromUri]int orderDetailId, [FromUri]string operation, [FromBody]FBAOrderDetail obj)
        {
            var orderDetailInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .SingleOrDefault(x => x.Id == orderDetailId);

            // TO DO: 等办公室不再用旧UI操作后，这里的else中部分可以简化
            if (operation == "Edit")
            {
                orderDetailInDb.ShipmentId = obj.ShipmentId;
                orderDetailInDb.AmzRefId = obj.AmzRefId;
                orderDetailInDb.WarehouseCode = obj.WarehouseCode;
                orderDetailInDb.HowToDeliver = obj.HowToDeliver;
                orderDetailInDb.GrossWeight = obj.GrossWeight;
                orderDetailInDb.Barcode = obj.Barcode;
                orderDetailInDb.CBM = obj.CBM;
                orderDetailInDb.Quantity = obj.Quantity;
                orderDetailInDb.Remark = obj.Remark;
            }
            else
            {
                //如果该主单还未被确认收货（没有收货日期），则禁止单个调节
                if (orderDetailInDb.FBAMasterOrder.InboundDate.Year == 1900)
                {
                    throw new Exception("Cannot update info because this master order is unreceived yet.");
                }

                //如果该detail被分配，则禁止更改实收数据
                if (orderDetailInDb.ComsumedQuantity != 0)
                {
                    throw new Exception("Cannot update info because this item has been comsumed.");
                }

                orderDetailInDb.ActualQuantity = obj.ActualQuantity;
                orderDetailInDb.ActualGrossWeight = obj.ActualGrossWeight;
                orderDetailInDb.ActualCBM = obj.ActualCBM;
                orderDetailInDb.Comment = obj.Comment;
                orderDetailInDb.Remark = obj.Remark;
            }

            _context.SaveChanges();
            return Ok(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>(orderDetailInDb));

        }

        // PUT /api/fba/FBAOrderDetail/?grandNumber={grandNumber}&inboundDate={inboundDate}&container={container}
        [HttpPut]
        public void UpdateReceiving([FromUri]string grandNumber, [FromUri]DateTime inboundDate, [FromUri]string container)
        {
            container = container.Trim();

            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.GrandNumber == grandNumber);

            var masterInDb = orderDetailsInDb.First().FBAMasterOrder;

            //如果已经分配库位了，就不能使用这个批量收货的功能
            if (_context.FBACartonLocations.Where(x => x.Container == masterInDb.Container).Count() > 0)
            {
                throw new Exception("Cannot using this batch receving function because there were some items allocated.");
            }

            //Container号查重
            var currentContainer = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber).Container;

            var isCurrentContainer = currentContainer == container ? true : false;
            var isExisted = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == container) == null ? false : true;

            if (isCurrentContainer || !isExisted)
            {
                foreach (var detail in orderDetailsInDb)
                {
                    if (container != null || container != "")
                    {
                        detail.Container = container;
                    }

                    if (detail.ActualQuantity == 0)
                    {
                        detail.ActualCBM = detail.CBM;
                        detail.ActualGrossWeight = detail.GrossWeight;
                        detail.ActualQuantity = detail.Quantity;
                    }
                }

                if (container != null || container != "")
                {
                    masterInDb.Container = container;
                }

                masterInDb.InboundDate = inboundDate;

                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Contianer Number " + container + " has been taken. Please delete the existed order and try agian.");
            }
        }

        // PUT /api/fba/FBAOrderDetail/?masterOrderId={masterOrderId}
        [HttpPut]
        public void UpdateReceivingWithoutReceivingDate([FromUri]int masterOrderId)
        {

            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            var masterInDb = orderDetailsInDb.First().FBAMasterOrder;

            //如果已经分配库位了，就不能使用这个批量收货的功能
            if (_context.FBACartonLocations.Where(x => x.Container == masterInDb.Container).Count() > 0)
            {
                throw new Exception("Cannot using this batch receving function because there were some items allocated.");
            }

            foreach (var detail in orderDetailsInDb)
            {
                if (detail.ActualQuantity == 0)
                {
                    detail.ActualCBM = detail.CBM;
                    detail.ActualGrossWeight = detail.GrossWeight;
                    detail.ActualQuantity = detail.Quantity;
                }
            }

            _context.SaveChanges();
        }

        // DELETE /api/fba/FBAOrderDetail/?orderDetailId={orderDetailId}
        [HttpDelete]
        public async Task RemoveOrderDetail([FromUri]int orderDetailId)
        {
            var orderDetailInDb = _context.FBAOrderDetails
                .SingleOrDefault(x => x.Id == orderDetailId);

            _context.FBAOrderDetails.Remove(orderDetailInDb);

            var dto = Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>(orderDetailInDb);
            await _logger.AddDeletedLogAsync<FBAOrderDetail>(dto, "Deleted an order detail(manifest item)", null, OperationLevel.Mediunm);

            _context.SaveChanges();
        }

        // DELETE /api/FBAOrderDetail/?orderDetailId={orDerDetailId}&fileName={fileName}
        [HttpDelete]
        public void DeleteLabelFile([FromUri]int orderDetailId, [FromUri]string fileName)
        {
            var orderDetailInDb = _context.FBAOrderDetails.Find(orderDetailId);

            var list = DeserializeLabelFilesString(orderDetailInDb.LabelFiles);
            list.Remove(list.SingleOrDefault(x => x.NameInSystem == fileName));
            orderDetailInDb.LabelFiles = SerializeLabelFiles(list);

            var filePath = Path.GetFullPath(@"D:\Labels\" + fileName);
            try
            {
                File.Delete(filePath);
            }
            catch(Exception e)
            {
                _context.SaveChanges();
                throw new Exception("File Doesn't exist in the server");
            }

            _context.SaveChanges();
        }

        private List<LabelFile> DeserializeLabelFilesString(string js)
        {
            if (js == null)
                js = "[]";

            return JsonConvert.DeserializeObject<List<LabelFile>>(js);
        }

        private string SerializeLabelFiles(List<LabelFile> list)
        {
            if (list == null)
                return "[]";

            return JsonConvert.SerializeObject(list);

        }
    }

    public class ManifestItem
    {
        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string Barcode { get; set; }

        public string WarehouseCode { get; set; }

        public string Deliver { get; set; }

        public float GrossWeight { get; set; }

        public float CBM { get; set; }

        public int Quantity { get; set; }

        public string Remark { get; set; }
    }
}
