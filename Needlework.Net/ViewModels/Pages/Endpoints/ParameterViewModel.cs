using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class ParameterViewModel : ReactiveObject
{
    public ParameterViewModel(string name, string type, bool isRequired, string? value = null)
    {
        Name = name;
        Type = type;
        IsRequired = isRequired;
        Value = value;
    }

    public string Name { get; }

    public string Type { get; }

    public bool IsRequired { get; }

    [Reactive]
    private string? _value;
}
