using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiwiGeekBible.WPF.Classes
{
    public class BibleReference
    {
        public string Book { get; set; }
        public string Chapter { get; set; }
        public uint Verse { get; set; }
    }
}
