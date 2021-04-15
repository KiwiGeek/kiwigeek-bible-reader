
namespace BibleComScraper.Classes
{
    internal class Book
    {
        public uint Index { get; init; }
        public string Name { get; init; }
        public string Code { get; init; }

        public Book(uint index, string name, string code)
        {
            Index = index;
            Name = name;
            Code = code;
        }
    }
}
