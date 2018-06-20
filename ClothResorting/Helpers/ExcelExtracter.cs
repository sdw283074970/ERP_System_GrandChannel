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
        private DateTime dateTimeNow = DateTime.Now;
        #endregion

        //PackingList全局变量
        #region
        private string _purchaseOrder;
        private string _styleNumber;
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
            _excel = new _Excel.Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //建立一个Pre-Recieve Order对象并添加进数据库
        #region
        public void CreatePreReceiveOrderAndOverView()
        {
            _ws = _wb.Worksheets[1];

            //建立一个PreReceiveOrder对象
            var newOrder = new PreReceiveOrder
            {
                Available = 0,
                ActualReceived = 0,
                CustomerName = _ws.Cells[1, 2].Value2,
                CreatDate = dateTimeNow,
                TotalCartons = (int)_ws.Cells[3, 2].Value2,
                TotalGrossWeight = Math.Round(_ws.Cells[5, 2].Value2 * 2.205, 2),
                TotalNetWeight = Math.Round(_ws.Cells[6, 2].Value2 * 2.205, 2),
                TotalVol = Math.Round(_ws.Cells[4, 2].Value2 * 35.315, 2),
                ContainerNumber = "UNKOWN",
                TotalPcs = 0,
                ActualReceivedPcs = 0,
                AvailablePcs = 0,
                Status = "Created"
            };

            _context.PreReceiveOrders.Add(newOrder);
            _context.SaveChanges();
        }
        #endregion

        //扫描并抽取每一页的Carton信息概览
        #region
        public void ExtractPackingList()
        {
            var list = new List<PurchaseOrderOverview>();
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

                list.Add(new PurchaseOrderOverview
                {
                    Available = 0,
                    ActualReceived = 0,
                    PurchaseOrder = _purchaseOrder.ToString(),
                    StyleNumber = _styleNumber,
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
                    InventoryCtn = 0,
                    InventoryPcs = 0
                });
            }

            _context.PurchaseOrderOverview.AddRange(list);
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
                var plInDb = _context.PurchaseOrderOverview.Include(s => s.PreReceiveOrder)
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
                        PurchaseOrderOverview = plInDb,
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
                            PurchaseOrderOverview = plInDb,
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
                        ReceivedDate = dateTimeNow
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
                            ReceivedDate = dateTimeNow
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

        //以LocationDetail为单位，从入库报告中抽取信息(与PackingList关联)
        public void ExtractLocationDetail(int preid, string po)
        {
            int n = 3;
            int countOfObj = 0;
            var preReceiveOrder = _context.PreReceiveOrders.Find(preid);
            var packingList = _context.PurchaseOrderOverview
                .SingleOrDefault(c => c.PreReceiveOrder.Id == preid && c.PurchaseOrder == po);
            var locationDetailList = new List<LocationDetail>();

            _ws = _wb.Worksheets[1];
            _purchaseOrder = _ws.Cells[1, 2] == null? "" : _ws.Cells[1, 2].Value2.ToString();

            if (_purchaseOrder != po)
            {
                Dispose();
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            while(_ws.Cells[n, 3].Value2 != null)
            {
                countOfObj += 1;
                n += 1;
            }

            for(int i = 0; i < countOfObj; i++)
            {
                locationDetailList.Add(new LocationDetail {
                    PurchaseOrderOverview = packingList,
                    PurchaseOrder = _purchaseOrder,
                    Style = _ws.Cells[3 + i, 1].Value2.ToString(),
                    Color = _ws.Cells[3 + i, 2].Value2.ToString(),
                    Size = _ws.Cells[3 + i, 3].Value2.ToString(),
                    NumberOfCartons = (int)_ws.Cells[3 + i, 4].Value2(),
                    Pcs = (int)_ws.Cells[3 + i, 5].Value2(),
                    Location = _ws.Cells[3 + i, 6].Value2(),
                    InboundDate = dateTimeNow
                });
            }

            _context.LocationDetails.AddRange(locationDetailList);
            _context.SaveChanges();

            Dispose();
        }
    }
}