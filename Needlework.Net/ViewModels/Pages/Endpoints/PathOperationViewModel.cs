﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PathOperationViewModel : ObservableObject
{
    public string Path { get; }
    public OperationViewModel Operation { get; }

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private Lazy<LcuRequestViewModel> _lcuRequest;

    public PathOperationViewModel(PathOperation pathOperation)
    {
        Path = pathOperation.Path;
        Operation = new OperationViewModel(pathOperation.Operation);
        LcuRequest = new(() => new LcuRequestViewModel()
        {
            Method = pathOperation.Method.ToUpper()
        });
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        var sb = new StringBuilder(Path);
        foreach (var pathParameter in Operation.PathParameters)
        {
            sb.Replace($"{{{pathParameter.Name}}}", pathParameter.Value);
        }

        var firstQueryAdded = false;
        foreach (var queryParameter in Operation.QueryParameters)
        {
            if (!string.IsNullOrWhiteSpace(queryParameter.Value))
            {
                sb.Append(firstQueryAdded ? '&' : '?');
                firstQueryAdded = true;
                sb.Append($"{queryParameter.Name}={Uri.EscapeDataString(queryParameter.Value)}");
            }
        }

        LcuRequest.Value.RequestPath = sb.ToString();
        await LcuRequest.Value.ExecuteAsync();
    }
}