using System;
using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NuGet.Packaging;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Filters : ComponentBase
    {
        [Inject]
        protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected StateContainer StateContainer { get; set; } = default!;

        [Inject]
        protected EditSuccess EditSuccessState { get; set; } = default!;

        protected List<FilterModel> AvailableFilters { get; set; } = new();

        protected List<FilterModel> SelectedFilters { get; set; } = new();

        protected string SearchFilterValue { get; set; } = string.Empty;
        
        protected string Alias { get; set; } = string.Empty;

        protected List<FilterValueModel> Filtered => AvailableFilters.SelectMany(filter => filter.AvailableFilterValues)
                                                                     .Where(filterValue => filterValue.Description.ToLowerInvariant()
                                                                     .Contains(SearchFilterValue.ToLowerInvariant()))
                                                                     .OrderBy(filterValue => filterValue.Description)
                                                                     .ToList();

        protected bool Loading { get; set; }

        protected bool Busy;

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (StateContainer.Dataset is null)
            {
                return;
            }

            Loading = true;

            var remoteSearchFiltersRequest = new SearchFiltersRequest()
            {
                DatasetId = StateContainer.Dataset.DatasetId,
                SourceId = StateContainer.Dataset.SourceId
            };

            var remoteSearchFiltersResult = await Client.GetSearchFilters(remoteSearchFiltersRequest);
            if (remoteSearchFiltersResult.IsSuccessStatusCode)
            {
                var result = await remoteSearchFiltersResult.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>();

                if (result is null)
                {
                    AvailableFilters = new();
                }
                else
                {

                    foreach (var (key, value) in result)
                    {
                        AvailableFilters.Add(new FilterModel()
                        {
                            FilterIdentifier = new FilterIdentifierModel()
                            {
                                Id = key,
                                Index = 0
                            },
                            FilterName = key,
                            AvailableFilterValues = value.Select(filterValue => new FilterValueModel()
                            {
                                Id = filterValue,
                                Description = filterValue
                            }).ToList()
                        });
                    }
                }
            }
            else
            {
                Error = true;
                ErrorMessage = remoteSearchFiltersResult.ReasonPhrase ?? T["Datasets.Search.Filters.Error"];
            }

            Loading = false;

            await base.OnInitializedAsync();
        }

        protected FilterModel? SelectedFilter { get; set; }

        protected FilterValueModel? SelectedFilterValue { get; set; }

        protected async Task DownloadDataset()
        {
            if (StateContainer.Dataset is null)
            {
                return;
            }

            if (Loading)
            {
                return;
            }

            Loading = true;

            var remoteDownloadDatasetRequest = new DownloadDatasetRequest()
            {
                DatasetId = StateContainer.Dataset.DatasetId,
                SourceId = StateContainer.Dataset.SourceId
            };

            List<SearchFiltersResultFilter> searchFilters = new();

            foreach (var selectedFilter in SelectedFilters)
            {
                searchFilters.Add(new SearchFiltersResultFilter()
                {
                    FilterIdentifier = new FilterIdentifier()
                    {
                        Id = selectedFilter.FilterIdentifier.Id,
                        Index = selectedFilter.FilterIdentifier.Index
                    },
                    FilterName = selectedFilter.FilterName,
                    AvailableFilterValues = selectedFilter.AvailableFilterValues.Select(filterValue => new FilterValue()
                    {
                        Id = filterValue.Id,
                        Description = filterValue.Description
                    }).ToList()
                });
            }

            Dictionary<string, List<string>> filters = new();

            searchFilters.ForEach(filter =>
            {
                filters.Add(filter.FilterIdentifier.Id, filter.AvailableFilterValues.Select(filterValue => filterValue.Id).ToList());
            });

            remoteDownloadDatasetRequest.FilterRequest.SelectedFilters.AddRange(filters);
            remoteDownloadDatasetRequest.FilterRequest.Alias = Alias;

            var remoteDownloadDatasetResult = await Client.DownloadDataset(remoteDownloadDatasetRequest);
            if (remoteDownloadDatasetResult.IsSuccessStatusCode)
            {
                var downloadedDatasetUuId = await remoteDownloadDatasetResult.Content.ReadFromJsonAsync<DownloadDatasetResponse>();
                if (downloadedDatasetUuId is not null)
                {
                    EditSuccessState.Success = true;
                    EditSuccessState.SuccessMessage = T["Datasets.Search.Filters.DownloadDataset.Success"];
                    NavigationManager.NavigateTo($"/dashboard/datasets/downloaded/view/{downloadedDatasetUuId.DatasetUUId}/{remoteDownloadDatasetRequest.DatasetId}/{remoteDownloadDatasetRequest.FilterRequest.Alias}");
                }
                else
                {
                    EditSuccessState.Success = false;
                    Error = true;
                    ErrorMessage = T["Datasets.Search.Filters.DownloadDataset.Error"];
                }
            }
            else
            {
                Error = true;
                ErrorMessage = remoteDownloadDatasetResult.ReasonPhrase ?? T["Datasets.Search.Filters.DownloadDataset.Error"];
            }

            Loading = false;
        }

        protected void OnSelectFilterValueChanged(ChangeEventArgs e, string filterName)
        {
            var foundFilter = AvailableFilters.Find(i => i.FilterName == filterName);
            if (foundFilter is not null)
            {
                if (SelectedFilters.Exists(i => i.FilterName == filterName))
                {
                    SelectedFilters.Find(i => i.FilterName == filterName)!.AvailableFilterValues.Clear();
                    foreach (var value in (e.Value as string[])!)
                    {
                        SelectedFilters.Find(i => i.FilterName == filterName)!.AvailableFilterValues.Add(
                            foundFilter.AvailableFilterValues.Find(i => i.Description == value)!);
                    }
                }
                else
                {
                    SelectedFilters.Add(new FilterModel()
                    {
                        FilterName = foundFilter.FilterName,
                        FilterIdentifier = foundFilter.FilterIdentifier
                    });
                    foreach (var value in (e.Value as string[])!)
                    {
                        SelectedFilters.Find(i => i.FilterName == filterName)!.AvailableFilterValues.Add(
                            foundFilter.AvailableFilterValues.Find(i => i.Description == value)!);
                    }
                }
            }
        }
        
        protected void ClearSelectedFilterValue()
        {
            SelectedFilterValue = null;
        }
    }
}
