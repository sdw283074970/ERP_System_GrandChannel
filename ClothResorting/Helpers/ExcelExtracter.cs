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
        #endregion

        //PackingList全局变量
        #region
        private string _purchaseOrderNumber;
        private string _styleNumber;
        private double _packedCartons;
        private double _netWeight;
        private double _grossWeight;
        private double _cFT;
        private double _numberOfDemension;
        private double _numberOfSizeRatio;
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
        public void CreateSilkIconPreReceiveOrderAndOverView()
        {
            _ws = _wb.Worksheets[1];
            

            //建立一个PreReceiveOrder对象
            var newOrder = new SilkIconPreReceiveOrder
            {
                Available = 0,
                ActualReceived = 0,
                CustomerName = _ws.Cells[1, 2].Value2,
                CreatDate = DateTime.Today,
                TotalCartons = (int)_ws.Cells[3, 2].Value2,
                TotalGrossWeight = Math.Round(_ws.Cells[5, 2].Value2 * 2.205, 2),
                TotalNetWeight = Math.Round(_ws.Cells[6, 2].Value2 * 2.205, 2),
                TotalVol = Math.Round(_ws.Cells[4, 2].Value2 * 35.315, 2),
                ContainerNumber = "UNKOWN",
                TotalPcs = 0,
                ActualReceivedPcs = 0,
                AvailablePcs = 0
            };

            _context.SilkIconPreReceiveOrders.Add(newOrder);
            _context.SaveChanges();
        }
        #endregion

        //扫描并抽取每一页的Carton信息概览
        #region
        public void ExtractSilkIconPackingList()
        {
            var list = new List<SilkIconPackingList>();
            var preReceiveOrderInDb = _context.SilkIconPreReceiveOrders     //获取刚建立的PreReceiveOrder
                .OrderByDescending(c => c.Id)
                .First();

            //开始扫描每一页
            for (int k = 2; k <= _wb.Worksheets.Count; k++)
            {
                var demensions = new List<Measurement>();
                int n = 11;
                int t;

                _ws = _wb.Worksheets[k];
                _purchaseOrderNumber = _ws.Cells[1, 2].Value2.ToString();
                _styleNumber = _ws.Cells[2, 2].Value2.ToString();
                _packedCartons = _ws.Cells[3, 2].Value2;
                _cFT = _ws.Cells[4, 2].Value2;
                _grossWeight = _ws.Cells[5, 2].Value2;
                _netWeight = _ws.Cells[6, 2].Value2;
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
                        PurchaseOrderNumber = _purchaseOrderNumber.ToString()
                    });
                }

                list.Add(new SilkIconPackingList
                {
                    Available = 0,
                    ActualReceived = 0,
                    PurchaseOrderNumber = _purchaseOrderNumber.ToString(),
                    StyleNumber = _styleNumber,
                    NetWeight = Math.Round(_netWeight * 2.205, 2),
                    GrossWeight = Math.Round(_grossWeight * 2.205, 2),
                    CFT = Math.Round(_cFT * 35.315, 2),
                    Date = null,
                    NumberOfSizeRatio = (int)_numberOfSizeRatio,
                    PackedCartons = (int)_packedCartons,
                    NumberOfDemension = (int)_numberOfDemension,
                    TotalMeasurements = demensions == null ? null : demensions,
                    SilkIconPreReceiveOrder = preReceiveOrderInDb,
                    TotalPcs = 0,
                    ActualReceivedPcs = 0,
                    AvailablePcs = 0
                });
            }

            _context.SilkIconPackingLists.AddRange(list);
            _context.SaveChanges();
        }
        #endregion

        //抽取SilkIcon公司发来的Excel表格中的CartonDetails
        #region
        public void ExtractSilkIconCartonDetails()
        {

            //扫描每一张Sheet
            for (int w = 2; w <= _wb.Worksheets.Count; w++)
            {
                var list = new List<SilkIconCartonDetail>();
                var cartonBreakDownList = new List<CartonBreakDown>();
                var cartonClassCount = 0;
                int i = 11;
                int j = 3;
                _ws = _wb.Worksheets[w];

                var preReceiveOrderInDb = _context.SilkIconPreReceiveOrders     //获取刚建立的PreReceiveOrder
                    .OrderByDescending(c => c.Id)
                    .First();

                //确定有多少行Carton需要扫描
                while (_ws.Cells[i, j].Value2 != null)
                {
                    i += 1;
                    cartonClassCount += 1;
                }

                i = 11;

                //确定SizeRatio的数量
                _numberOfSizeRatio = (int)_ws.Cells[2, 5].Value2;

                //找到与这一页CartonDetail相关的PackingList
                _purchaseOrderNumber = _ws.Cells[1, 2].Value2.ToString();
                var plInDb = _context.SilkIconPackingLists.Include(s => s.SilkIconPreReceiveOrder)
                    .SingleOrDefault(s => s.PurchaseOrderNumber == _purchaseOrderNumber
                        && s.SilkIconPreReceiveOrder.Id == preReceiveOrderInDb.Id);

                //为每一个CartonDetail扫描数据
                for (int c = 0; c < cartonClassCount; c++)
                {
                    var sizeList = new List<SizeRatio>();

                    for (int n = 0; n < _numberOfSizeRatio; n++)
                    {
                        sizeList.Add(new SizeRatio
                        {
                            Count = _ws.Cells[i, 18 + n].Value2 == null ? 0 : (int)_ws.Cells[i, 18 + n].Value2,
                            SizeName = _ws.Cells[10, 18 + n].Value2
                        });
                    }

                    //读取扫描结果
                    _style = _ws.Cells[i, j - 2].Value2.ToString();
                    _color = _ws.Cells[i, j - 1].Value2;
                    _cartonNumberRangeFrom = _ws.Cells[i, j].Value2;
                    _cartonNumberRangeTo = _ws.Cells[i, j + 2].Value2;
                    _sumOfCarton = _ws.Cells[i, j + 3].Value2;
                    _runCode = _ws.Cells[i, j + 5].Value2;
                    _dimension = _ws.Cells[i, j + 7].Value2;
                    _netWeightPerCartons = _ws.Cells[i, j + 10].Value2;
                    _grossWeightPerCartons = _ws.Cells[i, j + 11].Value2;
                    _pcsPerCartons = _ws.Cells[i, j + 12].Value2;
                    _totalPcs = _ws.Cells[i, j + 13].Value2;
                    _style = _ws.Cells[i, j - 2].Value2.ToString();

                    var carton = new SilkIconCartonDetail
                    {
                        ActualReceivedPcs = 0,
                        AvailablePcs = 0,
                        Available = 0,
                        ActualReceived = 0,
                        Style = _style,
                        Color = _color,
                        PurchaseOrderNumber = _purchaseOrderNumber.ToString(),
                        CartonNumberRangeFrom = (int)_cartonNumberRangeFrom,
                        CartonNumberRangeTo = (int)_cartonNumberRangeTo,
                        SumOfCarton = (int)_sumOfCarton,
                        RunCode = _runCode == null ? "" : _runCode,
                        Dimension = _dimension == null ? "" : _dimension,
                        NetWeightPerCartons = _netWeightPerCartons == null ? 0 : Math.Round((double)_netWeightPerCartons * 2.205, 2),
                        GrossWeightPerCartons = _grossWeightPerCartons == null ? 0 : Math.Round((double)_grossWeightPerCartons * 2.205, 2),
                        PcsPerCartons = _pcsPerCartons == null ? 0 : (int)_pcsPerCartons,
                        TotalPcs = _totalPcs == null ? 0 : (int)_totalPcs,
                        SilkIconPackingList = plInDb,
                        SizeRatios = sizeList,
                        Location = "N/A"
                    };

                    //顺便添加CartonBreakDown信息到CartonBreakDown表中
                    var sumOfCartons = (int)_sumOfCarton;

                    for (int k = 0; k < sizeList.Count; k++)
                    {
                        //即使数量是0也要记录，以防一个箱子中塞入额外不同尺寸pcs的情况
                        var cartonBreakDown = new CartonBreakDown
                        {
                            PurchaseNumber = _purchaseOrderNumber.ToString(),
                            Style = _style,
                            Color = _color,
                            CartonNumberRangeFrom = (int)_cartonNumberRangeFrom,
                            CartonNumberRangeTo = (int)_cartonNumberRangeTo,
                            RunCode = _runCode == null ? "" : _runCode,
                            Size = sizeList[k].SizeName,
                            ForecastPcs = sizeList[k].Count * sumOfCartons,
                            ActualPcs = 0,
                            AvailablePcs = 0,
                            SilkIconPackingList = plInDb,
                            Location = "N/A",
                            SilkIconCartonDetail = carton
                        };
                        cartonBreakDownList.Add(cartonBreakDown);
                    }

                    list.Add(carton);
                    i += 1;
                }

                _context.CartonBreakDowns.AddRange(cartonBreakDownList);
                _context.SilkIconCartonDetails.AddRange(list);
                _context.SaveChanges();
            }
        }
        #endregion

        public void Dispose()
        {
            var excelProcs = Process.GetProcessesByName("EXCEL");

            foreach (var procs in excelProcs)
            {
                procs.Kill();
            }
        }
    }
}