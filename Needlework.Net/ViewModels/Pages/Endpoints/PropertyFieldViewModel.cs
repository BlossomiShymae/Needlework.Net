namespace Needlework.Net.ViewModels.Pages.Endpoints;

public class PropertyFieldViewModel
{
    public PropertyFieldViewModel(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public string Type { get; }
}