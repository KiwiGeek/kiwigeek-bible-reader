using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleComScraper.Classes
{
    class Verse
    {
        public string Book { get; set; }
        public string ChapterName { get; set; }
        public uint VerseNumber { get; set; }
        public string SectionTitle { get; set; }
        public string VersePrefix { get; set; }
        public string VerseText { get; set; }
        public string VerseSuffix { get; set; }
        public bool StartsParagraph { get; set; }
        public bool EndsParagraph { get; set; }
    }
}
