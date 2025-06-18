using Needlework.Net.Models;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System;
using System.Reactive.Subjects;

namespace Needlework.Net.Services
{
    public class SchemaPaneService
    {
        private readonly Subject<SchemaPaneItem> _schemaPaneItemsSubject = new();

        public IObservable<SchemaPaneItem> SchemaPaneItems { get { return _schemaPaneItemsSubject; } }

        public void Add(string key, Tab tab)
        {
            var schemaPaneItem = new SchemaPaneItem(key, tab);
            _schemaPaneItemsSubject.OnNext(schemaPaneItem);
        }
    }
}
