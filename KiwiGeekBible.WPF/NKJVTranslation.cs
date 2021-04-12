using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace KiwiGeekBible.WPF
{
    class NKJVTranslation : IBibleTranslation
    {
        private Bible bible = new();

        public NKJVTranslation()
        {
            #region Genesis

            #region Genesis 1
            Chapter gen1 = new() { ChapterNumber = 1 };
            gen1.Verses.Add(new Verse(1, "In the beginning God created the heavens and the earth.", "The History of Creation") {StartsParagraph = true});
            gen1.Verses.Add(new Verse(2, "The earth was without form, and void; and darkness <i>was</i> on the face of the deep. And the Spirit of God was hovering over the face of the waters.") {EndsParagraph = true});
            
            gen1.Verses.Add(new Verse(3, "Then God said, \"Let there be light\"; and there was light.") {StartsParagraph=true});
            gen1.Verses.Add(new Verse(4, "And God saw the light, that <i>it was</i> good; and God divided the light from the darkness."));
            gen1.Verses.Add(new Verse(5, "God called the light Day, and the darkness He called Night. So the evening and the morning were the first day.") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(6, "Then God said, \"Let there be a firmament in the midst of the waters, and let it divide the waters from the waters.\"") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(7, "Thus God made the firmament, and divided the waters which <i>were</i> under the firmament from the waters which <i>were</i> above the firmament; and it was so."));
            gen1.Verses.Add(new Verse(8, "And God called the firmament Heaven. So the evening and the morning were the second day.") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(9, "Then God said, \"Let the waters under the heavens be gathered together into one place, and let the dry <i>land</i> appear\"; and it was so.") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(10, "And God called the dry <i>land</i> Earth, and the gathering together of the waters He called Seas. And God saw that <i>it was</i> good.") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(11, "Then God said, \"Let the earth bring forth grass, the herb <i>that</i> yields seed, <i>and</i> the fruit tree <i>that</i> yields fruit according to its kind, whose seed <i>is</i> in itself, on the earth\"; and it was so.") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(12, "And the earth brought forth grass, the herb <i>that</i> yields seed according to its kind, and the tree <i>that</i> yields fruit, whose seed <i>is</i> in itself according to its kind. And God saw that <i>it was</i> good."));
            gen1.Verses.Add(new Verse(13, "So the evening and the morning were the third day.") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(14, "Then God said, \"Let there be lights in the firmament of the heavens to divide the day from the night; and let them be for signs and seasons, and for days and years;") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(15, "and let them be for lights in the firmament of the heavens to give light on the earth\"; and it was so."));
            gen1.Verses.Add(new Verse(16, "Then God made two great lights: the greater light to rule the day, and the lesser light to rule the night. <i>He made</i> the stars also."));
            gen1.Verses.Add(new Verse(17, "God set them in the firmament of the heavens to give light on the earth,"));
            gen1.Verses.Add(new Verse(18, "and to rule over the day and over the night, and to divide the light from the darkness. And God saw that <i>it was</i> good."));
            gen1.Verses.Add(new Verse(19, "So the evening and the morning were the fourth day.") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(20, "Then God said, \"Let the waters abound with an abundance of living creatures, and let birds fly above the earth across the face of the firmament of the heavens.\"") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(21, "So God created great sea creatures and every living thing that moves, with which the waters abounded, according to their kind, and every winged bird according to its kind. And God saw that <i>it was</i> good."));
            gen1.Verses.Add(new Verse(22, "And God blessed them, saying, \"Be fruitful and multiply, and fill the waters in the seas, and let birds multiply on the earth.\""));
            gen1.Verses.Add(new Verse(23, "So the evening and the morning were the fifth day.") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(24, "Then God said, \"Let the earth bring forth the living creature according to its kind: cattle and creeping thing and beast of the earth, <i>each</i> according to its kind\"; and it was so.") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(25, "And God made the beast of the earth according to its kind, cattle according to its kind, and everything that creeps on the earth according to its kind. And God saw that <i>it was</i> good.") { EndsParagraph = true });

            gen1.Verses.Add(new Verse(26, "Then God said, \"Let Us make man in Our image, according to Our likeness; let them have dominion over the fish of the sea, over the birds of the air, and over the cattle, over all the earth and over every creeping thing that creeps on the earth.\"") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(27, "So God created man in His <i>own</i> image; in the image of God He created him; male and female He created them."));
            gen1.Verses.Add(new Verse(28, "Then God blessed them, and God said to them, \"Be fruitful and multiply; fill the earth and subdue it; have dominion over the fish of the sea, over the birds of the air, and over every living thing that moves on the earth.\"") { EndsParagraph = true });
            
            gen1.Verses.Add(new Verse(29, "And God said, \"See, I have given you every herb <i>that</i> yields seed which <i>is</i> on the face of all the earth, and every tree whose fruit yields seed; to you it shall be for food.") { StartsParagraph = true });
            gen1.Verses.Add(new Verse(30, "Also, to every beast of the earth, to every bird of the air, and to everything that creeps on the earth, in which <i>there is</i> life, <i>I have given</i> every green herb for food\"; and it was so."));
            gen1.Verses.Add(new Verse(31, "Then God saw everything that He had made, and indeed <i>it was</i> very good. So the evening and the morning were the sixth day.") { EndsParagraph = true });
            #endregion

            #region Genesis Assemble
            Book genesis = new() { Name = "Genesis", CanonicalCode = "GEN" };
            genesis.Chapters.Add(gen1);
            bible.Books.Add(genesis);
            #endregion

            #endregion

        }


        public string TranslationName()
        {
            return "New King James Version";
        }

        public string GetBookCode(string input)
        {
            input = input.ToUpper();
            if (new[] { "GENESIS", "GEN.", "GE", "GN", "GEN", "GE.", "GN" }.Contains(input)) return "GEN";
            //if (new[] { "LEVITICUS", "LEV.", "LEV", "LE", "LE.", "LV.", "LV" }.Contains(input)) return "LEVITICUS";
            return string.Empty;
        }

        public bool IsValidBookName(string input)
        {
            return !string.IsNullOrWhiteSpace(GetBookCode(input));
        }

        public List<Verse> GetChapter(string book, uint chapter)
        {
            return bible.Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper()).Chapters
                .First(c => c.ChapterNumber == chapter).Verses;
        }

        public bool IsValidChapterNumber(string book, uint chapter)
        {
            return (chapter <= GetBookChapterCount(book));
        }

        public uint GetBookChapterCount(string book)
        {
            return (uint)bible.Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper()).Chapters.Count;
        }

        public Verse GetVerse(string book, uint chapter, uint verse)
        {
            return bible
                .Books.First(b => b.CanonicalCode.ToUpper() == book.ToUpper())
                .Chapters.First(c => c.ChapterNumber == chapter)
                .Verses.First(f => f.VerseNumber == verse)
                ;
        }

        public uint GetBookChapterVerseCount(string book, uint chapter)
        {
            return (uint)bible
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
