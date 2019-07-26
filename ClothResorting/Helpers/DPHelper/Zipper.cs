using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers.DPHelper
{
    public class ZipperNameTransform : INameTransform
    {
        //public static void ZipFiles(string[] fileNames, string topPath, string zipedFileName, int compressionLevel, string password ="")
        //{
        //    using (ZipOutputStream zos = new ZipOutputStream(File.Open(zipedFileName, FileMode.OpenOrCreate)))
        //    {
        //        zos.SetLevel(compressionLevel);
        //        zos.Password = password;
        //        zos.SetComment("");

        //        foreach (string file in fileNames)
        //        {
        //            string fileName = string.Format("{0}/{1}", topPath, file);
        //            if (File.Exists(fileName))
        //            {
        //                FileInfo item = new FileInfo(fileName);
        //                FileStream fs = File.OpenRead(item.FullName);
        //                byte[] buffer = new byte[fs.Length];
        //                fs.Read(buffer, 0, buffer.Length);

        //                ZipEntry entry = new ZipEntry(item.Name);
        //                zos.PutNextEntry(entry);
        //                zos.Write(buffer, 0, buffer.Length);
        //            }
        //        }
        //    }
        //}

        public string TransformDirectory(string name)
        {
            return null;
        }

        public string TransformFile(string name)
        {
            return Path.GetFileName(name);
        }

        public string DownloadZippedFile(string[] files, string zipFileName, HttpContext context)
        {
            //MemoryStream ms = new MemoryStream();
            byte[] buffer = null;

            using (var ms = new MemoryStream())
            using (var fs = new FileStream(zipFileName, FileMode.OpenOrCreate, FileAccess.Write))
            using (ZipFile file = ZipFile.Create(ms))
            {
                file.BeginUpdate();
                file.NameTransform = new ZipNameTransform();

                foreach (var item in files)
                {
                    if (File.Exists(item))
                        file.Add(item);
                }

                file.CommitUpdate();
                buffer = ms.ToArray();
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
                buffer = null;

                return zipFileName;
                //buffer = new byte[ms.Length];
                //ms.Position = 0;
                //ms.Read(buffer, 0, buffer.Length); //读取文件内容(1次读ms.Length/1024M)
                //ms.Flush();
                //ms.Close();

                //context.Response.Clear();
                //context.Response.Buffer = true;
                //context.Response.ContentType = "appliction/octet-stream";
                //context.Response.AddHeader("content-disposition", "attachment; filename=" + zipFileName);
                //context.Response.BinaryWrite(buffer);
                //context.Response.Flush();
                //context.Response.Close();
                //context.Response.End();
            }
        }
    }
}