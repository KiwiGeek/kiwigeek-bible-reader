using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiwiGeekBible.WPF
{
    class ParsedReference
    {
       public bool WasParsedSuccessfully { get; set; }
       public string Book { get; set; }
       public uint Chapter { get; set; }
       public uint Verse { get; set; }

    }
}
