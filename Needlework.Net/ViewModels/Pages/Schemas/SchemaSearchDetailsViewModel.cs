using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System;

namespace Needlework.Net.ViewModels.Pages.Schemas
{
    public partial class SchemaSearchDetailsViewModel : ObservableObject
    {
        public SchemaSearchDetailsViewModel(Tab tab, PropertyClassViewModel vm)
        {
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
    }
}
