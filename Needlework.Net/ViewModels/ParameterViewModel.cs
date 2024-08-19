using CommunityToolkit.Mvvm.ComponentModel;

namespace Needlework.Net.ViewModels
{
    public partial class ParameterViewModel : ObservableObject
    {
        public string Name { get; }
        public string Type { get; }
        public bool IsRequired { get; }
        [ObservableProperty] private string? _value = null;

        public ParameterViewModel(string name, string type, bool isRequired, string? value = null)
        {
            Name = name;
            Type = type;
            IsRequired = isRequired;
            Value = value;
        }
    }
}
