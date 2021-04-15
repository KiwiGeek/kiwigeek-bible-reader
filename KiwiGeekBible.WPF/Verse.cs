using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Media;

namespace KiwiGeekBible.WPF
{
    public class Verse
    {
        public uint VerseNumber { get; set; }
        public string SectionTitle { get; set; }
        public string VersePrefix { get; set; }
        public string VerseText { get; set; }
        public string VerseSuffix { get; set; }
        public bool StartsParagraph { get; set; }
        public bool EndsParagraph { get; set; }

        public Verse(uint verseNumber, string text)
        {
            VerseNumber = verseNumber;
            VerseText = text;
        }

        public Verse(uint verseNumber, string text, string sectionTitle)
        {
            SectionTitle = sectionTitle;
            VerseNumber = verseNumber;
            VerseText = text;
        }

        public Verse(uint verseNumber, string text, string sectionTitle, string versePrefix, string verseSuffix)
        {
            VersePrefix = versePrefix;
            VerseSuffix = verseSuffix;
            SectionTitle = sectionTitle;
            VerseNumber = verseNumber;
            VerseText = text;
        }

        public Verse()
        {

        }
    }
}
