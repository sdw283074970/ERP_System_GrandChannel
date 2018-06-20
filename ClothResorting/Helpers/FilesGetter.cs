using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class FilesGetter
    {
        private string _filePath = "";

        //从httpRequest中获取文件并写入磁盘系统
        public string GetAndSaveFileFromHttpRequest(string targetRootPath)
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files[0];

                if (httpPostedFile != null)
                {
                    _filePath = targetRootPath + httpPostedFile.FileName;

                    httpPostedFile.SaveAs(_filePath);
                }
            }

            return _filePath;
        }
    }
}