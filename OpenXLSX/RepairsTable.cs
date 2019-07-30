using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenXLSX
{
    class RepairsTable
    {
        public string SheetID { get; set; }
        public string Equipment { get; set; }
        public string Name { get; set; }
        public string Repairs { get; set; }
        public DateTime DateInDateTime { get; set; }
    }
}
