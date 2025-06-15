using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Needlework.Net.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public class PropertyClassViewModel : ObservableObject
{
    public PropertyClassViewModel(string id, IDictionary<string, OpenApiSchema> properties, IList<IOpenApiAny> enumValue)
    {
        List<PropertyFieldViewModel> propertyFields = [];
        List<PropertyEnumViewModel> propertyEnums = [];
        foreach ((var propertyName, var propertySchema) in properties)
        {
            var type = OpenApiHelpers.GetSchemaType(propertySchema);
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

    public string Id { get; }

    public List<PropertyFieldViewModel> PropertyFields { get; } = [];

    public List<PropertyEnumViewModel> PropertyEnums { get; } = [];
}