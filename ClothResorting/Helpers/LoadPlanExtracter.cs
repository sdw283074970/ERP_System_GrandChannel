using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System.Diagnostics;

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
        #endregion

        //构造器
        public LoadPlanExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //读取xlsx文件,分析内容并抽取PickRequest对象
        public IEnumerable<PickRequest> GetPickRequestsFromXlsx()
        {
            var PickRequestList = new List<PickRequest>();
            var sumOfWs = _wb.Worksheets.Count;

            for (int i = 1; i <= sumOfWs; i++)
            {
                _ws = _wb.Worksheets[i];
                int sumOfSize = (int)_ws.Cells[1, 4].Value2;

                for(int j = 0; j < sumOfSize; j++)
                {
                    PickRequestList.Add(new PickRequest
                    {
                        PurchaseOrder = _ws.Cells[1, 2].Value2.ToString(),
                        Style = _ws.Cells[2, 2].Value2.ToString(),
                        Color = _ws.Cells[3, 2].Value2.ToString(),
                        Size = _ws.Cells[4, 2 + j].Value2,
                        TotalPcs = (int)_ws.Cells[5, 2 + j].Value2
                    });
                }
            }
            return PickRequestList;
        }

        //强行中止释放EXCEL进程
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