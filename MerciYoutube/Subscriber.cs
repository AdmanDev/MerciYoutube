namespace MerciYoutube
{
    // Represent subscriber object
    public class Subscriber
    {
        // Name of subsciber
        public string Name { get; private set; }
        // Subscriber icon url
        public string IconUrl { get; private set; }

        // Constructor
        public Subscriber(string name, string iconUrl)
        {
            this.Name = name;
            this.IconUrl = iconUrl;
        }
    }
}
