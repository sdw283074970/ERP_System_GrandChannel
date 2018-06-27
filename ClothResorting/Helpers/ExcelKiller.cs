using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class ExcelKiller
    {
        public void Dispose()
        {
            var excelProcs = Process.GetProcessesByName("EXCEL");

            foreach (var procs in excelProcs)
            {
                procs.Kill();
                procs.WaitForExit();
            }
        }
    }
}