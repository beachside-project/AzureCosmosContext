namespace ConsoleAppSample
{
    public class AttrItemSample
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Something => $"Id={Id}, Name={Name}";

        public AttrItemSample(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}