using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System.Diagnostics;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class LoadPlanExtracter
    {
        //全局变量
        #region
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime timeNow = DateTime.Now;
        #endregion

        //构造器
        public LoadPlanExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
            _ws = _wb.Worksheets[1];

        }

        //读取xlsx文件,分析内容并抽取PickRequest对象
        public IEnumerable<PickRequest> GetPickRequestsFromXlsx()
        {
            var PickRequestList = new List<PickRequest>();
            var sumOfGroups = 0;
            var startRow = 1;
            var n = 1;

            //计算有多少groups
            while (n > 0)
            {
                var cpt_1 = _ws.Cells[n, 1].Value2;
                var cpt_2 = _ws.Cells[n + 1, 1].Value2;

                if (cpt_1 == null)
                {
                    sumOfGroups += 1;
                }

                if (cpt_1 == null && cpt_2 == null)
                {
                    break;
                }
                n += 1;
            }

            //遍历每一组，为每一组生成一个PickRequest对象放入pickRequestList中
            for (int i = 1; i <= sumOfGroups; i++)
            {
                //扫描每组有多少个size
                int sumOfSize = 0;
                int k = 0;

                while(_ws.Cells[startRow + 3 , 2 + k].Value != null)
                {
                    sumOfSize += 1;
                    k += 1;
                }

                for (int j = 0; j < sumOfSize; j++)
                {
                    PickRequestList.Add(new PickRequest
                    {
                        PurchaseOrder = _ws.Cells[startRow, 2].Value2 == null ? "" : _ws.Cells[startRow, 2].Value2.ToString(),
                        OrderPurchaseOrder = _ws.Cells[startRow, 6].Value2 == null ? "" : _ws.Cells[startRow, 6].Value2.ToString(),
                        Style = _ws.Cells[startRow + 1, 2].Value2 == null ? "" : _ws.Cells[startRow + 1, 2].Value2.ToString(),
                        Color = _ws.Cells[startRow + 2, 2].Value2 == null ? "" : _ws.Cells[startRow + 2, 2].Value2.ToString(),
                        Size = _ws.Cells[startRow + 3, 2 + j].Value2,
                        TargetPcs = (int)_ws.Cells[startRow + 4, 2 + j].Value2
                    });
                }

                startRow += 6;
            }

            return PickRequestList;
        }
    }
}