using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.ViewModels.Pages.Endpoints;

namespace Needlework.Net.ViewModels.MainWindow
{
    public partial class SchemaSearchDetailsViewModel : ObservableObject
    {
        public SchemaSearchDetailsViewModel(string key, Tab tab)
        {
            Tab = tab;
            Key = key;
        }

        public string Key { get; }

        public Tab Tab { get; }

        public string Document => Tab switch
        {
            Tab.LCU => "LCU",
            Tab.GameClient => "Game Client",
            _ => throw new System.NotImplementedException()
        };
    }
}
