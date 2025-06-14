using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.OpenApi.Models;
using Needlework.Net.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class OperationViewModel : ObservableObject
{
    public OperationViewModel(OpenApiOperation operation, Models.Document document)
    {
        Summary = operation.Summary ?? string.Empty;
        Description = operation.Description ?? string.Empty;
        IsRequestBody = operation.RequestBody != null;
        ReturnType = GetReturnType(operation.Responses);
        RequestClasses = GetRequestClasses(operation.RequestBody, document);
        ResponseClasses = GetResponseClasses(operation.Responses, document);
        PathParameters = GetParameters(operation.Parameters.ToList(), ParameterLocation.Path);
        QueryParameters = GetParameters(operation.Parameters.ToList(), ParameterLocation.Query);
        RequestBodyType = GetRequestBodyType(operation.RequestBody);
        RequestTemplate = GetRequestTemplate(operation.RequestBody, document);
    }

    public string Summary { get; }

    public string Description { get; }

    public string ReturnType { get; }

    public bool IsRequestBody { get; }

    public string? RequestBodyType { get; }

    public List<PropertyClassViewModel> RequestClasses { get; }

    public List<PropertyClassViewModel> ResponseClasses { get; }

    public List<ParameterViewModel> PathParameters { get; }

    public List<ParameterViewModel> QueryParameters { get; }

    public string? RequestTemplate { get; }

    private string? GetRequestTemplate(OpenApiRequestBody? requestBody, Document document)
    {
        var requestClasses = GetRequestClasses(requestBody, document);
        if (requestClasses.Count == 0)
        {
            var type = GetRequestBodyType(requestBody);
            if (type == null) return null;
            return GetRequestDefaultValue(type);
        }

        var template = CreateTemplate(requestClasses);
        return JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(string.Join(string.Empty, template)), App.JsonSerializerOptions);
    }

    private List<string> CreateTemplate(List<PropertyClassViewModel> requestClasses)
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
                    List<PropertyClassViewModel> classes = [.. requestClasses];
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

    private List<ParameterViewModel> GetParameters(List<OpenApiParameter> parameters, ParameterLocation location)
    {
        var pathParameters = new List<ParameterViewModel>();
        foreach (var parameter in parameters)
        {
            if (parameter.In != location) continue;
            pathParameters.Add(new ParameterViewModel(parameter.Name, GetSchemaType(parameter.Schema), parameter.Required));
        }

        return pathParameters;
    }

    private bool TryGetResponse(OpenApiResponses responses, [NotNullWhen(true)] out OpenApiResponse? response)
    {
        response = null;
        var flag = false;
        if (responses.TryGetValue("2XX", out var x))
        {
            response = x;
            flag = true;
        }
        else if (responses.TryGetValue("200", out var y))
        {
            response = y;
            flag = true;
        }
        return flag;

    }

    private List<PropertyClassViewModel> GetResponseClasses(OpenApiResponses responses, Document document)
    {
        if (!TryGetResponse(responses, out var response))
            return [];

        if (response.Content.TryGetValue("application/json", out var media))
        {
            var rawDocument = document.OpenApiDocument;
            var schema = media.Schema;
            if (schema == null) return [];

            List<PropertyClassViewModel> propertyClasses = [];
            WalkSchema(schema, propertyClasses, rawDocument);
            return propertyClasses;
        }

        return [];
    }

    private void WalkSchema(OpenApiSchema schema, List<PropertyClassViewModel> propertyClasses, OpenApiDocument document)
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

    private List<PropertyClassViewModel> GetRequestClasses(OpenApiRequestBody? requestBody, Document document)
    {
        if (requestBody == null) return [];
        if (requestBody.Content.TryGetValue("application/json", out var media))
        {
            var rawDocument = document.OpenApiDocument;
            var schema = media.Schema;
            if (schema == null) return [];

            var type = GetSchemaType(media.Schema);
            if (IsComponent(type))
            {
                var componentId = GetComponentId(schema);
                var componentSchema = rawDocument.Components.Schemas[componentId];
                List<PropertyClassViewModel> propertyClasses = [];
                WalkSchema(componentSchema, propertyClasses, rawDocument);
                return propertyClasses;
            }
        }
        return [];
    }

    private string GetReturnType(OpenApiResponses responses)
    {
        if (!TryGetResponse(responses, out var response))
            return "none";

        if (response.Content.TryGetValue("application/json", out var media))
        {
            var schema = media.Schema;
            return GetSchemaType(schema);
        }

        return "none";
    }

    public static string GetSchemaType(OpenApiSchema? schema)
    {
        if (schema == null) return "object"; // Because GetLolVanguardV1Notification exists where it has a required parameter without a type...
        if (schema.Reference != null) return schema.Reference.Id;
        if (schema.Type == "object" && schema.AdditionalProperties?.Reference != null) return schema.AdditionalProperties.Reference.Id;
        if (schema.Type == "integer" || schema.Type == "number") return $"{schema.Type}:{schema.Format}";
        if (schema.Type == "array" && schema.AdditionalProperties?.Reference != null) return schema.AdditionalProperties.Reference.Id;
        if (schema.Type == "array" && schema.Items.Reference != null) return $"{schema.Items.Reference.Id}[]";
        if (schema.Type == "array" && (schema.Items.Type == "integer" || schema.Items.Type == "number")) return $"{schema.Items.Type}:{schema.Items.Format}[]";
        if (schema.Type == "array") return $"{schema.Items.Type}[]";
        return schema.Type;
    }
}