using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiwiGeekBible.WPF
{
    public class Book
    {
        public string Name { get; init; }
        public string CanonicalCode { get; init; }
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}
