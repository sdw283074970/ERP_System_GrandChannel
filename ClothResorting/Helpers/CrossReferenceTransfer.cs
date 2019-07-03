using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothResorting.Models;

namespace ClothResorting.Helpers
{
    public class CrossReferenceTransfer
    {
        private IEnumerable<NameCrossReference> _references;

        public CrossReferenceTransfer(ApplicationDbContext context)
        {
            _references = context.NameCrossReferences.ToList();
        }

        public string TransName(string stringType, string originalStr)
        {
            var reference = _references.SingleOrDefault(x => x.StringType == stringType && x.OriginalString == originalStr);

            return reference == null ? originalStr : reference.Synonym;
        }
    }
}