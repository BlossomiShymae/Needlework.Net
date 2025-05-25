using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.OpenApi.Models;
using Needlework.Net.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class OperationViewModel : ObservableObject
{
    public string Summary { get; }
    public string Description { get; }
    public string ReturnType { get; }
    public bool IsRequestBody { get; }
    public string? RequestBodyType { get; }
    public IAvaloniaReadOnlyList<PropertyClassViewModel> RequestClasses { get; }
    public IAvaloniaReadOnlyList<PropertyClassViewModel> ResponseClasses { get; }
    public IAvaloniaReadOnlyList<ParameterViewModel> PathParameters { get; }
    public IAvaloniaReadOnlyList<ParameterViewModel> QueryParameters { get; }
    public string? RequestTemplate { get; }

    public OperationViewModel(OpenApiOperation operation, Models.Document lcuSchemaDocument)
    {
        Summary = operation.Summary ?? string.Empty;
        Description = operation.Description ?? string.Empty;
        IsRequestBody = operation.RequestBody != null;
        ReturnType = GetReturnType(operation.Responses);
        RequestClasses = GetRequestClasses(operation.RequestBody, lcuSchemaDocument);
        ResponseClasses = GetResponseClasses(operation.Responses, lcuSchemaDocument);
        PathParameters = GetParameters(operation.Parameters, ParameterLocation.Path);
        QueryParameters = GetParameters(operation.Parameters, ParameterLocation.Query);
        RequestBodyType = GetRequestBodyType(operation.RequestBody);
        RequestTemplate = GetRequestTemplate(operation.RequestBody, lcuSchemaDocument);
    }

    private string? GetRequestTemplate(OpenApiRequestBody? requestBody, Document lcuSchemaDocument)
    {
        var requestClasses = GetRequestClasses(requestBody, lcuSchemaDocument);
        if (requestClasses.Count == 0)
        {
            var type = GetRequestBodyType(requestBody);
            if (type == null) return null;
            return GetRequestDefaultValue(type);
        }

        var template = CreateTemplate(requestClasses);
        return JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(string.Join(string.Empty, template)), App.JsonSerializerOptions);
    }

    private List<string> CreateTemplate(AvaloniaList<PropertyClassViewModel> requestClasses)
    {
        if (requestClasses.Count == 0) return [];
        List<string> template = [];
        template.Add("{");

        var rootClass = requestClasses.First();
        if (rootClass.PropertyEnums.Any()) return [rootClass.PropertyEnums.First().Values];
        var propertyFields = rootClass.PropertyFields;
        for (int i = 0; i < propertyFields.Count; i++)
        {
            template.Add($"\"{propertyFields[i].Name}\"");
            template.Add(":");
            template.Add($"#{propertyFields[i].Type}");

            if (i == propertyFields.Count - 1) template.Add("}");
            else template.Add(",");
        }

        for (int i = 0; i < template.Count; i++)
        {
            var type = template[i];
            if (!type.Contains("#")) continue;

            var foundClass = requestClasses.Where(c => c.Id == type.Replace("#", string.Empty));
            if (foundClass.Any())
            {
                if (foundClass.First().PropertyEnums.Any())
                {
                    template[i] = string.Join(string.Empty, CreateTemplate([.. foundClass]));
                }
                else
                {
                    AvaloniaList<PropertyClassViewModel> classes = [.. requestClasses];
                    classes.Remove(rootClass);
                    template[i] = string.Join(string.Empty, CreateTemplate(classes));
                }
            }
            else
            {
                template[i] = GetRequestDefaultValue(type);
            }
        }

        return template;
    }

    private static string GetRequestDefaultValue(string type)
    {
        var defaultValue = string.Empty;
        if (type.Contains("[]")) defaultValue = "[]";
        else if (type.Contains("string")) defaultValue = "\"\"";
        else if (type.Contains("boolean")) defaultValue = "false";
        else if (type.Contains("integer")) defaultValue = "0";
        else if (type.Contains("double") || type.Contains("float")) defaultValue = "0.0";
        else if (type.Contains("object")) defaultValue = "{}";
        return defaultValue;
    }

    private string? GetRequestBodyType(OpenApiRequestBody? requestBody)
    {
        if (requestBody == null) return null;
        if (requestBody.Content.TryGetValue("application/json", out var media))
        {
            var schema = media.Schema;
            if (schema == null) return null; // Because "PostLolAccountVerificationV1SendDeactivationPin" exists where the media body is empty...
            return GetSchemaType(schema);
        }
        return null;
    }

    private AvaloniaList<ParameterViewModel> GetParameters(IList<OpenApiParameter> parameters, ParameterLocation location)
    {
        var pathParameters = new AvaloniaList<ParameterViewModel>();
        foreach (var parameter in parameters)
        {
            if (parameter.In != location) continue;
            pathParameters.Add(new ParameterViewModel(parameter.Name, GetSchemaType(parameter.Schema), parameter.Required));
        }

        return pathParameters;
    }

    private AvaloniaList<PropertyClassViewModel> GetResponseClasses(OpenApiResponses responses, Document lcuSchemaDocument)
    {
        if (responses.TryGetValue("2XX", out var response)
            && response.Content.TryGetValue("application/json", out var media))
        {
            var document = lcuSchemaDocument.OpenApiDocument;
            var schema = media.Schema;
            AvaloniaList<PropertyClassViewModel> propertyClasses = [];
            WalkSchema(schema, propertyClasses, document);
            return propertyClasses;
        }
        return [];
    }

    private void WalkSchema(OpenApiSchema schema, AvaloniaList<PropertyClassViewModel> propertyClasses, OpenApiDocument document)
    {
        var type = GetSchemaType(schema);
        if (IsComponent(type))
        {
            string componentId = GetComponentId(schema);
            var componentSchema = document.Components.Schemas[componentId];
            var responseClass = new PropertyClassViewModel(componentId, componentSchema.Properties, componentSchema.Enum);

            if (propertyClasses.Where(c => c.Id == componentId).Any()) return; // Avoid adding duplicate schemas in classes
            propertyClasses.Add(responseClass);

            foreach ((var _, var property) in componentSchema.Properties)
                // Check for self-references like "LolLootLootOddsResponse"
                // I blame dubble
                if (IsComponent(GetSchemaType(property)) && componentId != GetComponentId(property))
                    WalkSchema(property, propertyClasses, document);
        }
    }

    private static string GetComponentId(OpenApiSchema schema)
    {
        string componentId;
        if (schema.Reference != null) componentId = schema.Reference.Id;
        else if (schema.Items != null) componentId = schema.Items.Reference.Id;
        else componentId = schema.AdditionalProperties.Reference.Id;
        return componentId;
    }

    private static bool IsComponent(string type)
    {
        return !(type.Contains("object")
                            || type.Contains("array")
                            || type.Contains("bool")
                            || type.Contains("string")
                            || type.Contains("integer")
                            || type.Contains("number"));
    }

    private AvaloniaList<PropertyClassViewModel> GetRequestClasses(OpenApiRequestBody? requestBody, Document lcuSchemaDocument)
    {
        if (requestBody == null) return [];
        if (requestBody.Content.TryGetValue("application/json", out var media))
        {
            var document = lcuSchemaDocument.OpenApiDocument;
            var schema = media.Schema;
            if (schema == null) return [];

            var type = GetSchemaType(media.Schema);
            if (IsComponent(type))
            {
                var componentId = GetComponentId(schema);
                var componentSchema = document.Components.Schemas[componentId];
                AvaloniaList<PropertyClassViewModel> propertyClasses = [];
                WalkSchema(componentSchema, propertyClasses, document);
                return propertyClasses;
            }
        }
        return [];
    }

    private string GetReturnType(OpenApiResponses responses)
    {
        if (responses.TryGetValue("2XX", out var response)
            && response.Content.TryGetValue("application/json", out var media))
        {
            var schema = media.Schema;
            return GetSchemaType(schema);
        }
        return "none";
    }

    public static string GetSchemaType(OpenApiSchema schema)
    {
        if (schema.Reference != null) return schema.Reference.Id;
        if (schema.Type == "object" && schema.AdditionalProperties?.Reference != null) return schema.AdditionalProperties.Reference.Id;
        if (schema.Type == "integer" || schema.Type == "number") return $"{schema.Type}:{schema.Format}";
        if (schema.Type == "array" && schema.Items.Reference != null) return $"{schema.Items.Reference.Id}[]";
        if (schema.Type == "array" && (schema.Items.Type == "integer" || schema.Items.Type == "number")) return $"{schema.Items.Type}:{schema.Items.Format}[]";
        if (schema.Type == "array") return $"{schema.Items.Type}[]";
        return schema.Type;
    }
}