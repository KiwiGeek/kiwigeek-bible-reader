using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiwiGeekBible.WPF
{
    public class Chapter
    {
        public uint ChapterNumber { get; set; }
        public List<Verse> Verses { get; set; } = new List<Verse>();

    }
}
