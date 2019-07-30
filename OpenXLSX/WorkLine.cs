using System;

namespace OpenXLSX
{
    class WorkLine : IComparable<WorkLine>
    {
        public string SheetID { get; set; }
        public string Equipment { get; set; }
        public string Name { get; set; }
        public string Repairs { get; set; }
        public string Date { get; set; }
        public int Count { get; set; }
        public DateTime DateInDateTime { get; set; }
        public int CompareTo(WorkLine d)
        {

            return this.Date.CompareTo(d.Name);
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            WorkLine objAsPart = obj as WorkLine;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public bool Equals(WorkLine other)
        {
            if (other == null) return false;
            return (this.Date.Equals(other.Name));
        }
    }
}
