using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.RichTextBoxUI.Menus;
using Telerik.Windows.Documents;
using Telerik.Windows.Documents.FormatProviders.Html;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.TextSearch;
using ContextMenuEventArgs = Telerik.Windows.Controls.RichTextBoxUI.Menus.ContextMenuEventArgs;


namespace KiwiGeekBible.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IBibleTranslation nkjv;
        private const bool VERSES_AS_PARAGRAPHS = true;

        public MainWindow()
        {
            InitializeComponent();

            nkjv = new BibleTranslation("NKJV");

            Telerik.Windows.Controls.RichTextBoxUI.ContextMenu contextMenu = (Telerik.Windows.Controls.RichTextBoxUI.ContextMenu)this.radRichTextBox.ContextMenu;
            contextMenu.Showing += richTextBox_OnContextMenuShowing;

        }

        private void richTextBox_OnContextMenuShowing(object? sender, ContextMenuEventArgs e)
        {

            // todo: remove edit hyperlink for auto links

            Debug.WriteLine(e.ContextMenuGroupCollection.ToString());
            foreach (ContextMenuGroup cmgc in e.ContextMenuGroupCollection)
            {
                if (cmgc.Type == ContextMenuGroupType.HyperlinkCommands)
                {
                    RadMenuItem toRemove = null;
                    foreach (RadMenuItem rmi in cmgc)
                    {
                        if (rmi.Command is Telerik.Windows.Documents.RichTextBoxCommands.OpenHyperlinkCommand)
                        {
                            toRemove = rmi;
                        }
                    }

                    if (toRemove != null)
                    {
                        cmgc.Remove(toRemove);
                    }
                }

            }

        }

        private void radRichTextBox_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Space || e.Key == Key.Enter)
            {

                double originalOffset = radRichTextBox.VerticalOffset;
                double originalY = this.radRichTextBox.Document.CaretPosition.Location.Y;
                using (DocumentPosition originalCaretPosition =
                    new DocumentPosition(this.radRichTextBox.Document.CaretPosition, true))
                {
                    Paragraph currentParagraph = radRichTextBox.Document.CaretPosition.GetCurrentParagraph();

                    // remove any extant links, so that we can reparse them
                    IEnumerable<HyperlinkRangeStart> links = this.radRichTextBox.Document.CaretPosition
                        .GetCurrentParagraph().EnumerateChildrenOfType<HyperlinkRangeStart>();
                    foreach (HyperlinkRangeStart link in links)
                    {
                        RadDocumentEditor documentEditor = new RadDocumentEditor(radRichTextBox.Document);
                        documentEditor.DeleteAnnotationRange(link);
                    }

                    foreach (Inline inline in currentParagraph.Inlines)
                    {

                        string currentBook = string.Empty;
                        uint currentChapter = 0;
                        uint currentVerse = 0;


                        DocumentTextSearch search = new DocumentTextSearch(radRichTextBox.Document);
                        foreach (TextRange textRange in search.FindAll("(\\b((1|2|3|I|II|III|i|ii|iii) )?[\\w']+\\b\\s\\d+:\\d*[\\d \\-:]{0,300}\\d)|(, ?\\d+:\\d+)[\\-]*\\d*[\\d \\-:]{0,300}\\d|(, ?\\d+)[\\-]*\\d*|(v\\d{1,3})"))
                        {

                            // textRange will represent a string in one of the following formats:
                            // 1: book chapter:verse (with the possible suffix of "-{chapter:}verse
                            // 2: , chapter:verse (with the possible suffix of "-{chapter:}verse
                            // 3: , verse  (with the possible suffix of "-{chapter:}verse
                            // 4: v[verse]



                            RadDocumentEditor documentEditor = new RadDocumentEditor(radRichTextBox.Document);
                            radRichTextBox.Document.Selection.Ranges.Clear();
                            radRichTextBox.Document.Selection.AddSelectionStart(textRange.StartPosition);
                            radRichTextBox.Document.Selection.AddSelectionEnd(textRange.EndPosition);
                            string currentWord = radRichTextBox.Document.Selection.GetSelectedText();

                            Debug.WriteLine($"trying to parse {currentWord}");

                            ParsedReference result = ProcessReference(currentWord, currentBook, currentChapter, currentVerse);

                            if (result.WasParsedSuccessfully)
                            {

                                HyperlinkInfo info = new HyperlinkInfo()
                                {
                                    NavigateUri = $"kgb://book={result.Book}&chap={result.Chapter}&verse={result.Verse}",
                                    Target = HyperlinkTargets.Blank,
                                    IsAnchor = false
                                };


                                documentEditor.InsertHyperlink(info);
                                currentBook = result.Book;
                                currentChapter = result.Chapter;
                                currentVerse = result.Verse;
                            }

                            radRichTextBox.Document.Selection.Ranges.Clear();

                        }
                    }

                    radRichTextBox.Document.CaretPosition.MoveToPosition(originalCaretPosition);
                    radRichTextBox.ActiveEditorPresenter.ScrollToVerticalOffset(originalOffset - originalY + this.radRichTextBox.Document.CaretPosition.Location.Y);
                }

            }

            radRichTextBox.UpdateEditorLayout(false);

        }

        ParsedReference ProcessReference(string input, string currentBook, uint currentChapter, uint currentVerse)
        {

            ParsedReference result = new ParsedReference();
            result.WasParsedSuccessfully = false;
            result.Book = currentBook;
            result.Chapter = currentChapter;
            result.Verse = currentVerse;

            try
            {

                // let's start by trying to parse a "v[verse]" reference

                Regex v1Regex = new Regex("(\\b(?<book>((1|2|3|I|II|III|i|ii|iii) )?[\\w']+)\\b\\s(?<chapter>\\d+):(?<verse>\\d*)[\\d \\-:]{0,300})");
                Regex v2Regex = new Regex("(, ?(?<chapter>\\d+):)(?<verse>\\d+)[\\-]*\\d*[\\d \\-:]{0,300}\\d");
                Regex v3Regex = new Regex("(, ?(?<verse>\\d+))[\\-]*\\d*");
                Regex v4Regex = new Regex("(v(?<verse>\\d{1,3}))");
                if (v1Regex.IsMatch(input))
                {
                    result.WasParsedSuccessfully = true;
                    Match m = v1Regex.Match(input);
                    result.Book = nkjv.GetBookCode(m.Groups["book"].Value).ToLower();
                    result.Chapter = uint.Parse(m.Groups["chapter"].Value);
                    result.Verse = uint.Parse(m.Groups["verse"].Value);

                }
                else if (v2Regex.IsMatch(input))
                {
                    result.WasParsedSuccessfully = true;
                    Match m = v2Regex.Match(input);
                    result.Chapter = uint.Parse(m.Groups["chapter"].Value);
                    result.Verse = uint.Parse(m.Groups["verse"].Value);
                }
                else if (v3Regex.IsMatch(input))
                {
                    result.WasParsedSuccessfully = true;
                    Match m = v3Regex.Match(input);
                    result.Verse = uint.Parse(m.Groups["verse"].Value);
                }
                else if (v4Regex.IsMatch(input))
                {
                    result.WasParsedSuccessfully = true;
                    Match m = v4Regex.Match(input);
                    result.Verse = uint.Parse(m.Groups["verse"].Value);
                }

                // validate that the book name is in the Bible we have.
                result.WasParsedSuccessfully &= nkjv.IsValidBookName(result.Book);

                // validate that the chapter number is valid in the bible we have.
                result.WasParsedSuccessfully &= nkjv.IsValidChapterNumber(result.Book, result.Chapter);

                result.WasParsedSuccessfully &=
                    nkjv.IsValidBookChapterVerseNumber(result.Book, result.Chapter, result.Verse);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"failed to parse {input}");
            }




            return result;
        }

        private void RadRichTextBox_OnHyperlinkClicked(object? sender, HyperlinkClickedEventArgs e)
        {
            if (e.URL.StartsWith("kgb://"))
            {
                // parse book, chapter and verse from link
                Regex urlRegex = new Regex("book=(?<book>.*)&chap=(?<chap>\\d*)&verse=(?<verse>\\d*)");
                Match m = urlRegex.Match(e.URL);
                RenderChapter(m.Groups["book"].Value, uint.Parse(m.Groups["chap"].Value));
                e.Handled = true;
            }


        }

        private void RenderChapter(string book, uint chapter)
        {

            // remove "<br />" from the start of any verses if we're in VERSES_AS_PARAGRAPHS mode, and not prose mode.
            // if we're in prose mode, we need to insert the verse label _after_ the line-break.

            List<Verse> chapterContents = nkjv.GetChapter(book, chapter);

            string chapterHTML = string.Empty;
            foreach (Verse verse in chapterContents)
            {

                if (!string.IsNullOrWhiteSpace(verse.SectionTitle))
                {
                    chapterHTML += $"<h2>{verse.SectionTitle}</h2>";
                }

                if (VERSES_AS_PARAGRAPHS)
                {
                    /* if (verse.VerseText.StartsWith("<br />"))
                     {
                         verse.VerseText = verse.VerseText.Substring(6,verse.VerseText.Length-6);
                     }*/

                    if (verse.StartsParagraph)
                    {
                        chapterHTML += "<p>";
                    }

                    if (verse.VerseText.StartsWith("<br />"))
                    {
                        verse.VerseText = verse.VerseText.Substring(6, verse.VerseText.Length - 6);
                        chapterHTML += $"<br /><b>{verse.VerseNumber}</b>&nbsp;{verse.VerseText}";
                    }
                    else
                    {
                        chapterHTML += $"<b>{verse.VerseNumber}</b>&nbsp;{verse.VerseText}";
                    }


                    if (verse.EndsParagraph)
                    {
                        chapterHTML += "</p>";
                    }
                }
                else
                {
                    chapterHTML += $"<p><b>{verse.VerseNumber}</b>&nbsp;{verse.VerseText}&nbsp;&nbsp;</p>";
                }


            }

            HtmlFormatProvider provider = new HtmlFormatProvider();
            RadDocument document = provider.Import(chapterHTML);

            BibleViewer.Document = document;

        }


    }
}
