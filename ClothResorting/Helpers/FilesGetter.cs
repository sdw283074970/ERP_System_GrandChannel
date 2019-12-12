using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ClothResorting.Helpers
{
    public class FilesGetter
    {
        private string _filePath = "";

        public string FileName;

        //从httpRequest中获取文件并写入磁盘系统
        public string GetAndSaveSingleFileFromHttpRequest(string targetRootPath)
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files[0];

                if (httpPostedFile != null)
                {
                    var timeStamp = DateTime.Now.Year.ToString()
                        + DateTime.Now.Month.ToString()
                        + DateTime.Now.Day.ToString()
                        + DateTime.Now.Hour.ToString()
                        + DateTime.Now.Second.ToString()
                        + DateTime.Now.Millisecond.ToString();

                    string fileNameOnly = httpPostedFile.FileName.Split('\\').Last();

                    FileName = fileNameOnly;

                    _filePath = targetRootPath + timeStamp  + "-" + fileNameOnly;

                    httpPostedFile.SaveAs(_filePath);
                }
            }

            return _filePath;
        }

        public IEnumerable<string> GetAndSaveMultipleFileFromHttpRequest(string targetRootPath)
        {
            var pathList = new List<string>();

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var filesCount = HttpContext.Current.Request.Files.Count;

                for(var i = 0; i < filesCount; i++)
                {
                    var httpPostedFile = HttpContext.Current.Request.Files[i];

                    if (httpPostedFile != null)
                    {
                        var timeStamp = DateTime.Now.Year.ToString()
                            + DateTime.Now.Month.ToString()
                            + DateTime.Now.Day.ToString()
                            + DateTime.Now.Hour.ToString()
                            + DateTime.Now.Second.ToString()
                            + DateTime.Now.Millisecond.ToString();

                        string fileNameOnly = httpPostedFile.FileName.Split('\\').Last();

                        FileName = fileNameOnly;

                        _filePath = targetRootPath + timeStamp + "-" + fileNameOnly;

                        httpPostedFile.SaveAs(_filePath);

                        pathList.Add(_filePath);
                    }
                }
            }

            return pathList;
        }
    }
}