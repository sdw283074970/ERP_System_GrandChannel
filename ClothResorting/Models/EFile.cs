using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class EFile
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public DateTime UploadDate { get; set; }

        public string UploadBy { get; set; }

        public FBAMasterOrder FBAMasterOrder { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }
    }
}