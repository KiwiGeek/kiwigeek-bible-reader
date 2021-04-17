using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using KiwiGeekBible.WPF.Services;
using Telerik.Windows.Controls.Spreadsheet.Controls;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace KiwiGeekBible.WPF
{
    class BibleTranslation : IBibleTranslation
    {

        public string TranslationCode { get; init; }
        public string TranslationName { get; init; }
        private BibleService _bibleService;
        private Bible _bible = new Bible();

        public BibleTranslation(string translationCode)
        {
            TranslationCode = translationCode;
            _bibleService = new BibleService(TranslationCode);

            //instantiate the book list
            _bible.Books = _bibleService.GetBooks();

            foreach (Book b in _bible.Books)
            {
                b.Chapters = _bibleService.GetChapters(b);
                foreach (Chapter c in b.Chapters)
                {
                    c.Verses = _bibleService.GetVerses(b, c);
                }
            }
        }


        public string GetBookCode(string input)
        {
            // todo: look these up, rather than hard coding them
            input = input.ToUpper();
            if (new[] { "GENESIS", "GEN.", "GE.", "GN.", "GEN", "GE", "GN" }.Contains(input)) return "GEN";
            if (new[] { "EXODUS", "EXO.", "EX.", "EXO", "EX" }.Contains(input)) return "EXO";
            if (new[] { "LEVITICUS", "LEV.", "LE.", "LV.", "LEV", "LE", "LV" }.Contains(input)) return "LEV";
            if (new[] { "NUMBERS", "NUM.", "NU.", "NM.", "NB.", "NUM", "NU", "NM", "NB" }.Contains(input)) return "NUM";
            if (new[] { "DEUTERONOMY", "DEUT.", "DT.", "DE.", "DEUT", "DT", "DE", "DEU", "DEU." }.Contains(input)) return "DEU";
            if (new[] { "JOSHUA", "JOSH.", "JOS.", "JSH.", "JOSH", "JOS", "JSH" }.Contains(input)) return "JOS";
            if (new[] { "JUDGES", "JUDG.", "JDG.", "JG.", "JDGS.", "JUDG", "JDG", "JG", "JDGS" }.Contains(input)) return "JDG";
            if (new[] { "RUTH", "RUTH.", "RTH.", "RU.", "RTH", "RU", "RUT", "RUT." }.Contains(input)) return "RUT";
            if (new[] { "I SAMUEL", "1 SAMUEL", "1 SAM.", "1 SM.", "1 SA.", "1 S.", "1 SAM", "1 SM", "1 SA", "1 S", "1SA" }.Contains(input)) return "1SA";
            if (new[] { "II SAMUEL", "2 SAMUEL", "2 SAM.", "2 SM.", "2 SA.", "2 S.", "2 SAM", "2 SM", "2 SA", "2 S", "2SA" }.Contains(input)) return "2SA";
            if (new[] { "I KINGS", "1 KINGS", "1 KGS.", "1 KIN.", "1 KI.", "1 K.", "1 KGS", "1 KIN", "1 KI", "1 K", "1KI" }.Contains(input)) return "1KI";
            if (new[] { "II KINGS", "2 KINGS", "2 KGS.", "2 KIN.", "2 KI.", "2 K.", "2 KGS", "2 KIN", "2 KI", "2 K", "2KI" }.Contains(input)) return "2KI";
            if (new[] { "I CHRONICLES", "1 CHRONICLES", "1 CHR.", "1 CH.", "1 CHRON.", "1CH.", "1 CHR", "1 CH", "1 CHRON", "1CH", "1CHRON", "1CHRON." }.Contains(input)) return "1CH";
            if (new[] { "II CHRONICLES", "2 CHRONICLES", "2 CHR.", "2 CH.", "2 CHRON.", "2CH.", "2 CHR", "2 CH", "2 CHRON", "2CH", "2CHRON", "2CHRON." }.Contains(input)) return "2CH";
            if (new[] { "EZRA", "EZR.", "EZ.", "EZR", "EZ" }.Contains(input)) return "EZR";
            if (new[] { "NEHEMIAH", "NEH.", "NE.", "NEH", "NE" }.Contains(input)) return "NEH";
            if (new[] { "ESTHER", "ESTH.", "EST.", "ES.", "ESTH", "EST", "ES" }.Contains(input)) return "EST";
            if (new[] { "JOB", "JB.", "JB" }.Contains(input)) return "JOB";
            if (new[] { "PSALMS", "PSALM", "PS", "PS.", "PSLM.", "PSLM", "PSA", "PSA.", "PSM.", "PSM" }.Contains(input)) return "PSA";
            if (new[] { "PROVERBS", "PROV.", "PROV", "PRV.", "PRV", "PR", "PR.", "PRO.", "PRO" }.Contains(input)) return "PRO";
            if (new[] { "ECCLESIASTES", "ECCLES.", "ECCLES", "ECCLE.", "ECCLE", "ECC.", "ECC", "EC.", "EC" }.Contains(input)) return "ECC";
            if (new[] { "SONG OF SOLOMON", "SONG", "SNG", "SONG.", "SNG.", "SONG OF SONGS" }.Contains(input)) return "SNG";
            if (new[] { "ISAIAH", "ISA.", "ISA", "IS.", "IS" }.Contains(input)) return "ISA";
            if (new[] { "JEREMIAH", "JER.", "JER", "JE.", "JE", "JR.", "JR" }.Contains(input)) return "JER";
            if (new[] { "LAMENTATIONS", "LAM.", "LAM", "LA.", "LA" }.Contains(input)) return "LAM";
            if (new[] { "EZEKIEL", "EZEK.", "EZEK", "EZE.", "EZE", "EZK.", "EZK" }.Contains(input)) return "EZK";
            if (new[] { "DANIEL", "DAN.", "DAN", "DA.", "DA", "DN.", "DN" }.Contains(input)) return "DAN";
            if (new[] { "HOSEA", "HOS.", "HOS", "HO", "HO." }.Contains(input)) return "HOS";
            if (new[] { "JOEL", "JL.", "JL", "JOL", "JOL." }.Contains(input)) return "JOL";
            if (new[] { "AMOS", "AM", "AM.", "AMO.", "AMO" }.Contains(input)) return "AMO";
            if (new[] { "OBADIAH", "OBA", "OBA.", "OBD.", "OBD" }.Contains(input)) return "OBA";
            if (new[] { "JONAH", "JON.", "JON", "JHN.", "JHN." }.Contains(input)) return "JON";
            if (new[] { "MICAH", "MIC.", "MIC", "MC", "MC." }.Contains(input)) return "MIC";
            if (new[] { "NAHUM", "NAH.", "NAH", "NA.", "NA", "NAM.", "NAM"}.Contains(input)) return "NAM";
            if (new[] { "HABAKKUK", "HAB.", "HAB" }.Contains(input)) return "HAB";
            if (new[] { "ZEPHANIAH", "ZEPH.", "ZEPH", "ZEP.", "ZEP", "ZP.", "ZP" }.Contains(input)) return "ZEP";
            if (new[] { "HAGGAI", "HAG.", "HAG", "HG.", "HG" }.Contains(input)) return "HAG";
            if (new[] { "ZECHARIAH", "ZECH.", "ZECH", "ZEC.", "ZEC", "ZC.", "ZC" }.Contains(input)) return "ZEC";
            if (new[] { "MALACHI", "MAL.", "MAL", "ML", "ML." }.Contains(input)) return "MAL";
            if (new[] { "MATTHEW", "MATT.", "MATT", "MAT.", "MAT", "MT.", "MT" }.Contains(input)) return "MAT";
            if (new[] { "MARK", "MK.", "MK", "MRK.", "MRK" }.Contains(input)) return "MRK";
            if (new[] { "LUKE", "LUK.", "LUK", "LK.", "LK" }.Contains(input)) return "LUK";
            if (new[] { "JOHN", "JHN.", "JHN", "JN.", "JN"}.Contains(input)) return "JHN";
            if (new[] { "ACTS", "ACT", "ACT."}.Contains(input)) return "ACT";
            if (new[] { "ROMANS", "ROM.", "ROM","RO.", "RO", "RM.", "RM"}.Contains(input)) return "ROM";
            if (new[] { "I CORINTHIANS", "1 CORINTHIANS", "1 COR.", "1 COR", "1COR.", "1COR.", "1CO.", "1CO", "1 CO.", "1 CO"}.Contains(input)) return "1CO";
            if (new[] { "II CORINTHIANS", "2 CORINTHIANS", "2 COR.", "2 COR", "2COR.", "2COR.", "2CO.", "2CO", "2 CO.", "2 CO" }.Contains(input)) return "2CO";
            if (new[] { "GALATIANS", "GAL.", "GAL", "GA.", "GA"}.Contains(input)) return "GAL";
            if (new[] { "EPHESIANS","EHP.", "EPH", "EPHES.", "EHPES" }.Contains(input)) return "EPH";
            if (new[] { "PHILIPPIANS", "PHIL.", "PHIL", "PHP.", "PHP", "PP.", "PP"}.Contains(input)) return "PHP";
            if (new[] { "COLOSSIANS", "COL.", "COL"}.Contains(input)) return "COL";
            if (new[] { "I THESSALONIANS", "1 THESSALONIANS", "1 THESS.", "1 THESS", "1THESS.", "1THESS", "1 THES.", "1 THES", "1THES", "1THES.", "1 TH.", "1 TH", "1TH.", "1TH"}.Contains(input)) return "1TH";
            if (new[] { "II THESSALONIANS", "2 THESSALONIANS", "2 THESS.", "2 THESS", "2THESS.", "2THESS", "2 THES.", "2 THES", "2THES", "2THES.", "2 TH.", "2 TH", "2TH.", "2TH" }.Contains(input)) return "2TH";
            if (new[] { "I TIMOTHY", "1 TIMOTHY", "1 TIM.", "1 TIM", "1 TI.", "1 TI", "1TIM.", "1TIM", "1TI.", "1TI" }.Contains(input)) return "1TI";
            if (new[] { "II TIMOTHY", "2 TIMOTHY", "2 TIM.", "2 TIM", "2 TI.", "2 TI", "2TIM.", "2TIM", "2TI.", "2TI" }.Contains(input)) return "2TI";
            if (new[] { "TITUS", "TIT.", "TIT", "TI.", "TI"}.Contains(input)) return "TIT";
            if (new[] { "PHILEMON", "PHILEM.", "PHILEM", "PHM.", "PHM", "PM.", "PM"}.Contains(input)) return "PHM";
            if (new[] { "HEBREWS", "HEB.", "HEB"}.Contains(input)) return "HEB";
            if (new[] { "JAMES", "JAS.", "JAS", "JM.", "JM"}.Contains(input)) return "JAS";
            if (new[] { "I PETER", "1 PETER", "1 PET.", "1 PET", "1 PE.", "1 PE", "1 PT.", "1 PT", "1 P.", "1 P", "1PET.", "1PET", "1PE.", "1PE", "1PT.", "1PT", "1P.", "1P" }.Contains(input)) return "1PE";
            if (new[] { "II PETER", "2 PETER", "2 PET.", "2 PET", "2 PE.", "2 PE", "2 PT.", "2 PT", "2 P.", "2 P", "2PET.", "2PET", "2PE.", "2PE", "2PT.", "2PT", "2P.", "2P" }.Contains(input)) return "2PE";
            if (new[] { "I JOHN", "1 JOHN", "1 JN.", "1 JN", "1 JHN.", "1 JHN", "1 J.", "1 J", "1JN.", "1JN", "1JHN.", "1JHN", "1J.", "1J" }.Contains(input)) return "1JN";
            if (new[] { "II JOHN", "2 JOHN", "2 JN.", "2 JN", "2 JHN.", "2 JHN", "2 J.", "2 J", "2JN.", "2JN", "2JHN.", "2JHN", "2J.", "2J" }.Contains(input)) return "2JN";
            if (new[] { "III JOHN", "3 JOHN", "3 JN.", "3 JN", "3 JHN.", "3 JHN", "3 J.", "3 J", "3JN.", "3JN", "3JHN.", "3JHN", "3J.", "3J" }.Contains(input)) return "3JN";
            if (new[] { "JUDE", "JUD.", "JUD", "JD.", "JD"}.Contains(input)) return "JUD";
            if (new[] { "REVELATION", "REV.", "REV", "RV.", "RV"}.Contains(input)) return "REV";
            return string.Empty;
        }

        public bool IsValidBookName(string input)
        {
            return !string.IsNullOrWhiteSpace(GetBookCode(input));
        }

        public List<Verse> GetChapter(string book, uint chapter)
        {
            return _bible.Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper()).Chapters
                .First(c => c.ChapterNumber == chapter).Verses;
        }

        public bool IsValidChapterNumber(string book, uint chapter)
        {
            return (chapter <= GetBookChapterCount(book));
        }

        public uint GetBookChapterCount(string book)
        {
            return (uint)_bible.Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper()).Chapters.Count;
        }

        public Verse GetVerse(string book, uint chapter, uint verse)
        {
            return _bible
                    .Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper())
                    .Chapters.First(c => c.ChapterNumber == chapter)
                    .Verses.First(f => f.VerseNumber == verse);
        }

        public uint GetBookChapterVerseCount(string book, uint chapter)
        {
            return (uint)_bible
                .Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper())
                .Chapters.First(c => c.ChapterNumber == chapter)
                .Verses.Count;
        }

        public bool IsValidBookChapterVerseNumber(string book, uint chapter, uint verse)
        {
            return (verse <= GetBookChapterVerseCount(book, chapter));
        }
    }
}
