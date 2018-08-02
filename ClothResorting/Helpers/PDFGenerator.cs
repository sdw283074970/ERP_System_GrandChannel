using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace ClothResorting.Helpers
{
    public class PDFGenerator
    {
        public void GeneratePdf(string pdfPath)
        {
            var fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var doc = new Document();
            var writer = PdfWriter.GetInstance(doc, fs);
            doc.Open();
            doc.Add(new Paragraph("Hello World!"));
            doc.Close();
        }
    }
}