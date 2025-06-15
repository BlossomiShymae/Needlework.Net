using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.OpenApi.Models;
using Needlework.Net.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class OperationViewModel : ObservableObject
{
    public OperationViewModel(OpenApiOperation operation, Models.Document document)
    {
        Summary = operation.Summary ?? string.Empty;
        Description = operation.Description ?? string.Empty;
        IsRequestBody = operation.RequestBody != null;
        ReturnType = OpenApiHelpers.GetReturnType(operation.Responses);
        RequestClasses = OpenApiHelpers.GetRequestClasses(operation.RequestBody, document);
        ResponseClasses = OpenApiHelpers.GetResponseClasses(operation.Responses, document);
        PathParameters = OpenApiHelpers.GetParameters(operation.Parameters.ToList(), ParameterLocation.Path);
        QueryParameters = OpenApiHelpers.GetParameters(operation.Parameters.ToList(), ParameterLocation.Query);
        RequestBodyType = OpenApiHelpers.GetRequestBodyType(operation.RequestBody);
        RequestTemplate = OpenApiHelpers.GetRequestTemplate(operation.RequestBody, document);
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
}