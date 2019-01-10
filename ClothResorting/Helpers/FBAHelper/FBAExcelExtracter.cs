using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAExcelExtracter
    {
        //全局变量
        #region
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime _dateTimeNow;
        private string _userName;
        #endregion

        //构造器
        #region
        public FBAExcelExtracter()
        {
            _context = new ApplicationDbContext();
            _dateTimeNow = DateTime.Now;
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        public FBAExcelExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _dateTimeNow = DateTime.Now;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }
        #endregion

        //抽取FBA通用PackingList模板
        public void ExtractFBAPackingListTemplate(string grandNumber)
        {
            var orderDetailsList = new List<FBAOrderDetail>();
            _ws = _wb.Worksheets[1];
            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber);

            var countOfOrderDetail = 0;
            var index = 2;

            var shipmentId = string.Empty;
            var amzRefId = string.Empty;
            var lotSize = string.Empty;
            var warehouseCode = string.Empty;
            var howToDeliver = string.Empty;
            var grossWeight = 0f;
            var cbm = 0f;
            var quantity = 0;
            var remark = string.Empty;

            //扫描有多少个OrderDetail对象(行数-1)
            while(_ws.Cells[index, 1].Value2 != null)
            {
                countOfOrderDetail++;
                index++;
            }

            for(int i = 0; i < countOfOrderDetail; i++)
            {
                shipmentId = _ws.Cells[i + 2, 1].Value2.ToString();
                amzRefId = _ws.Cells[i + 2, 2].Value2.ToString();
                lotSize = _ws.Cells[i + 2, 3].Value2.ToString();
                warehouseCode = _ws.Cells[i + 2, 4].Value2.ToString();
                howToDeliver = _ws.Cells[i + 2, 5].Value2.ToString();
                grossWeight = (float)_ws.Cells[i + 2, 6].Value2;
                cbm = (float)_ws.Cells[i + 2, 7].Value2;
                quantity = (int)_ws.Cells[i + 2, 8].Value2;
                remark = _ws.Cells[i + 2, 9].Value2.ToString();

                var orderDetail = new FBAOrderDetail();

                orderDetail.AssembleFirstStringPart(shipmentId, amzRefId, warehouseCode);
                orderDetail.AssembleSecontStringPart(lotSize, howToDeliver, remark);
                orderDetail.AssembleNumberPart(grossWeight, cbm, quantity);

                orderDetail.Container = masterOrderInDb.Container;
                orderDetail.GrandNumber = grandNumber;
                orderDetail.FBAMasterOrder = masterOrderInDb;

                orderDetailsList.Add(orderDetail);
            }

            _context.FBAOrderDetails.AddRange(orderDetailsList);
            _context.SaveChanges();
        }
    }
}