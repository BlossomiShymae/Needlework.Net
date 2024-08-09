using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.Desktop.ViewModels
{
    public class PropertyClassViewModel : ObservableObject
    {
        public string Id { get; }
        public IAvaloniaReadOnlyList<PropertyFieldViewModel> PropertyFields { get; } = new AvaloniaList<PropertyFieldViewModel>();
        public IAvaloniaReadOnlyList<PropertyEnumViewModel> PropertyEnums { get; } = new AvaloniaList<PropertyEnumViewModel>();

        public PropertyClassViewModel(string id, IDictionary<string, OpenApiSchema> properties, IList<IOpenApiAny> enumValue)
        {
            AvaloniaList<PropertyFieldViewModel> propertyFields = [];
            AvaloniaList<PropertyEnumViewModel> propertyEnums = [];
            foreach ((var propertyName, var propertySchema) in properties)
            {
                var type = OperationViewModel.GetSchemaType(propertySchema);
                var field = new PropertyFieldViewModel(propertyName, type);
                propertyFields.Add(field);
            }
            if (enumValue != null && enumValue.Any())
            {
                var propertyEnum = new PropertyEnumViewModel(enumValue);
                propertyEnums.Add(propertyEnum);
            }
            PropertyFields = propertyFields;
            PropertyEnums = propertyEnums;
            Id = id;
        }
    }
}