using System.Collections.Generic;
using System.Linq;
using KiwiGeekBible.WPF.Services;

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
            if (new[] { "GENESIS", "GEN.", "GE", "GN", "GEN", "GE.", "GN" }.Contains(input)) return "GEN";
            //if (new[] { "LEVITICUS", "LEV.", "LEV", "LE", "LE.", "LV.", "LV" }.Contains(input)) return "LEVITICUS";
            if (new[] { "PROVERBS", "PRO.", "PRO"}.Contains(input)) return "PRO";
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
