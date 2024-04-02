using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Services.Custom;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;
using BlazorDatasource.Server.Models.Dataset;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Shared.Domain.Custom;
using BlazorDatasource.Shared.Infrastructure.Models;
using NuGet.Packaging;
using static NuGet.Packaging.PackagingConstants;
using Newtonsoft.Json;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Shared : ComponentBase
    {
        [Inject]
        protected ISharedDatasetService SharedDatasetService { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject]
        protected EditSuccess EditSuccessState { get; set; } = default!;

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected SharedDatasetListModel? SharedDatasetListModel { get; set; }

        protected bool Loading { get; set; }

        protected bool Busy;

        protected Dictionary<Guid, bool> BusyDictionary = new();

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Loading = true;

            Page = 1;
            PageSize = PagingDefaults.DefaultGridPageSize;
            AvailablePageSizes = PagingDefaults.GridPageSizes;

            Loading = false;

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            await ReloadAsync();
            await base.OnParametersSetAsync();
        }

        protected async Task ReloadAsync()
        {
            if (Loading || Page < 1)
            {
                return;
            }

            if (Busy)
            {
                return;
            }

            Loading = true;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(TokenProvider.AccessToken);
            var username = jwtToken.Claims.First(claim => claim.Type == "preferred_username").Value;

            BusyDictionary = new Dictionary<Guid, bool>();

            var sharedDatasets = await SharedDatasetService.GetAllSharedDatasetsBySharedToAsync(username);
            foreach (var sharedDataset in sharedDatasets)
            {
                BusyDictionary.Add(sharedDataset.DatasetId, false);
            }
            var sharedDatasetModels = sharedDatasets.Select(sharedDataset => new SharedDatasetModel
            {
                Id = sharedDataset.Id,
                DatasetId = sharedDataset.DatasetId,
                SourceName = sharedDataset.SourceName,
                SourceId = sharedDataset.SourceId,
                SelectedFilters = sharedDataset.SelectedFilters,
                DatasetName = sharedDataset.DatasetName,
                DatasetAlias = sharedDataset.DatasetAlias,
                XAxis = sharedDataset.XAxis,
                YAxis = sharedDataset.YAxis,
                ChartType = sharedDataset.ChartType,
                CreatedDate = sharedDataset.CreatedDate,
                CreatedBy = sharedDataset.CreatedBy,
                SharedTo = sharedDataset.SharedTo,
                IsGenerated = sharedDataset.IsGenerated,
                NewDatasetId = sharedDataset.NewDatasetId,
                NewDatasetName = sharedDataset.NewDatasetName,
                NewDatasetAlias = sharedDataset.NewDatasetAlias,
                GeneratedDate = sharedDataset.GeneratedDate,
                DatasetExists = sharedDataset.DatasetExists
            });

            var pagedSharedDatasets = sharedDatasetModels.AsQueryable().ToPagedList(Page - 1, PageSize);

            SharedDatasetListModel = new SharedDatasetListModel().PrepareToGrid(pagedSharedDatasets, () =>
            {
                return pagedSharedDatasets.Select(sharedDataset => new SharedDatasetModel
                {
                    Id = sharedDataset.Id,
                    DatasetId = sharedDataset.DatasetId,
                    SourceName = sharedDataset.SourceName,
                    SourceId = sharedDataset.SourceId,
                    SelectedFilters = sharedDataset.SelectedFilters,
                    DatasetName = sharedDataset.DatasetName,
                    DatasetAlias = sharedDataset.DatasetAlias,
                    XAxis = sharedDataset.XAxis,
                    YAxis = sharedDataset.YAxis,
                    ChartType = sharedDataset.ChartType,
                    CreatedDate = sharedDataset.CreatedDate,
                    CreatedBy = sharedDataset.CreatedBy,
                    SharedTo = sharedDataset.SharedTo,
                    IsGenerated = sharedDataset.IsGenerated,
                    NewDatasetId = sharedDataset.NewDatasetId,
                    NewDatasetName = sharedDataset.NewDatasetName,
                    NewDatasetAlias = sharedDataset.NewDatasetAlias,
                    GeneratedDate = sharedDataset.GeneratedDate,
                    DatasetExists = sharedDataset.DatasetExists
                });
            });

            Loading = false;
        }
        protected async Task OnPageSizeChanged(ChangeEventArgs args)
        {
            if (args.Value is not null)
            {
                Page = 1;
                PageSize = Convert.ToInt32(args.Value);
                await ReloadAsync();
            }
        }

        public void OnViewClick(MouseEventArgs args, SharedDatasetModel sharedDatasetModel)
        {
            NavigationManager.NavigateTo($"/dashboard/datasets/downloaded/view/{sharedDatasetModel.NewDatasetId}/{sharedDatasetModel.SourceName}/{sharedDatasetModel.NewDatasetAlias}/{sharedDatasetModel.XAxis}/{sharedDatasetModel.YAxis}/{sharedDatasetModel.NewDatasetAlias}/{sharedDatasetModel.ChartType}");
        }

        public async Task OnGenerateClick(MouseEventArgs args, SharedDatasetModel sharedDatasetModel)
        {
            if (BusyDictionary.ContainsKey(sharedDatasetModel.DatasetId))
            {
                BusyDictionary[sharedDatasetModel.DatasetId] = true;
            }

            await GenerateDataset(sharedDatasetModel);

            if (BusyDictionary.ContainsKey(sharedDatasetModel.DatasetId))
            {
                BusyDictionary[sharedDatasetModel.DatasetId] = false;
            }
        }

        private async Task GenerateDataset(SharedDatasetModel sharedDatasetModel)
        {
            var remoteDownloadDatasetRequest = new DownloadDatasetRequest
            {
                DatasetId = sharedDatasetModel.SourceName,
                SourceId = sharedDatasetModel.SourceId.ToString(),
                FilterRequest =
                {
                    SelectedFilters = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(sharedDatasetModel.SelectedFilters),
                    Alias = sharedDatasetModel.DatasetAlias + "_shared" + DateTime.Now.ToString("ddMMyyyyHHss")
                }
            };

            var remoteDownloadDatasetResult = await Client.DownloadDataset(remoteDownloadDatasetRequest);
            if (remoteDownloadDatasetResult.IsSuccessStatusCode)
            {
                var downloadedDatasetUuId = await remoteDownloadDatasetResult.Content.ReadFromJsonAsync<DownloadDatasetResponse>();
                if (downloadedDatasetUuId is not null)
                {
                    var sharedDatasetToUpdate = new SharedDataset
                    {
                        Id = sharedDatasetModel.Id,
                        DatasetId = sharedDatasetModel.DatasetId,
                        DatasetName = sharedDatasetModel.DatasetName,
                        SourceName = sharedDatasetModel.SourceName,
                        SourceId = sharedDatasetModel.SourceId,
                        DatasetAlias = sharedDatasetModel.DatasetAlias,
                        SelectedFilters = sharedDatasetModel.SelectedFilters,
                        XAxis = sharedDatasetModel.XAxis,
                        YAxis = sharedDatasetModel.YAxis,
                        ChartType = sharedDatasetModel.ChartType,
                        CreatedDate = sharedDatasetModel.CreatedDate,
                        CreatedBy = sharedDatasetModel.CreatedBy,
                        SharedTo = sharedDatasetModel.SharedTo,
                        IsGenerated = true,
                        NewDatasetId = Guid.Parse(downloadedDatasetUuId.DatasetUUId),
                        NewDatasetAlias = remoteDownloadDatasetRequest.FilterRequest.Alias,
                        GeneratedDate = DateTime.Now
                    };

                    await SharedDatasetService.UpdateSharedDatasetAsync(sharedDatasetToUpdate);

                    EditSuccessState.Success = true;
                    EditSuccessState.SuccessMessage = T["Datasets.Search.Filters.DownloadDataset.Success"];

                    NavigationManager.NavigateTo($"/dashboard/datasets/downloaded/view/{sharedDatasetToUpdate.NewDatasetId}/{sharedDatasetToUpdate.SourceName}/{sharedDatasetToUpdate.NewDatasetAlias}/{sharedDatasetToUpdate.XAxis}/{sharedDatasetToUpdate.YAxis}/{sharedDatasetToUpdate.NewDatasetAlias}/{sharedDatasetToUpdate.ChartType}");
                }
                else
                {
                    EditSuccessState.Success = false;
                    Error = true;
                    ErrorMessage = T["Datasets.Search.Filters.DownloadDataset.Error"];
                }
            }
            else if (remoteDownloadDatasetResult.StatusCode == System.Net.HttpStatusCode.Found)
            {
                var sharedDatasetToUpdate = new SharedDataset
                {
                    Id = sharedDatasetModel.Id,
                    DatasetId = sharedDatasetModel.DatasetId,
                    DatasetName = sharedDatasetModel.DatasetName,
                    SourceName = sharedDatasetModel.SourceName,
                    SourceId = sharedDatasetModel.SourceId,
                    DatasetAlias = sharedDatasetModel.DatasetAlias,
                    SelectedFilters = sharedDatasetModel.SelectedFilters,
                    XAxis = sharedDatasetModel.XAxis,
                    YAxis = sharedDatasetModel.YAxis,
                    ChartType = sharedDatasetModel.ChartType,
                    CreatedDate = sharedDatasetModel.CreatedDate,
                    CreatedBy = sharedDatasetModel.CreatedBy,
                    SharedTo = sharedDatasetModel.SharedTo,
                    DatasetExists = true
                };

                await SharedDatasetService.UpdateSharedDatasetAsync(sharedDatasetToUpdate);

                Error = true;
                ErrorMessage = remoteDownloadDatasetResult.ReasonPhrase ?? T["Datasets.Search.Filters.DownloadDataset.Error"];

                await ReloadAsync();
            }
            else
            {
                Error = true;
                ErrorMessage = remoteDownloadDatasetResult.ReasonPhrase ?? T["Datasets.Search.Filters.DownloadDataset.Error"];
            }
        }
    }
}
