using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public class PropertyClassViewModel : ReactiveObject
{
    public PropertyClassViewModel(string id, IDictionary<string, OpenApiSchema> properties, IList<IOpenApiAny> enumValue)
    {
        List<PropertyFieldViewModel> propertyFields = [];
        List<PropertyEnumViewModel> propertyEnums = [];
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
        PropertyFields = [.. propertyFields];
        PropertyEnums = [.. propertyEnums];
        Id = id;
    }

    public string Id { get; }

    public ObservableCollection<PropertyFieldViewModel> PropertyFields { get; } = [];

    public ObservableCollection<PropertyEnumViewModel> PropertyEnums { get; } = [];
}