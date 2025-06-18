using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System;

namespace Needlework.Net.ViewModels.Pages.Schemas
{
    public partial class SchemaSearchDetailsViewModel : ObservableObject
    {
        private readonly SchemaPaneService _schemaPaneService;

        public SchemaSearchDetailsViewModel(Tab tab, PropertyClassViewModel vm, SchemaPaneService schemaPaneService)
        {
            _schemaPaneService = schemaPaneService;

            Tab = tab;
            Id = vm.Id;
        }

        public string Id { get; }

        public Tab Tab { get; }

        public string Document => Tab switch
        {
            Tab.LCU => "LCU",
            Tab.GameClient => "Game Client",
            _ => throw new NotImplementedException()
        };

        [RelayCommand]
        private void Display()
        {
            _schemaPaneService.Add(Id, Tab);
        }
    }
}
