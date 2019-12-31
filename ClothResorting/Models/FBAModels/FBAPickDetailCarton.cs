using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.FBAModels
{
    public class FBAPickDetailCarton
    {
        public int Id { get; set; }

        public int PickCtns { get; set; }

        public string LabelFiles { get; set; }

        public FBACartonLocation FBACartonLocation { get; set; }

        public FBAPickDetail FBAPickDetail { get; set; }

        public FBAPickDetailCarton()
        {
            LabelFiles = "[]";
        }
    }
}