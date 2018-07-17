using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using _Excel = Microsoft.Office.Interop.Excel;
using System.Collections;
using ClothResorting.Models;
using System.Diagnostics;
using System.Data.Entity;
using System.Web.Http;
using System.Net;

namespace ClothResorting.Helpers
{
    public class ExcelExtracter
    {
        //全局变量
        #region
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime _dateTimeNow;
        #endregion

        //PackingList全局变量
        #region
        private string _purchaseOrder;
        private string _styleNumber;
        private int _countOfPo;
        private double _packedCartons;
        private double _netWeight;
        private double _grossWeight;
        private double? _numberOfDemension;
        private double? _numberOfSizeRatio;
        #endregion

        //CartonDetail全局变量
        #region
        private string _style;
        private string _color;
        private double _cartonNumberRangeFrom;
        private double _cartonNumberRangeTo;
        private double _sumOfCarton;
        private string _runCode;
        private string _dimension;
        private double? _grossWeightPerCartons;
        private double? _netWeightPerCartons;
        private double _cFT;
        private double? _pcsPerCartons;
        private double? _totalPcs;
        #endregion

        //构造器
        public ExcelExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _dateTimeNow = DateTime.Now;
            _excel = new _Excel.Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //建立一个Pre-Recieve Order对象并添加进数据库
        #region
        public void CreateSILKICONPreReceiveOrder()
        {
            _ws = _wb.Worksheets[1];

            //建立一个PreReceiveOrder对象
            var newOrder = new PreReceiveOrder
            {
                ActualReceivedCtns = 0,
                CustomerName = _ws.Cells[1, 2].Value2,
                CreatDate = _dateTimeNow,
                TotalCartons = (int)_ws.Cells[3, 2].Value2,
                TotalGrossWeight = Math.Round(_ws.Cells[5, 2].Value2 * 2.205, 2),
                TotalNetWeight = Math.Round(_ws.Cells[6, 2].Value2 * 2.205, 2),
                TotalVol = Math.Round(_ws.Cells[4, 2].Value2 * 35.315, 2),
                ContainerNumber = "UNKOWN",
                TotalPcs = 0,
                ActualReceivedPcs = 0,
                Status = "New Created"
            };

            _context.PreReceiveOrders.Add(newOrder);
            _context.SaveChanges();
        }
        #endregion

        //扫描并抽取每一页的Carton信息概览
        #region
        public void ExtractSIPurchaseOrderSummary()
        {
            var list = new List<PurchaseOrderSummary>();
            var preReceiveOrderInDb = _context.PreReceiveOrders     //获取刚建立的PreReceiveOrder
                .OrderByDescending(c => c.Id)
                .First();

            //开始扫描每一页
            for (int k = 2; k <= _wb.Worksheets.Count; k++)
            {
                var demensions = new List<Measurement>();
                int n = 11;
                int t;

                _ws = _wb.Worksheets[k];
                _purchaseOrder = _ws.Cells[1, 2].Value2.ToString();
                _styleNumber = _ws.Cells[2, 2].Value2.ToString();
                _packedCartons = _ws.Cells[3, 2].Value2;
                _cFT = _ws.Cells[4, 2].Value2 == null ? 0 : _ws.Cells[4, 2].Value2;
                _grossWeight = _ws.Cells[5, 2].Value2 == null ? 0 : _ws.Cells[5, 2].Value2;
                _netWeight = _ws.Cells[6, 2].Value2 == null ? 0 : _ws.Cells[6, 2].Value2;
                _numberOfDemension = _ws.Cells[1, 5].Value2;
                _numberOfSizeRatio = _ws.Cells[2, 5].Value2;

                //定位Total measurements
                while (_ws.Cells[n, 1].Value2 != null)
                {
                    n += 1;
                }

                t = n + 2;      //t为Total measurements开始的行数

                //扫描Total measurements并添加进列表
                for (int d = 0; d < _numberOfDemension; d++)
                {
                    demensions.Add(new Measurement {
                        Record = _ws.Cells[t + d, 1].Value2,
                        PurchaseOrder = _purchaseOrder.ToString()
                    });
                }

                list.Add(new PurchaseOrderSummary
                {
                    Available = 0,
                    ActualReceived = 0,
                    PurchaseOrder = _purchaseOrder.ToString(),
                    Style = _styleNumber,
                    NetWeight = Math.Round(_netWeight * 2.205, 2),
                    GrossWeight = Math.Round(_grossWeight * 2.205, 2),
                    CFT = Math.Round(_cFT * 35.315, 2),
                    ReceivedDate = null,
                    NumberOfSizeRatio = (int)_numberOfSizeRatio,
                    PackedCartons = (int)_packedCartons,
                    NumberOfDemension = (int)_numberOfDemension,
                    TotalMeasurements = demensions == null ? null : demensions,
                    PreReceiveOrder = preReceiveOrderInDb,
                    TotalPcs = 0,
                    ActualReceivedPcs = 0,
                    AvailablePcs = 0,
                    OrderType = "Replenishment"
                });
            }

            _context.PurchaseOrderSummaries.AddRange(list);
            _context.SaveChanges();
        }
        #endregion

        //抽取SilkIcon公司发来的Excel表格中的CartonDetails
        #region
        public void ExtractCartonDetails()
        {
            var wbCount = _wb.Worksheets.Count;

            //扫描每一张Sheet
            for (int w = 2; w <= wbCount; w++)
            {
                var list = new List<CartonDetail>();
                var cartonBreakDownList = new List<CartonBreakDown>();
                var cartonClassCount = 0;
                int i = 11;
                int j = 3;
                _ws = _wb.Worksheets[w];

                var preReceiveOrderInDb = _context.PreReceiveOrders     //获取刚建立的PreReceiveOrder
                    .OrderByDescending(c => c.Id)
                    .First();

                //确定有多少行Carton需要扫描
                while (_ws.Cells[i, 6].Value2 != null)
                {
                    i += 1;
                    cartonClassCount += 1;
                }

                i = 11;

                //确定SizeRatio的数量
                _numberOfSizeRatio = (int)_ws.Cells[2, 5].Value2;

                //找到与这一页CartonDetail相关的PackingList
                _purchaseOrder = _ws.Cells[1, 2].Value2.ToString();
                var plInDb = _context.PurchaseOrderSummaries.Include(s => s.PreReceiveOrder)
                    .SingleOrDefault(s => s.PurchaseOrder == _purchaseOrder
                        && s.PreReceiveOrder.Id == preReceiveOrderInDb.Id);

                //为每一个CartonDetail扫描数据
                for (int c = 0; c < cartonClassCount; c++)
                {
                    var sizeList = new List<SizeRatio>();

                    for (int n = 0; n < _numberOfSizeRatio; n++)
                    {
                        if (_ws.Cells[i, 18 + n].Value2 != null && _ws.Cells[i, 18 + n].Value2 != 0)
                        {
                            sizeList.Add(new SizeRatio
                            {
                                Count = (int)_ws.Cells[i, 18 + n].Value2,
                                SizeName = _ws.Cells[10, 18 + n].Value2
                            });
                        }
                    }

                    //读取扫描结果
                    _style = _ws.Cells[i, j - 2].Value2.ToString();
                    _color = _ws.Cells[i, j - 1].Value2;
                    //箱号范围有两种格式，即分开的和通过'-'合并的，分别获取箱号范围值
                    _cartonNumberRangeFrom = _ws.Cells[i, j].Value2 == null ? GetFrom(_ws.Cells[i, j + 1].Value2) : _ws.Cells[i, j].Value2;
                    _cartonNumberRangeTo = _ws.Cells[i, j + 2].Value2 == null ? GetTo(_ws.Cells[i, j + 1].Value2) : _ws.Cells[i, j + 2].Value2;
                    _sumOfCarton = _ws.Cells[i, j + 3].Value2;
                    _runCode = _ws.Cells[i, j + 5].Value2;
                    _dimension = _ws.Cells[i, j + 7].Value2;
                    _netWeightPerCartons = _ws.Cells[i, j + 10].Value2;
                    _grossWeightPerCartons = _ws.Cells[i, j + 11].Value2;
                    _pcsPerCartons = _ws.Cells[i, j + 12].Value2;
                    _totalPcs = _ws.Cells[i, j + 13].Value2;
                    _style = _ws.Cells[i, j - 2].Value2.ToString();

                    var carton = new CartonDetail
                    {
                        ActualReceivedPcs = 0,
                        AvailablePcs = 0,
                        Available = 0,
                        ActualReceived = 0,
                        Style = _style,
                        Color = _color,
                        PurchaseOrder = _purchaseOrder.ToString(),
                        CartonNumberRangeFrom = (int)_cartonNumberRangeFrom,
                        CartonNumberRangeTo = (int)_cartonNumberRangeTo,
                        SumOfCarton = (int)_sumOfCarton,
                        RunCode = _runCode == null ? "" : _runCode,
                        Dimension = _dimension == null ? "" : _dimension,
                        NetWeightPerCartons = _netWeightPerCartons == null ? 0 : Math.Round((double)_netWeightPerCartons * 2.205, 2),
                        GrossWeightPerCartons = _grossWeightPerCartons == null ? 0 : Math.Round((double)_grossWeightPerCartons * 2.205, 2),
                        PcsPerCartons = _pcsPerCartons == null ? 0 : (int)_pcsPerCartons,
                        TotalPcs = _totalPcs == null ? 0 : (int)_totalPcs,
                        PurchaseOrderSummary = plInDb,
                        SizeRatios = sizeList,
                        Location = "N/A"
                    };

                    //顺便添加CartonBreakDown信息到CartonBreakDown表中
                    var sumOfCartons = (int)_sumOfCarton;

                    for (int s = 0; s < sizeList.Count; s++)
                    {
                        //即使数量是0也要记录，以防一个箱子中塞入额外不同尺寸pcs的情况
                        var cartonBreakDown = new CartonBreakDown
                        {
                            PurchaseOrder = _purchaseOrder.ToString(),
                            Style = _style,
                            Color = _color,
                            CartonNumberRangeFrom = (int)_cartonNumberRangeFrom,
                            CartonNumberRangeTo = (int)_cartonNumberRangeTo,
                            RunCode = _runCode == null ? "" : _runCode,
                            Size = sizeList[s].SizeName,
                            ForecastPcs = sizeList[s].Count * sumOfCartons,
                            PcsPerCartons = sizeList[s].Count,
                            ActualPcs = 0,
                            AvailablePcs = 0,
                            PurchaseOrderSummary = plInDb,
                            Location = "N/A",
                            CartonDetail = carton
                        };
                        cartonBreakDownList.Add(cartonBreakDown);
                    }

                    list.Add(carton);
                    i += 1;
                }

                _context.CartonBreakDowns.AddRange(cartonBreakDownList);
                _context.CartonDetails.AddRange(list);
                _context.SaveChanges();
            }
        }
        #endregion

        //从类似"12-25"字符串中获取箱号范围的前段
        public int GetFrom(string cn)
        {
            string[] arr;
            if(cn.Contains('-'))
            {
                arr = cn.Split('-');
                return int.Parse(arr[0]);
            }
            else
            {
                return int.Parse(cn);
            }
        }

        //获取后段
        public int GetTo(string cn)
        {
            string[] arr;
            if (cn.Contains('-'))
            {
                arr = cn.Split('-');
                return int.Parse(arr[1]);
            }
            else
            {
                return int.Parse(cn);
            }
        }

        public void Dispose()
        {
            var excelProcs = Process.GetProcessesByName("EXCEL");

            foreach (var procs in excelProcs)
            {
                procs.Kill();
            }
        }

        //以CartonDetail为单位，抽取批量散货的excel信息(与packinglist无关，仅散货)
        #region
        public void ExtractBulkloadRecord()
        {
            var numberOfWorkSheet = _wb.Worksheets.Count;

            //遍历每一张ws
            for(int i = 1; i <= numberOfWorkSheet; i++)
            {
                var n = 3;
                var cartonBreakDownList = new List<CartonBreakDown>();
                var list = new List<CartonDetail>();
                var cartonClassCount = 0;

                _ws = _wb.Worksheets[i];
                _purchaseOrder = _ws.Cells[1, 2].Value2 == null ? "" : _ws.Cells[1, 2].Value2.ToString();
                _numberOfSizeRatio = _ws.Cells[1, 4].Value2;

                //数有多少carton储存对象(cartonDetail)
                while (_ws.Cells[n, 3].Value2 != null)
                {
                    cartonClassCount += 1;
                    n += 1;
                }

                //将ws中的每一个cartonDetail对象添加到代添加到数据库的列表中
                for (int k = 0; k < cartonClassCount; k++)
                {
                    //获取SizeRaio表
                    var sizeList = new List<SizeRatio>();

                    //扫描当前cartonDetail包含的sizeRatio
                    for (int p = 0; p < _numberOfSizeRatio; p++)
                    {
                        if (_ws.Cells[k + 3, 5 + p].Value2 != null && _ws.Cells[k + 3, 5 + p].Value2 != 0)
                        {
                            sizeList.Add(new SizeRatio
                            {
                                Count = (int)_ws.Cells[k + 3, 5 + p].Value2,
                                SizeName = _ws.Cells[2, 5 + p].Value2
                            });
                        }
                    }

                    //将ws中的关键变量储存至内存中
                    _style = _ws.Cells[k + 3, 1].Value2.ToString();
                    _color = _ws.Cells[k + 3, 2].Value2.ToString();
                    int numberOfCartons = (int)_ws.Cells[k + 3, 3].Value2;
                    var location = _ws.Cells[k + 3, 4].Value2 == null ? "N/A" : _ws.Cells[k + 3, 4].Value2.ToString();

                    //新建CartonDetail对象，将其添加到list中
                    var carton = new CartonDetail
                    {
                        PurchaseOrder = _purchaseOrder,
                        Style = _style,
                        Color = _color,
                        SumOfCarton = 0,
                        ActualReceived = numberOfCartons,
                        Available = numberOfCartons,
                        Location = location,
                        ActualReceivedPcs = 0,
                        AvailablePcs = 0,
                        SizeRatios = sizeList,
                        ReceivedDate = _dateTimeNow
                    };

                    //遍历sizeRatios，生成数个cartonBreakdown
                    for (int s = 0; s < sizeList.Count; s++)
                    {
                        var cartonBreakDown = new CartonBreakDown
                        {
                            PurchaseOrder = _purchaseOrder.ToString(),
                            Style = _style,
                            Color = _color,
                            CartonNumberRangeFrom = 0,
                            CartonNumberRangeTo = 0,
                            RunCode = "",
                            Size = sizeList[s].SizeName,
                            ForecastPcs = 0,
                            PcsPerCartons = sizeList[s].Count,
                            ActualPcs = sizeList[s].Count,
                            AvailablePcs = 0,
                            Location = location,
                            CartonDetail = carton,
                            ReceivedDate = _dateTimeNow
                        };

                        carton.ActualReceivedPcs = sizeList[s].Count;
                        carton.AvailablePcs = sizeList[s].Count;
                        cartonBreakDownList.Add(cartonBreakDown);
                    }

                    list.Add(carton);
                    i += 1;
                }

                //写进数据库
                _context.CartonBreakDowns.AddRange(cartonBreakDownList);
                _context.CartonDetails.AddRange(list);
                _context.SaveChanges();

                //释放EXCEL资源
                Dispose();
            }
        }
        #endregion

        //以LocationDetail为单位，从入库报告中抽取信息(与PackingList无关联，与整个库存的PO对象有关联)
        #region
        public void ExtractReplenishimentLocationDetail(string po)
        {
            int n = 3;
            int countOfObj = 0;
            var locationDetailList = new List<LocationDetail>();

            _ws = _wb.Worksheets[1];
            _purchaseOrder = _ws.Cells[1, 2] == null? "" : _ws.Cells[1, 2].Value2.ToString();

            var purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
                .SingleOrDefault(c => c.PurchaseOrder == _purchaseOrder);


            if (_purchaseOrder != po)
            {
                Dispose();
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            //获取数据库中所有的speciesInventory记录，用于判断入库报告中是否有新种类入库
            var species = _context.SpeciesInventories.Where(c => c.Id > 0).ToList();
            //临时表储存新加入的speciesInventory，用于避免在循环中多次查询数据库，以提高效率
            var speciesList = new List<SpeciesInventory>();

            while (_ws.Cells[n, 3].Value2 != null)
            {
                countOfObj += 1;
                n += 1;
            }

            for(int i = 0; i < countOfObj; i++)
            {
                var locationDetail = new LocationDetail
                {
                    PurchaseOrderInventory = purchaseOrderInventoryInDb,
                    PurchaseOrder = _purchaseOrder,
                    Style = _ws.Cells[3 + i, 1].Value2.ToString(),
                    Color = _ws.Cells[3 + i, 2].Value2.ToString(),
                    Size = _ws.Cells[3 + i, 3].Value2.ToString(),
                    OrgNumberOfCartons = (int)_ws.Cells[3 + i, 4].Value2(),
                    InvNumberOfCartons = (int)_ws.Cells[3 + i, 4].Value2(),
                    OrgPcs = (int)_ws.Cells[3 + i, 5].Value2(),
                    InvPcs = (int)_ws.Cells[3 + i, 5].Value2(),
                    Location = _ws.Cells[3 + i, 6].Value2(),
                    InboundDate = _dateTimeNow
                };

                locationDetailList.Add(locationDetail);

                //判断入库的对象是否是新种类，如果临时List和数据库species中都没有则说明是新种类，则在SpeciesInventories表中添加该类
                if (species.SingleOrDefault(c => c.PurchaseOrder == locationDetail.PurchaseOrder 
                    && c.Style == locationDetail.Style
                    && c.Color == locationDetail.Color
                    && c.Size == locationDetail.Size) == null && speciesList.SingleOrDefault(c => c.PurchaseOrder == locationDetail.PurchaseOrder
                    && c.Style == locationDetail.Style
                    && c.Color == locationDetail.Color
                    && c.Size == locationDetail.Size) == null)
                {
                    speciesList.Add(new SpeciesInventory {
                        PurchaseOrder = locationDetail.PurchaseOrder,
                        Style = locationDetail.Style,
                        Color = locationDetail.Color,
                        Size = locationDetail.Size,
                        OrgPcs = 0,
                        AdjPcs = 0,
                        InvPcs = 0,
                        PurchaseOrderInventory = purchaseOrderInventoryInDb
                    });
                }
            }
            
            _context.LocationDetails.AddRange(locationDetailList);
            _context.SpeciesInventories.AddRange(speciesList);
            _context.SaveChanges();

            //从入库报告中同步pcs数量到speciesInventory的原始数量、调整数量和库存数量中
            var speciesInventoryInDb = _context.SpeciesInventories.Where(c => c.Id > 0);
            foreach(var locationDetail in locationDetailList)
            {
                //此处不使用sync来同步统计是因为在循环中使用sync会多次读写数据库，降低运行效率
                speciesInventoryInDb.SingleOrDefault(c => c.PurchaseOrder == locationDetail.PurchaseOrder
                    && c.Style == locationDetail.Style
                    && c.Color == locationDetail.Color
                    && c.Size == locationDetail.Size)
                    .OrgPcs += locationDetail.OrgPcs;

                speciesInventoryInDb.SingleOrDefault(c => c.PurchaseOrder == locationDetail.PurchaseOrder
                    && c.Style == locationDetail.Style
                    && c.Color == locationDetail.Color
                    && c.Size == locationDetail.Size)
                    .AdjPcs += locationDetail.OrgPcs;

                speciesInventoryInDb.SingleOrDefault(c => c.PurchaseOrder == locationDetail.PurchaseOrder
                    && c.Style == locationDetail.Style
                    && c.Color == locationDetail.Color
                    && c.Size == locationDetail.Size)
                    .InvPcs += locationDetail.OrgPcs;
            }

            _context.SaveChanges();
        }
        #endregion

        //以RegularLocationDetail为单位，从入库报告中抽取信息(与PackingList无关联), 暂时没用
        public void ExtractRegularLocationDetail(string po)
        {
            int n = 3;
            int countOfObj = 0;
            var locationDetailList = new List<RegularLocationDetail>();

            _ws = _wb.Worksheets[1];
            _purchaseOrder = _ws.Cells[1, 2] == null ? "" : _ws.Cells[1, 2].Value2.ToString();

            var purchaseOrderInDb = _context.PurchaseOrderInventories
                .SingleOrDefault(c => c.PurchaseOrder == _purchaseOrder);

            if (_purchaseOrder != po)
            {
                Dispose();
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            while (_ws.Cells[n, 3].Value2 != null)
            {
                countOfObj += 1;
                n += 1;
            }

            for (int i = 0; i < countOfObj; i++)
            {
                locationDetailList.Add(new RegularLocationDetail
                {
                    PurchaseOrderInventory = purchaseOrderInDb,
                    PurchaseOrder = _purchaseOrder,
                    Style = _ws.Cells[3 + i, 1].Value2.ToString(),
                    Color = _ws.Cells[3 + i, 2].Value2.ToString(),
                    RunCode = _ws.Cells[3 + i, 3].Value2.ToString(),
                    OrgNumberOfCartons = (int)_ws.Cells[3 + i, 4].Value2(),
                    InvNumberOfCartons = (int)_ws.Cells[3 + i, 4].Value2(),
                    OrgPcs = (int)_ws.Cells[3 + i, 5].Value2(),
                    InvPcs = (int)_ws.Cells[3 + i, 5].Value2(),
                    Location = _ws.Cells[3 + i, 6].Value2(),
                    InboundDate = _dateTimeNow
                });
            }

            _context.RegularLocationDetails.AddRange(locationDetailList);
            _context.SaveChanges();

            Dispose();
        }

        //新建FreeCountry的预收货订单
        public void CreateFCPreReceiveOrder()
        {
            _context.PreReceiveOrders.Add(new PreReceiveOrder
            {
                CustomerName = "Free Country",
                CreatDate = _dateTimeNow,
                ContainerNumber = "UNKONOWN",
                TotalCartons = 0,
                ActualReceivedCtns = 0,
                TotalPcs = 0,
                ActualReceivedPcs = 0,
                Status = "New Created",
                TotalGrossWeight = 0,
                TotalNetWeight = 0,
                TotalVol = 0,
            });

            _context.SaveChanges();
        }

        //抽取excel文件中的PO信息，并与之前新建的FC预收订单关联
        public void ExtractFCPurchaseOrderSummary()
        {
            _ws = _wb.Worksheets[1];
            var packingList = new List<POSummary>();
            var index = 2;
            var latestPreReceiveOrder = _context.PreReceiveOrders.OrderByDescending(c => c.Id).FirstOrDefault();
            _countOfPo = 0;

            while (index > 0)
            {
                if (_ws.Cells[index, 1].Value2 != null)
                {
                    _countOfPo += 1;
                }

                if (_ws.Cells[index + 1, 1].Value2 == null && _ws.Cells[index + 2, 1].Value2 == null)
                {
                    break;
                }

                index += 1;
            }

            index = 2;

            for(int i = 0; i < _countOfPo; i++)
            {
                packingList.Add(new POSummary {
                    PurchaseOrder = _ws.Cells[index, 1].Value2.ToString(),
                    Style = _ws.Cells[index, 2].Value2.ToString(),
                    PoLine = (int)_ws.Cells[index, 3].Value2,
                    Customer = _ws.Cells[index, 5].Value2.ToString(),
                    Quantity = (int)_ws.Cells[index, 6].Value2,
                    Cartons = (int)_ws.Cells[index, 8].Value2,
                    GrossWeight = 0,
                    NetWeight = 0,
                    NNetWeight = 0,
                    CBM = 0,
                    ActualCtns = 0,
                    ActualPcs = 0,
                    Container = "Unkown",
                    PreReceiveOrder = latestPreReceiveOrder
                });

                index += 2;
            }

            //可以在本页面获取packingList的总量
            latestPreReceiveOrder.TotalCartons = (int) _ws.Cells[_countOfPo * 2 + 1, 8].Value2;
            latestPreReceiveOrder.TotalPcs = (int)_ws.Cells[_countOfPo * 2 + 1, 6].Value2;

            _context.POSummaries.AddRange(packingList);
            _context.SaveChanges();
        }

        //抽取Detail中的各个PO详细信息
        public void ExtractFCPurchaseOrderDetail()
        {
            _ws = _wb.Worksheets[2];
            var rowIndex = 1;
            var preReceiveOrderId = _context.PreReceiveOrders.OrderByDescending(c => c.Id).First().Id;
            var regularCartonDetailList = new List<RegularCartonDetail>();

            //扫描Detail页面中有多少个RegularCartonDetal对象
            for (int i = 0; i < _countOfPo; i++)
            {
                var countOfSpace = 0;
                var countOfRow = 0;
                var countOfColumn = 4;      //FC的装箱单第4列不知为何未空，必须从第五列开始计数
                var startIndex = rowIndex;      //rowIndex会变化，startIndex是不变的
                var columnIndex = 5;
                var poLine = (int)_ws.Cells[startIndex + 1, 3].Value2;
                string purchaseOrder = _ws.Cells[startIndex + 1, 1].Value2.ToString();

                var poSummaryInDb = _context.POSummaries
                    .Include(c => c.PreReceiveOrder)
                    .Where(c => c.PurchaseOrder == purchaseOrder 
                        && c.PreReceiveOrder.Id == preReceiveOrderId
                        && c.PoLine == poLine)
                    .First();

                //扫描该PoSummary对象占多少行
                while (_ws.Cells[rowIndex, 1].Value2 != null)
                {
                    countOfRow += 1;
                    rowIndex += 1;
                }

                //扫描该PoSummary对象占多少列
                while (_ws.Cells[startIndex + 2, columnIndex].Value2 != null)
                {
                    countOfColumn += 1;
                    columnIndex += 1;
                }

                //扫描该PoSummary的所有SKU(RegularCartonDetail数量)
                var countOfSKU = countOfRow - 4;
                var countOfSize = countOfColumn - 13;

                for (int j = 0; j < countOfSKU; j++)
                {
                    //扫描每一种SKU有多少种Size及数量
                    var sizeList = new List<SizeRatio>();

                    for (int k = 0; k < countOfSize; k++)
                    {
                        sizeList.Add(new SizeRatio
                        {
                            SizeName = _ws.Cells[startIndex + 2, 12 + k].Value2.ToString(),
                            Count = (int)_ws.Cells[startIndex + 3 + j, 12 + k].Value2
                        });
                    }

                    var sizeBundle = "";
                    var pcsBundle = "";

                    //统计当前SKU中的size组合
                    foreach(var size in sizeList)
                    {
                        sizeBundle += size.SizeName + " ";
                        pcsBundle += size.Count.ToString() + " ";
                    }

                    regularCartonDetailList.Add(new RegularCartonDetail {
                        CartonRange = _ws.Cells[startIndex + 3 + j, 1].Value2.ToString(),
                        PurchaseOrder = _ws.Cells[startIndex + 3 + j, 2].Value2.ToString(),
                        Style = _ws.Cells[startIndex + 3 + j, 3].Value2.ToString(),
                        Customer = _ws.Cells[startIndex + 3 + j, 4].Value2.ToString(),
                        Dimension = _ws.Cells[startIndex + 3 + j, 5].Value2.ToString(),
                        GrossWeight = 0,
                        NetWeight = 0,
                        Color = _ws.Cells[startIndex + 3 + j, 10].Value2.ToString(),
                        Cartons = (int)_ws.Cells[startIndex + 3 + j, 11].Value2,
                        SizeBundle = sizeBundle,
                        PcsBundle = pcsBundle,
                        PcsPerCarton = (int)_ws.Cells[startIndex + 3 + j, countOfColumn - 1].Value2,
                        Quantity = (int)_ws.Cells[startIndex + 3 + j, countOfColumn].Value2,
                        ActualCtns = 0,
                        ActualPcs = 0,
                        POSummary = poSummaryInDb
                    });
                }
                
                //扫描该PoSummary对象与下一个对象之间的间隔行数
                while (_ws.Cells[rowIndex, 1].Value2 == null && countOfSpace <= 5)
                {
                    countOfSpace += 1;
                    rowIndex += 1;
                }
            }
            _context.RegularCartonDetails.AddRange(regularCartonDetailList);
            _context.SaveChanges();
        }

        // 抽取FreeCountry正常订单的库存分配信息
        public void ExtractFCRegularLocation(int preid)
        {
            _ws = _wb.Worksheets[1];

            int countOfRow = 0;
            int index = 2;
            var locationList = new List<FCRegularLocation>();
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preid);

            // 扫描有多少行即有多少个条目
            while(index > 0)
            {
                if (_ws.Cells[index, 1].Value2 != null)
                {
                    countOfRow += 1;
                    index += 1;
                }
                else
                {
                    break;
                }
            }

            for (int i = 0; i < countOfRow; i++)
            {
                locationList.Add(new FCRegularLocation {
                    PurchaseOrder = _ws.Cells[i + 2, 1].Value2.ToString(),
                    Style = _ws.Cells[i + 2, 2].Value2.ToString(),
                    Color = _ws.Cells[i + 2, 3].Value2.ToString(),
                    CustomerCode = _ws.Cells[i + 2, 4].Value2.ToString(),
                    SizeBundle = _ws.Cells[i + 2, 5].Value2.ToString(),
                    PcsBundle = _ws.Cells[i + 2, 1].Value2.ToString(),
                    Cartons = (int)_ws.Cells[i + 2, 1].Value2,
                    Quantity = (int)_ws.Cells[i + 2, 1].Value2,
                    Location = _ws.Cells[i + 2, 1].Value2.ToString(),
                    InboundDate = _dateTimeNow,
                    PreReceiveOrder = preReceiveOrderInDb
                });
            }

            _context.FCRegularLocations.AddRange(locationList);
            _context.SaveChanges();
        }
    }
}