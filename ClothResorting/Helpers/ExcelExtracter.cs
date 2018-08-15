using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using System.Collections;
using ClothResorting.Models;
using System.Diagnostics;
using System.Data.Entity;
using System.Web.Http;
using System.Net;
using ClothResorting.Models.DataTransferModels;

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
        private double? _netWeight;
        private double? _grossWeight;
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
        #region
        public ExcelExtracter()
        {
            _context = new ApplicationDbContext();
            _dateTimeNow = DateTime.Now;
        }

        public ExcelExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _dateTimeNow = DateTime.Now;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }
        #endregion

        //-----👇👇👇👇👇👇-----以下为抽取SILKICON装箱单的新方法-----👇👇👇👇👇👇-----
        //建立一个Pre-Recieve Order对象并添加进数据库
        #region
        public void CreatePreReceiveOrder()
        {
            //建立一个PreReceiveOrder对象
            var newOrder = new PreReceiveOrder
            {
                ActualReceivedCtns = 0,
                CustomerName = "Unknown",
                CreatDate = _dateTimeNow,
                TotalCartons = 0,
                TotalGrossWeight = 0,
                TotalNetWeight = 0,
                TotalVol = 0,
                ContainerNumber = "Unknown",
                TotalPcs = 0,
                ActualReceivedPcs = 0,
                Status = "New Created"
            };

            _context.PreReceiveOrders.Add(newOrder);
            _context.SaveChanges();
        }
        #endregion

        //扫描单页模板，计算每一个POSummary并写入数据库
        #region
        public void ExtractSIPOSummaryAndCartonDetail(int preId, string orderType)
        {
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preId);     //获取刚建立的PreReceiveOrder

            var poList = new List<POSummary>();
            var cartonList = new List<RegularCartonDetail>();
            _ws = _wb.Worksheets[1];
            _countOfPo = 0;
            var index = 1;

            //扫描POSummary的数量
            do
            {
                if (_ws.Cells[index, 1].Value2 != null && _ws.Cells[index + 1, 1].Value2 == null)
                {
                    _countOfPo++;
                }

                if (_ws.Cells[index, 1].Value2 == null && _ws.Cells[index + 1, 1].Value2 == null)
                {
                    break;
                }

                index++;
            } while (index > 0);

            //先建立这么多数量的空POSummary
            for(int i = 0; i < _countOfPo; i++)
            {
                poList.Add(new POSummary {
                    PreReceiveOrder = preReceiveOrderInDb
                });
            }

            //先写入数据库一次
            _context.POSummaries.AddRange(poList);
            _context.SaveChanges();

            //分别扫描各个POSummary的CartonDetail
            var startIndex = 1;
            var poSummariesInDb = _context.POSummaries.OrderByDescending(x => x.Id).Take(poList.Count);
            
            foreach(var poSummary in poSummariesInDb)
            {
                //扫描当前POSummary对象的CartonDetail有多少行
                index = startIndex + 1;
                var cartonDetailList = new List<RegularCartonDetail>();
                var countOfSKU = 0;
                var countOfColumn = 0;
                
                //扫描一个POSummary有多少个SKU
                while (_ws.Cells[index, 1].Value2 != null)
                {
                    countOfSKU++;
                    index++;
                }

                //扫描一个POSummary对象包含多少列
                index = 1;
                while (_ws.Cells[startIndex, index].Value2 != null)
                {
                    countOfColumn++;
                    index++;
                }

                for(int j = 0; j < countOfSKU; j++)
                {
                    //扫描每一种SKU有多少种Size及数量
                    var sizeList = new List<SizeRatio>();
                    var countOfSize = countOfColumn - 12;

                    for (int k = 0; k < countOfSize; k++)
                    {
                        sizeList.Add(new SizeRatio
                        {
                            SizeName = _ws.Cells[startIndex, 11 + k].Value2.ToString(),
                            Count = _ws.Cells[startIndex + 1 + j, 11 + k].Value2 == null ? 0 : (int)_ws.Cells[startIndex + 1 + j, 11 + k].Value2
                        });
                    }

                    if (orderType == "Replenishment")
                    {
                        //为每一个不为0的size都生成一个cartonDetail对象
                        foreach (var size in sizeList)
                        {
                            if (size.Count != 0)
                            {
                                _netWeight = _ws.Cells[startIndex + 1 + j, 7].Value2;
                                _grossWeight = _ws.Cells[startIndex + 1 + j, 6].Value2;
                                _runCode = _ws.Cells[startIndex + 1 + j, 4].Value2;
                                _dimension = _ws.Cells[startIndex + 1 + j, 5].Value2;

                                cartonDetailList.Add(new RegularCartonDetail
                                {
                                    CartonRange = _ws.Cells[startIndex + 1 + j, 1].Value2.ToString(),
                                    PurchaseOrder = _ws.Cells[startIndex + 1 + j, 2].Value2.ToString(),
                                    Style = _ws.Cells[startIndex + 1 + j, 3].Value2.ToString(),
                                    Customer = _runCode == null ? "" : _runCode.ToString(),
                                    Dimension = _dimension == null ? "" : _dimension.ToString(),
                                    GrossWeight = _grossWeight == null ? 0 : (double)_grossWeight,
                                    NetWeight = _netWeight == null ? 0 : (double)_netWeight,
                                    Color = _ws.Cells[startIndex + 1 + j, 9].Value2.ToString(),
                                    Cartons = (int)_ws.Cells[startIndex + 1 + j, 10].Value2,
                                    PcsPerCarton = (int)_ws.Cells[startIndex + 1 + j, countOfColumn - 1].Value2,
                                    Quantity = (int)_ws.Cells[startIndex + 1 + j, countOfColumn].Value2,
                                    SizeBundle = size.SizeName,
                                    PcsBundle = size.Count.ToString(),
                                    Status = "Created",
                                    OrderType = orderType,
                                    POSummary = poSummary
                                });
                            }
                        }
                    }
                    else if (orderType == "Regular")    //Regular类型的订单size和pcs为捆绑字符，入S M L XL/1 2 2 1
                    {
                        var sizeBundle = sizeList[0].SizeName;
                        var pcsBundle = sizeList[0].Count.ToString();

                        //统计当前SKU中的size组合
                        for (int k = 1; k < sizeList.Count; k++)
                        {
                            sizeBundle += " " + sizeList[k].SizeName;
                            pcsBundle += " " + sizeList[k].Count.ToString();
                        }

                        _netWeight = _ws.Cells[startIndex + 1 + j, 7].Value2;
                        _grossWeight = _ws.Cells[startIndex + 1 + j, 6].Value2;
                        _runCode = _ws.Cells[startIndex + 1 + j, 4].Value2;
                        _dimension = _ws.Cells[startIndex + 1 + j, 5].Value2;
                        var cartonRange = _ws.Cells[startIndex + 1 + j, 1].Value2.ToString();

                        cartonDetailList.Add(new RegularCartonDetail
                        {
                            CartonRange = cartonRange,
                            PurchaseOrder = _ws.Cells[startIndex + 1 + j, 2].Value2.ToString(),
                            Style = _ws.Cells[startIndex + 1 + j, 3].Value2.ToString(),
                            Customer = _runCode == null ? "" : _runCode.ToString(),
                            Dimension = _dimension == null ? "" : _dimension.ToString(),
                            GrossWeight = _grossWeight == null ? 0 : (double)_grossWeight,
                            NetWeight = _netWeight == null ? 0 : (double)_netWeight,
                            Color = _ws.Cells[startIndex + 1 + j, 9].Value2.ToString(),
                            Cartons = cartonDetailList.Where(x => x.CartonRange == cartonRange).Count() == 0 ? (int)_ws.Cells[startIndex + 1 + j, 10].Value2 : 0,        //同一箱只会计一次箱数，但件数还是分开记
                            PcsPerCarton = (int)_ws.Cells[startIndex + 1 + j, countOfColumn - 1].Value2,
                            Quantity = (int)_ws.Cells[startIndex + 1 + j, countOfColumn].Value2,
                            SizeBundle = sizeBundle,
                            PcsBundle = pcsBundle,
                            Status = "Created",
                            OrderType = orderType,
                            POSummary = poSummary
                        });
                    }

                    //经过计算cartonDetailList的信息，重新补充POSummary的信息
                    poSummary.Container = "UnKnown";
                    poSummary.PurchaseOrder = cartonDetailList[0].PurchaseOrder;
                    poSummary.Style = cartonDetailList[0].Style;
                    poSummary.Customer = "Silk-Icon";
                    poSummary.Quantity = cartonDetailList.Sum(x => x.Quantity);
                    poSummary.Cartons = cartonDetailList.Sum(x => x.Cartons);
                    poSummary.OrderType = orderType;
                }
                startIndex += countOfSKU + 2;

                cartonList.AddRange(cartonDetailList);
            }

            //重新统计新建的preReceiveOrder对象的数据
            preReceiveOrderInDb.TotalCartons = cartonList.Sum(x => x.Cartons);
            preReceiveOrderInDb.TotalPcs = cartonList.Sum(x => x.Quantity);
            preReceiveOrderInDb.CustomerName = "Silk-Icon";

            _context.RegularCartonDetails.AddRange(cartonList);
            _context.SaveChanges();
        }
        #endregion
        //-----👆👆👆👆👆👆-----以上为抽取SILKICON装箱单的新方法-----👆👆👆👆👆👆-----



        //-----👇👇👇👇👇👇-----以下为抽取SILKICON装箱单的旧方法-----👇👇👇👇👇👇-----
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
                    NetWeight = _netWeight,
                    GrossWeight = _grossWeight,
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

        //私有方法，获取箱号范围的前后段
        #region
        //从类似"12-25"字符串中获取箱号范围的前段
        private int GetFrom(string cn)
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
        private int GetTo(string cn)
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
        #endregion

        //-----👆👆👆👆👆👆-----以上为抽取SILKICON装箱单的旧方法-----👆👆👆👆👆👆-----



        ////以CartonDetail为单位，抽取批量散货的excel信息(与packinglist无关，仅散货)
        #region
        //public void ExtractBulkloadRecord()
        //{
        //    var numberOfWorkSheet = _wb.Worksheets.Count;

        //    //遍历每一张ws
        //    for(int i = 1; i <= numberOfWorkSheet; i++)
        //    {
        //        var n = 3;
        //        var cartonBreakDownList = new List<CartonBreakDown>();
        //        var list = new List<CartonDetail>();
        //        var cartonClassCount = 0;

        //        _ws = _wb.Worksheets[i];
        //        _purchaseOrder = _ws.Cells[1, 2].Value2 == null ? "" : _ws.Cells[1, 2].Value2.ToString();
        //        _numberOfSizeRatio = _ws.Cells[1, 4].Value2;

        //        //数有多少carton储存对象(cartonDetail)
        //        while (_ws.Cells[n, 3].Value2 != null)
        //        {
        //            cartonClassCount += 1;
        //            n += 1;
        //        }

        //        //将ws中的每一个cartonDetail对象添加到代添加到数据库的列表中
        //        for (int k = 0; k < cartonClassCount; k++)
        //        {
        //            //获取SizeRaio表
        //            var sizeList = new List<SizeRatio>();

        //            //扫描当前cartonDetail包含的sizeRatio
        //            for (int p = 0; p < _numberOfSizeRatio; p++)
        //            {
        //                if (_ws.Cells[k + 3, 5 + p].Value2 != null && _ws.Cells[k + 3, 5 + p].Value2 != 0)
        //                {
        //                    sizeList.Add(new SizeRatio
        //                    {
        //                        Count = (int)_ws.Cells[k + 3, 5 + p].Value2,
        //                        SizeName = _ws.Cells[2, 5 + p].Value2
        //                    });
        //                }
        //            }

        //            //将ws中的关键变量储存至内存中
        //            _style = _ws.Cells[k + 3, 1].Value2.ToString();
        //            _color = _ws.Cells[k + 3, 2].Value2.ToString();
        //            int numberOfCartons = (int)_ws.Cells[k + 3, 3].Value2;
        //            var location = _ws.Cells[k + 3, 4].Value2 == null ? "N/A" : _ws.Cells[k + 3, 4].Value2.ToString();

        //            //新建CartonDetail对象，将其添加到list中
        //            var carton = new CartonDetail
        //            {
        //                PurchaseOrder = _purchaseOrder,
        //                Style = _style,
        //                Color = _color,
        //                SumOfCarton = 0,
        //                ActualReceived = numberOfCartons,
        //                Available = numberOfCartons,
        //                Location = location,
        //                ActualReceivedPcs = 0,
        //                AvailablePcs = 0,
        //                SizeRatios = sizeList,
        //                ReceivedDate = _dateTimeNow
        //            };

        //            //遍历sizeRatios，生成数个cartonBreakdown
        //            for (int s = 0; s < sizeList.Count; s++)
        //            {
        //                var cartonBreakDown = new CartonBreakDown
        //                {
        //                    PurchaseOrder = _purchaseOrder.ToString(),
        //                    Style = _style,
        //                    Color = _color,
        //                    CartonNumberRangeFrom = 0,
        //                    CartonNumberRangeTo = 0,
        //                    RunCode = "",
        //                    Size = sizeList[s].SizeName,
        //                    ForecastPcs = 0,
        //                    PcsPerCartons = sizeList[s].Count,
        //                    ActualPcs = sizeList[s].Count,
        //                    AvailablePcs = 0,
        //                    Location = location,
        //                    CartonDetail = carton,
        //                    ReceivedDate = _dateTimeNow
        //                };

        //                carton.ActualReceivedPcs = sizeList[s].Count;
        //                carton.AvailablePcs = sizeList[s].Count;
        //                cartonBreakDownList.Add(cartonBreakDown);
        //            }

        //            list.Add(carton);
        //            i += 1;
        //        }

        //        //写进数据库
        //        _context.CartonBreakDowns.AddRange(cartonBreakDownList);
        //        _context.CartonDetails.AddRange(list);
        //        _context.SaveChanges();

        //        //释放EXCEL资源
        //        Dispose();
        //    }
        //}
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



        //-----👇👇👇👇👇👇-----以下为抽取FC装箱单的方法-----👇👇👇👇👇👇-----
        //新建FreeCountry的预收货订单
        #region
        public void CreateFCPreReceiveOrder()
        {
            _context.PreReceiveOrders.Add(new PreReceiveOrder
            {
                CustomerName = "Free Country",
                CreatDate = _dateTimeNow,
                ContainerNumber = "UNKNOWN",
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
        #endregion

        //抽取excel文件中的PO信息，并与之前新建的FC预收订单关联
        #region
        public void ExtractFCPurchaseOrderSummary(int id)
        {
            _ws = _wb.Worksheets[1];
            var packingList = new List<POSummary>();
            var index = 2;
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(id);
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
                    PreReceiveOrder = preReceiveOrderInDb
                });

                index += 2;
            }

            //可以在本页面获取packingList的总量
            preReceiveOrderInDb.TotalCartons = (int) _ws.Cells[_countOfPo * 2 + 1, 8].Value2;
            preReceiveOrderInDb.TotalPcs = (int)_ws.Cells[_countOfPo * 2 + 1, 6].Value2;
            preReceiveOrderInDb.CustomerName = "Free Country";

            _context.POSummaries.AddRange(packingList);
            _context.SaveChanges();
        }
        #endregion

        //抽取Detail中的各个PO详细信息
        #region
        public void ExtractFCPurchaseOrderDetail(int id)
        {
            _ws = _wb.Worksheets[2];
            var rowIndex = 1;
            id = _context.PreReceiveOrders.OrderByDescending(c => c.Id).First().Id;
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

                    var sizeBundle = sizeList[0].SizeName;
                    var pcsBundle = sizeList[0].Count.ToString();

                    //统计当前SKU中的size组合
                    for(int index = 1; index < sizeList.Count; index++)
                    {
                        sizeBundle += " " + sizeList[index].SizeName;
                        pcsBundle += " " + sizeList[index].Count.ToString();
                    }

                    var sizeBD = CheckSizeName(sizeBundle, pcsBundle);
                    var pcsBD = CheckPcs(sizeBundle, pcsBundle);

                    var poSummaryInDbs = _context.POSummaries
                        .Include(c => c.PreReceiveOrder)
                        .Include(c => c.RegularCartonDetails)
                        .Where(c => c.PurchaseOrder == purchaseOrder
                            && c.PreReceiveOrder.Id == id
                            && c.PoLine == poLine);

                    //判断是否有相同的poSummary,相同的poSummary就意味着有相同的CartionDetail,必须一对一连接他们之间的关系
                    if (poSummaryInDbs.Count() == 1)
                    {
                        var poSummaryInDb = poSummaryInDbs.First();

                        var regularCartonDetail = new RegularCartonDetail
                        {
                            CartonRange = _ws.Cells[startIndex + 3 + j, 1].Value2.ToString(),
                            PurchaseOrder = _ws.Cells[startIndex + 3 + j, 2].Value2.ToString(),
                            Style = _ws.Cells[startIndex + 3 + j, 3].Value2.ToString(),
                            Customer = _ws.Cells[startIndex + 3 + j, 4].Value2.ToString(),
                            Dimension = _ws.Cells[startIndex + 3 + j, 5].Value2.ToString(),
                            GrossWeight = 0,
                            NetWeight = 0,
                            Color = _ws.Cells[startIndex + 3 + j, 10].Value2.ToString(),
                            Cartons = (int)_ws.Cells[startIndex + 3 + j, 11].Value2,
                            SizeBundle = sizeBD,
                            PcsBundle = pcsBD,
                            PcsPerCarton = (int)_ws.Cells[startIndex + 3 + j, countOfColumn - 1].Value2,
                            Quantity = (int)_ws.Cells[startIndex + 3 + j, countOfColumn].Value2,
                            ActualCtns = 0,
                            ActualPcs = 0,
                            InboundDate = null,
                            Status = "Created",
                            ToBeAllocatedCtns = 0,
                            ToBeAllocatedPcs = 0,
                            POSummary = poSummaryInDb
                        };

                        regularCartonDetailList.Add(regularCartonDetail);
                    }
                    else if (poSummaryInDbs.Count() > 1)
                    {
                        var regularCartonDetail = new RegularCartonDetail
                        {
                            CartonRange = _ws.Cells[startIndex + 3 + j, 1].Value2.ToString(),
                            PurchaseOrder = _ws.Cells[startIndex + 3 + j, 2].Value2.ToString(),
                            Style = _ws.Cells[startIndex + 3 + j, 3].Value2.ToString(),
                            Customer = _ws.Cells[startIndex + 3 + j, 4].Value2.ToString(),
                            Dimension = _ws.Cells[startIndex + 3 + j, 5].Value2.ToString(),
                            GrossWeight = 0,
                            NetWeight = 0,
                            Color = _ws.Cells[startIndex + 3 + j, 10].Value2.ToString(),
                            Cartons = (int)_ws.Cells[startIndex + 3 + j, 11].Value2,
                            SizeBundle = sizeBD,
                            PcsBundle = pcsBD,
                            PcsPerCarton = (int)_ws.Cells[startIndex + 3 + j, countOfColumn - 1].Value2,
                            Quantity = (int)_ws.Cells[startIndex + 3 + j, countOfColumn].Value2,
                            ActualCtns = 0,
                            ActualPcs = 0,
                            InboundDate = null,
                            Status = "Created",
                            ToBeAllocatedCtns = 0,
                            ToBeAllocatedPcs = 0
                        };

                        foreach (var poSummaryIndb in poSummaryInDbs)
                        {
                            if (regularCartonDetailList
                                .SingleOrDefault(c => c.PurchaseOrder == regularCartonDetail.PurchaseOrder
                                    && c.Style == regularCartonDetail.Style
                                    && c.Customer == regularCartonDetail.Customer
                                    && c.Color == regularCartonDetail.Color
                                    && c.PcsBundle == regularCartonDetail.PcsBundle
                                    && c.SizeBundle == regularCartonDetail.SizeBundle
                                    && c.POSummary == poSummaryIndb) == null)
                            {
                                regularCartonDetail.POSummary = poSummaryIndb;
                                regularCartonDetailList.Add(regularCartonDetail);
                                break;
                            }
                        }
                    }

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
        #endregion

        //私有辅助方法，检查一个cartonDetail中装有多少种Size，只有一种Size意味着是Solid pack，否则是Buncle pack。
        #region
        //如果箱子中只有一种size，只返回这种size的size名称，否则返回原有名称
        private string CheckSizeName(string sizeBundle, string pcsBundle)
        {
            var sizeString = sizeBundle.Split(' ');
            var pcsString = pcsBundle.Split(' ');
            var pcsCount = pcsString.Count();
            var zeroCount = 0;
            var noneZeroIndex = 0;

            for(int i = 0; i < pcsCount; i++)
            {
                if (pcsString[i] == "0")
                {
                    zeroCount += 1;
                }
            }

            if (zeroCount == pcsCount - 1)      //只有一个pcs有数量其他全为零，说明这是solid pack
            {
                for(int i = 0; i < pcsCount; i++)  //找出第几个pcs是不为零的
                {
                    if (pcsString[i] != "0")
                    {
                        noneZeroIndex = i;
                    }
                }

                return sizeString[noneZeroIndex];   //返回不为零的pcs对应的size名
            }
            else    //否则说明这是bundle pack
            {
                return sizeBundle;
            }
        }


        //如果箱子中只有一种size，只返回这种size的数量，否则返回原有熟练bundle
        private string CheckPcs(string sizeBundle, string pcsBundle)
        {
            var pcsString = pcsBundle.Split(' ');
            var pcsCount = pcsString.Count();
            var zeroCount = 0;
            var pcs = "0";

            foreach (var p in pcsString)
            {
                if (p == "0")
                {
                    zeroCount += 1;
                }
                else
                {
                    pcs = p;
                }
            }

            if (zeroCount == pcsCount - 1)      //只有一个pcs有数量其他全为零，说明这是solid pack
            {
                return pcs;
            }
            else    //否则说明这是bundle pack
            {
                return pcsBundle;
            }
        }
        #endregion
        //-----👆👆👆👆👆👆-----以上为抽取FC装箱单的方法-----👆👆👆👆👆👆-----



        //// 抽取FreeCountry正常订单的库存模板分配信息
        #region
        //public void ExtractFCRegularLocation(int preid)
        //{
        //    _ws = _wb.Worksheets[1];

        //    int countOfRow = 0;
        //    int index = 2;
        //    var locationList = new List<FCRegularLocation>();
        //    var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preid);

        //    // 扫描有多少行即有多少个条目
        //    while(index > 0)
        //    {
        //        if (_ws.Cells[index, 1].Value2 != null)
        //        {
        //            countOfRow += 1;
        //            index += 1;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    for (int i = 0; i < countOfRow; i++)
        //    {
        //        locationList.Add(new FCRegularLocation {
        //            PurchaseOrder = _ws.Cells[i + 2, 1].Value2.ToString(),
        //            Style = _ws.Cells[i + 2, 2].Value2.ToString(),
        //            Color = _ws.Cells[i + 2, 3].Value2.ToString(),
        //            CustomerCode = _ws.Cells[i + 2, 4].Value2.ToString(),
        //            SizeBundle = _ws.Cells[i + 2, 5].Value2.ToString(),
        //            PcsBundle = _ws.Cells[i + 2, 1].Value2.ToString(),
        //            Cartons = (int)_ws.Cells[i + 2, 1].Value2,
        //            Quantity = (int)_ws.Cells[i + 2, 1].Value2,
        //            Location = _ws.Cells[i + 2, 1].Value2.ToString(),
        //            InboundDate = _dateTimeNow,
        //            PreReceiveOrder = preReceiveOrderInDb
        //        });
        //    }

        //    _context.FCRegularLocations.AddRange(locationList);
        //    _context.SaveChanges();
        //}
        #endregion


        //-----👇👇👇👇👇👇-----以下为抽取FC出货单的方法-----👇👇👇👇👇👇-----
        //抽取Pull sheet模板中的信息，生成ShipOrder下的拣货记录表，并从原库存中将可用箱数部分或全部转化为“拣货中”箱数
        #region
        public void ExtractPullSheet(int pullSheetId)
        {
            var pullSheet = _context.PullSheets.Find(pullSheetId);
            var diagnosticList = new List<PullSheetDiagnostic>();
            //首先抽取第一页的PSI信息，将PSI中指定的内容放入一个储存在内存中的“待选对象池”，池中内容不一定都会用完
            _ws = _wb.Worksheets[1];

            var psiRowCount = 0;
            var index = 2;
            var psiList = new List<PSIModel>();

            while(_ws.Cells[index, 1].Value2 != null)
            {
                psiRowCount += 1;
                index += 1;
            }

            //检查是否有重复的pis记录
            for (int i = 0; i < psiRowCount; i++)
            {
                var container = _ws.Cells[i + 2, 1].Value2.ToString();
                var cutPo = _ws.Cells[i + 2, 2].Value2.ToString();
                var style = _ws.Cells[i + 2, 3].Value2.ToString();
                var isExisted = false;

                foreach(var psi in psiList)
                {
                    if (psi.Container == container && psi.CutPurchaseOrder == cutPo && psi.Style == style)
                    {
                        isExisted = true;
                    }
                }

                if (isExisted == false)
                {
                    psiList.Add(new PSIModel
                    {
                        Container = container,
                        CutPurchaseOrder = cutPo,
                        Style = style
                    });
                }
            }

            //基于PSI信息，查找整个数据库的库存对象，将这些对象放在一个“待选池”列表中
            var cartonLocationPool = new List<FCRegularLocationDetail>();

            foreach(var psi in psiList)
            {
                var psiResult = _context.FCRegularLocationDetails
                    .Where(c => c.Container == psi.Container
                        && c.PurchaseOrder == psi.CutPurchaseOrder
                        && c.Style == psi.Style)
                    .ToList();

                cartonLocationPool.AddRange(psiResult);
            }

            //然后抽取第二页的Pull Sheet模板化的信息, 在“待选池”中扣除抽取出来的信息
            _ws = _wb.Worksheets[2];
            var pullSheetCount = 0;
            index = 1;

            //扫描有多少种需要拣货的SKU
            while(_ws.Cells[index, 1].Value2 != null)
            {
                pullSheetCount += 1;
                index += 3;
            }

            //每一个SKU都在“待选池”中拣货
            var pickDetailList = new List<PickDetail>();
            var usedPoolCartonLocationDetails = new List<FCRegularLocationDetail>();

            for(int i = 1; i <= pullSheetCount; i++)
            {
                index = 4;      //size列的起始点
                var startRow = (i - 1) * 3 + 1;
                string style = _ws.Cells[startRow + 1, 1].Value2.ToString();
                var color = _ws.Cells[startRow + 1, 2].Value2.ToString();
                string purchaseOrder = _ws.Cells[startRow + 1, 3].Value2.ToString();
                var sizeCount = 0;
                var sizeList = new List<SizeRatio>();

                //扫描每一种SKU有多少种Size
                while (_ws.Cells[startRow, index].Value2 != null)
                {
                    sizeCount += 1;
                    index += 1;
                }

                //扫描每一种需求的Size名称和件数
                for (int j = 0; j < sizeCount; j++)
                {
                    sizeList.Add(new SizeRatio
                    {
                        SizeName = _ws.Cells[startRow, 4 + j].Value2.ToString(),
                        Count = (int)_ws.Cells[startRow + 1, 4 + j].Value2
                    });
                }

                //确定拣货对象(Cut PO)的种类
                var sku = _context.POSummaries
                    .Include(x => x.RegularCartonDetails)
                    .Where(x => x.Style == style
                        && x.PurchaseOrder == purchaseOrder)
                    .FirstOrDefault();

                var skuCount = sku == null ? 0 : sku.RegularCartonDetails.Count;

                if (skuCount > 1)       //如果POSummary不只一个RegularCartonDetail对象就说明是Solid
                {

                    //为该SKU下的每一种Size备货
                    foreach (var size in sizeList)
                    {
                        //待选池中所有符合拣货条件的对象
                        var poolLocations = cartonLocationPool.Where(x => x.Style == style
                                && x.Color == color
                                && x.SizeBundle == size.SizeName
                                && x.PurchaseOrder == purchaseOrder);

                        if (poolLocations.Count() == 0)
                        {
                            diagnosticList.Add(new PullSheetDiagnostic
                            {
                                Type = "Missing",
                                DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                                Description = "SKU style:<font color='red'>" + style + "</font>, Color:<font color='red'>" + color + "</font> was not found. Some PSI infomations must be missed or incorrect.<br>Please check if the related container number listed in PSI is existed correct.",
                                PullSheet = pullSheet
                            });

                            _context.PullSheetDiagnostics.AddRange(diagnosticList);
                            _context.SaveChanges();
                        }

                        var targetPcs = size.Count;

                        foreach (var pool in poolLocations)
                        {
                            //当当前的待选对象件数小于等于目标件数时，全部拿走，并生成对应的PickDetail
                            if (pool.AvailablePcs <= targetPcs && pool.AvailablePcs != 0 && targetPcs!= 0)
                            {
                                pickDetailList.Add(ConvertToSolidPickDetail(pullSheet, pool, pool.AvailablePcs));

                                targetPcs -= pool.AvailablePcs;

                                pool.PickingCtns += pool.AvailableCtns;
                                pool.PickingPcs += pool.AvailablePcs;

                                pool.AvailableCtns = 0;
                                pool.AvailablePcs = 0;

                                //将有变化的结果放到新建的“使用过的待选池”中
                                usedPoolCartonLocationDetails.Add(pool);
                            }
                            //当当前的待选对象件数大于目标件数时，只拿走需要的，并生成对应的PickDetail
                            else if ( pool.AvailablePcs > targetPcs && pool.AvailablePcs != 0 && targetPcs != 0)
                            {
                                pickDetailList.Add(ConvertToSolidPickDetail(pullSheet, pool, targetPcs));

                                pool.PickingCtns += targetPcs / pool.PcsPerCaron;
                                pool.PickingPcs += targetPcs;

                                pool.AvailableCtns -= targetPcs / pool.PcsPerCaron;
                                pool.AvailablePcs -= targetPcs;

                                targetPcs = 0;

                                //将有变化的结果放到新建的“使用过的待选池”中
                                usedPoolCartonLocationDetails.Add(pool);
                            }
                        }

                        //如果targetPcs还没收集齐，则缺货
                        if (targetPcs > 0)
                        {
                            // ...生成缺货记录
                            diagnosticList.Add(new PullSheetDiagnostic {
                                Type = "Shortage",
                                DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                                Description = "Pull sheet and PSI may be incorrect. Please check SKU detail of Style:<font color='red'>" + style + "</font>, Color:<font color='red'>" + color +"</font> In pull sheet.",
                                PullSheet = pullSheet
                            });
                        }

                        //usedPoolCartonLocationDetails.AddRange(poolLocations);
                    }
                }
                else if (skuCount == 1)    //如果POSummary下只有一个RegularCartonDetail对象就说明是Bundle
                {
                    //待选池中所有符合拣货条件的对象
                    var poolLocations = cartonLocationPool.Where(x => x.Style == style
                            && x.Color == color
                            && x.PurchaseOrder == purchaseOrder);

                    if (poolLocations.Count() == 0)
                    {
                        diagnosticList.Add(new PullSheetDiagnostic
                        {
                            Type = "Missing",
                            DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                            Description = "SKU style:<font color='red'>" + style + "</font>, Color:<font color='red'>" + color + "</font> was not found. Some PSI infomations must be missed or incorrect.<br>Please check if the related container number listed in PSI is existed and correct.",
                            PullSheet = pullSheet
                        });

                        _context.PullSheetDiagnostics.AddRange(diagnosticList);
                        _context.SaveChanges();
                    }

                    //计算该SKU的目标箱数， 箱数 = 总件数 / 每箱件数
                    var targetCtns = sizeList.Sum(x => x.Count) / poolLocations.First().PcsPerCaron;

                    foreach(var pool in poolLocations)
                    {
                        //当当前的待选对象箱数小于等于目标箱数时，全部拿走，并记录
                        if (pool.AvailableCtns <= targetCtns && pool.AvailableCtns != 0 && targetCtns != 0)
                        {
                            pickDetailList.Add(ConvertToBundlePickDetail(pullSheet, pool, pool.AvailableCtns));

                            pool.PickingCtns += pool.AvailableCtns;
                            pool.PickingPcs += pool.AvailableCtns * pool.PcsPerCaron;

                            targetCtns -= pool.AvailableCtns;

                            pool.AvailableCtns = 0;
                            pool.AvailablePcs = 0;

                            //将有变化的结果放到新建的“使用过的待选池”中
                            usedPoolCartonLocationDetails.Add(pool);
                        }
                        //当当前的待选对象箱数大于目标箱数时，只拿走需要的，并记录
                        else if (pool.AvailableCtns > targetCtns && pool.AvailableCtns != 0 && targetCtns != 0)
                        {
                            pickDetailList.Add(ConvertToBundlePickDetail(pullSheet, pool, targetCtns));

                            pool.PickingCtns += targetCtns;
                            pool.PickingPcs += targetCtns * pool.PcsPerCaron;

                            pool.AvailableCtns -= targetCtns;
                            pool.PickingPcs -= targetCtns * pool.PcsPerCaron;

                            targetCtns = 0;

                            //将有变化的结果放到新建的“使用过的待选池”中
                            usedPoolCartonLocationDetails.Add(pool);
                        }
                    }

                    //如果targetCtns还没收集齐，则缺货
                    if (targetCtns > 0)
                    {
                        //...生成缺货记录
                        diagnosticList.Add(new PullSheetDiagnostic
                        {
                            Type = "Shortage",
                            DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                            Description = "Pull sheet and PSI may be incorrect. Please check SKU detail of Style:<font color='red'>" + style + "</font>, Color:<font color='red'>" + color + "</font> In pull sheet.",
                            PullSheet = pullSheet
                        });
                    }
                    
                    //usedPoolCartonLocationDetails.AddRange(poolLocations);
                }
                else    //最后是count等于0,即数据库中不存在相关sku记录的情况,把该sku打印出来,生成缺货记录
                {
                    //...生成缺货记录
                    diagnosticList.Add(new PullSheetDiagnostic
                    {
                        Type = "Missing",
                        DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                        Description = "SKU style:<font color='red'>" + style + "</font>, Color:<font color='red'>" + color + "</font> was not found. Some PSI infomations must be missed or incorrect.<br>Please check if the related container number listed in PSI is existed and correct.",
                        PullSheet = pullSheet
                    });
                }
            }

            //将新收集的"备选池"对象同步到其原有的数据库对象中去
            var cartonLocationDetailsInDb = _context.FCRegularLocationDetails.Where(x => x.Id > 0);

            foreach(var usedCartonLocation in usedPoolCartonLocationDetails)
            {
                var cartonInDb = cartonLocationDetailsInDb.SingleOrDefault(x => x.Id == usedCartonLocation.Id);

                if (cartonInDb.Status == "In Stock")
                {
                    cartonInDb.Status = "Picking";
                }

                cartonInDb.AvailableCtns = usedCartonLocation.AvailableCtns;
                cartonInDb.AvailablePcs = usedCartonLocation.AvailablePcs;
                cartonInDb.PickingCtns = usedCartonLocation.PickingCtns;
                cartonInDb.PickingPcs = usedCartonLocation.PickingPcs;
            }

            // 最后更改PullSheet的状态
            _context.PullSheets.Find(pullSheetId).Status = "Picking";

            _context.PickDetails.AddRange(pickDetailList);
            _context.PullSheetDiagnostics.AddRange(diagnosticList);
            _context.SaveChanges();
        }
        #endregion

        //辅助方法：根据调整后的pool以及取货数量，生成该pullsheet下的pickdetail
        #region
        private PickDetail ConvertToSolidPickDetail(PullSheet pullSheet, FCRegularLocationDetail pool, int targetPcs)
        {
            return new PickDetail
            {
                PurchaseOrder = pool.PurchaseOrder,
                Style = pool.Style,
                Color = pool.Color,
                SizeBundle = pool.SizeBundle,
                PcsBundle = pool.PcsBundle,
                CustomerCode = pool.CustomerCode,
                PickDate = _dateTimeNow.ToString("MM/dd/yyyy"),
                Container = pool.Container,
                Location = pool.Location,
                Status = "Picking",
                PcsPerCarton = pool.PcsPerCaron,
                PickCtns = targetPcs / pool.PcsPerCaron,
                PickPcs = targetPcs,
                PullSheet = pullSheet,
                LocationDetailId = pool.Id
            };
        }

        private PickDetail ConvertToBundlePickDetail(PullSheet pullSheet, FCRegularLocationDetail pool, int targetCtns)
        {
            return new PickDetail
            {
                PurchaseOrder = pool.PurchaseOrder,
                Style = pool.Style,
                Color = pool.Color,
                SizeBundle = pool.SizeBundle,
                PcsBundle = pool.PcsBundle,
                CustomerCode = pool.CustomerCode,
                PickDate = _dateTimeNow.ToString("MM/dd/yyyy"),
                Status = "Picking",
                Container = pool.Container,
                Location = pool.Location,
                PcsPerCarton = pool.PcsPerCaron,
                PickPcs = targetCtns * pool.PcsPerCaron,
                PickCtns = targetCtns,
                PullSheet = pullSheet,
                LocationDetailId = pool.Id
            };
        }
        #endregion
        //-----👆👆👆👆👆👆-----以上为抽取FC出货单的方法-----👆👆👆👆👆👆-----



        //强行中止EXCEL进程的方法
        #region
        public void Dispose()
        {
            var excelProcs = Process.GetProcessesByName("EXCEL");

            foreach (var procs in excelProcs)
            {
                procs.Kill();
            }
        }
        #endregion

        //公共方法，获取最新创建的PreReceiveOrder
        public int GetLatestPreReceiveOrderId()
        {
            return _context.PreReceiveOrders.OrderByDescending(c => c.Id).First().Id;
        }
    }
}