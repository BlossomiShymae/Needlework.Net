using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System.Collections.Generic;

namespace Needlework.Net.ViewModels.MainWindow
{
    public partial class SchemaViewModel : ObservableObject
    {
        public SchemaViewModel(PropertyClassViewModel vm)
        {
            Id = vm.Id;
            PropertyFields = vm.PropertyFields;
            PropertyEnums = vm.PropertyEnums;
        }

        public string Id { get; }

        public List<PropertyFieldViewModel> PropertyFields { get; } = [];

        public List<PropertyEnumViewModel> PropertyEnums { get; } = [];
    }
}
