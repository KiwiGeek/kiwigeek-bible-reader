using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Media;

namespace KiwiGeekBible.WPF
{
    interface IBibleTranslation
    {
        string TranslationName { get; init; }
        string TranslationCode { get; init; }
        string GetBookCode(string input);
        bool IsValidBookName(string input);
        List<Verse> GetChapter(string book, uint chapter);
        bool IsValidChapterNumber(string book, uint chapter);
        uint GetBookChapterCount(string book);
        Verse GetVerse(string book, uint chapter, uint verse); 
        uint GetBookChapterVerseCount(string book, uint chapter);
        bool IsValidBookChapterVerseNumber(string book, uint chapter, uint verse);

    }
}
