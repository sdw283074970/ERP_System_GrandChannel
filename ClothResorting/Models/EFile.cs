using ClothResorting.Models.FBAModels;
using ClothResorting.Models.FBAModels.StaticModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class EFile
    {
        public EFile()
        {
            UploadDate = new DateTime(1900, 1, 1);
            SendDate = new DateTime(1900, 1, 1);
            Status = FBAStatus.Valid;
        }

        public int Id { get; set; } 

        public string FileName { get; set; }

        public string RootPath { get; set; }

        public string Status { get; set; }

        public string DiscardBy { get; set; }

        public string CustomizedFileName { get; set; }

        public DateTime UploadDate { get; set; }

        public string UploadBy { get; set; }

        public DateTime SendDate { get; set; }

        public FBAMasterOrder FBAMasterOrder { get; set; }

        public FBAShipOrder FBAShipOrder { get; set; }
    }
}