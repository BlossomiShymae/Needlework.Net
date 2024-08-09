namespace Needlework.Net.Desktop.ViewModels
{
    public class PropertyFieldViewModel
    {
        public string Name { get; }
        public string Type { get; }

        public PropertyFieldViewModel(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}