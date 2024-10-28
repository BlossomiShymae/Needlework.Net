namespace Needlework.Net.ViewModels.Pages.Endpoints;

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