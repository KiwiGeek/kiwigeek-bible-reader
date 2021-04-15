namespace BibleComScraper.Classes
{
    class Chapter
    {
        public string Code { get; init; }
        public uint Index { get; init; }
        public string Name { get; init; }

        public Chapter(string code, uint index, string name)
        {
            Code = code;
            Index = index;
            Name = name;
        }
    }
}
