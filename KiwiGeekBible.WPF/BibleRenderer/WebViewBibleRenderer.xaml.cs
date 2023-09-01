using KiwiGeekBible.WPF.Classes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KiwiGeekBible.WPF.BibleRenderer
{

    public partial class WebViewBibleRenderer : UserControl, IBibleRenderer
    {

        private IBibleTranslation _bible;

        public WebViewBibleRenderer(IBibleTranslation bible)
        {
            InitializeComponent();
            _bible = bible;
        }

        public void HighlightScriptureRange(BibleReference start, BibleReference end)
        {
            throw new NotImplementedException();
        }

        public async Task NavigateToScripture(BibleReference reference)
        {
            RenderChapter(reference.Book, uint.Parse(reference.Chapter));
            await WebView2.ExecuteScriptAsync($"document.getElementById(\"v{reference.Verse}\").scrollIntoView();");
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await WebView2.EnsureCoreWebView2Async();
            WebView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        }

        private void RenderChapter(string book, uint chapter)
        {

            // remove "<br />" from the start of any verses if we're in VERSES_AS_PARAGRAPHS mode, and not prose mode.
            // if we're in prose mode, we need to insert the verse label _after_ the line-break.

            (string bookName, List<Verse> chapterContents) = _bible.GetChapter(book, chapter);

            string chapterHTML = string.Empty;

            // insert the book and chapter title
            chapterHTML += $"<h1>{bookName} {chapter}</h1>";

            foreach (Verse verse in chapterContents)
            {

                if (!string.IsNullOrWhiteSpace(verse.SectionTitle))
                {
                    chapterHTML += $"<h3>{verse.SectionTitle}</h3>";
                }

#if VERSES_AS_PROSE
                if (verse.StartsParagraph)
                {
                    chapterHTML += "<p>";
                }

                if (verse.VerseText.StartsWith("<br />"))
                {
                    verse.VerseText = verse.VerseText.Substring(6, verse.VerseText.Length - 6);
                    chapterHTML += $"<br /><b id=\"v{verse.VerseNumber}\">{verse.VerseNumber}</b>&nbsp;{verse.VerseText}";
                }
                else
                {
                    chapterHTML += $"<b id=\"v{verse.VerseNumber}\">{verse.VerseNumber}</b>&nbsp;{verse.VerseText}";
                }


                if (verse.EndsParagraph)
                {
                    chapterHTML += "</p>";
                }
#else
                string scrubbedVerse = verse.VerseText;
                if (scrubbedVerse.StartsWith("<br />"))
                {
                    scrubbedVerse = scrubbedVerse.Substring(6);
                }
                chapterHTML += $"<p id=\"v{verse.VerseNumber}\"><b>{verse.VerseNumber}</b>&nbsp;{scrubbedVerse}&nbsp;&nbsp;</p>";
#endif


            }

            string bodyHTML = "<html><head><style> h1 {text-align: center}</style></head><body>" + chapterHTML + "</body>";

            WebView2.NavigateToString(bodyHTML);

        }
    }
}
