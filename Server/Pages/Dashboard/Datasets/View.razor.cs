using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorDatasource.Server.Helpers;
using BlazorDatasource.Shared.Domain.Custom;
using BlazorDatasource.Shared.Services.Custom;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class View : ComponentBase
    {
        [Parameter]
        public string? DownloadedDatasetUuId { get; set; }

        [Parameter]
        public string? DownloadedDatasetName { get; set; }

        [Parameter]
        public string? DownloadedDatasetAlias { get; set; }

        [Parameter]
        public string? ParamHorizontalAxis { get; set; }

        [Parameter]
        public string? ParamVerticalAxis { get; set; }

        [Parameter]
        public string? ParamData { get; set; }

        [Parameter]
        public string? ParamChartType { get; set; }

        [Inject]
        protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected EditSuccess EditSuccessState { get; set; } = default!;

        [Inject]
        protected MainHelper MainHelper { get; set; } = default!;

        [Inject]
        protected IFavoriteDatasetService FavoriteDatasetService { get; set; } = default!;

        [Inject]
        protected ISharedDatasetService SharedDatasetService { get; set; } = default!;

        protected ChartType DefaultChartType { get; set; }

        protected bool RenderProperties { get; set; } = true;

        protected bool DisplayCharts { get; set; } = true;

        protected List<KeyValuePair<string, decimal>> ChartData { get; set; } = new();

        protected ChartDataMultiple ChartDataMultiple { get; set; } = new();

        protected DatasetTimelineListModel? Results { get; set; }

        protected DownloadedDatasetModel? DownloadedDataset { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected bool Loading { get; set; }

        protected bool Busy;

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected List<Dictionary<string, object>>? DatasetTimelineResults;

        protected List<string>? AxisList = new();

        protected ChartType SelectedChartType { get; set; } = ChartType.Line;

        protected string? SelectedXAxis { get; set; }

        protected string? SelectedYAxis { get; set; }

        protected string? SelectedData { get; set; }

        protected bool ShowSaveFavorite { get; set; } = true;

        protected string? Status { get; set; } = string.Empty;

        protected string? SharedTo { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Page = 1;
            PageSize = PagingDefaults.DefaultGridPageSizeSmall;
            AvailablePageSizes = PagingDefaults.GridPageSizes;

            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            await ReloadAsync();
            await base.OnParametersSetAsync();
        }

        protected async Task ReloadAsync()
        {
            if (DownloadedDatasetUuId is null)
            {
                return;
            }

            if (Loading)
            {
                return;
            }

            if (Busy)
            {
                return;
            }

            Loading = true;

            var remoteViewDatasetRequest = new ViewDatasetRequest()
            {
                DatasetUUId = DownloadedDatasetUuId
            };

            var datasetResult = await Client.GetDatasetById(DownloadedDatasetUuId);
            if (datasetResult.IsSuccessStatusCode)
            {
                DownloadedDataset = await datasetResult.Content.ReadFromJsonAsync<DownloadedDatasetModel>();
            }

            var statusResult = await Client.GetDatasetStatusByUUId(remoteViewDatasetRequest);
            if (statusResult.IsSuccessStatusCode)
            {
                var temp = await statusResult.Content.ReadFromJsonAsync<string>();
                Status = temp?.Replace("\"", "");
            }

            await Task.Delay(2000);
            var remoteViewDatasetResult = await Client.GetDatasetByUUId(remoteViewDatasetRequest);
            if (remoteViewDatasetResult.IsSuccessStatusCode)
            {
                DatasetTimelineResults = await remoteViewDatasetResult.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
                if (DatasetTimelineResults is null)
                {
                    return;
                }

                AxisList = MainHelper.GetChartAxisLabels(DatasetTimelineResults, null);
                var datasetTimelineResultsQueryable = DatasetTimelineResults.AsQueryable();
                var pagedDatasetTimelineResults = datasetTimelineResultsQueryable.ToPagedList(Page - 1, PageSize);

                Results = new DatasetTimelineListModel().PrepareToGrid(pagedDatasetTimelineResults, () =>
                {
                    return pagedDatasetTimelineResults.Select(datasetTimeline => new DatasetTimelineModel
                    {
                        Properties = datasetTimeline.Select(property => new TimelineProperty
                        {
                            Key = property.Key,
                            Value = property.Value.ToString()
                        }).ToList()
                    });
                });

                ChartData.Clear();
                foreach (var item in Results.Data)
                {
                    string? chartDataTupleKey = null;
                    decimal chartDataTupleValue = decimal.Zero;
                    foreach (var property in item.Properties)
                    {
                        
                        if (property.Key == "time")
                        {
                            chartDataTupleKey = property.Value;
                        }
                        else if (property.Key == DownloadedDatasetAlias)
                        {
                            decimal.TryParse(property.Value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out chartDataTupleValue);
                        }
                    }

                    ChartData.Add(new KeyValuePair<string, decimal>(chartDataTupleKey ?? "", chartDataTupleValue));
                }

                ChartData = ChartData.OrderBy(keyValuePair => keyValuePair.Key).ToList();

                if (ParamHorizontalAxis is not null && ParamVerticalAxis is not null && ParamData is not null &&
                    ParamChartType is not null)
                {
                    SelectedXAxis = ParamHorizontalAxis;
                    SelectedYAxis = ParamVerticalAxis;
                    SelectedData = ParamData;
                    Enum.TryParse(ParamChartType, out ChartType chartType);
                    SelectedChartType = chartType;
                    ShowSaveFavorite = false;
                    GenerateChart(SelectedData, SelectedXAxis, SelectedYAxis);
                }
            }
            else
            {
                Error = true;
                ErrorMessage = remoteViewDatasetResult.ReasonPhrase ?? T["Datasets.Downloaded.View.Error"];
            }

            Loading = false;
        }

        private void GenerateChart(string? datasetName, string? xAxis, string? yAxis)
        {
            var result = MainHelper.GetChartDataMultiple(
                DatasetTimelineResults!,
                datasetName!,
                xAxis!,
                yAxis!);

            ChartDataMultiple.Id = Guid.NewGuid();
            ChartDataMultiple.Labels = result.Labels;
            ChartDataMultiple.Data = result.Data;
        }

        public void OnGenerateClick()
        {
            ShowSaveFavorite = true;

            GenerateChart(SelectedData, SelectedXAxis, SelectedYAxis);

            Busy = false;
        }

        public async Task OnShareClick()
        {
            Busy = true;

            DatasetSourceInfoResponse? datasetSourceInfoResponse = null;

            var datasetSourceInfoResult = await Client.GetDatasetSourceInfoById(DownloadedDatasetUuId!);
            if (datasetSourceInfoResult.IsSuccessStatusCode)
            {
                datasetSourceInfoResponse = await datasetSourceInfoResult.Content.ReadFromJsonAsync<DatasetSourceInfoResponse>();
            }

            if (datasetSourceInfoResponse is null || DownloadedDataset is null)
            {
                Busy = false;
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(TokenProvider.AccessToken);
            var username = jwtToken.Claims.First(claim => claim.Type == "preferred_username").Value;

            var sharedDataset = new SharedDataset()
            {
                DatasetId = Guid.Parse(DownloadedDatasetUuId!),
                DatasetName = DownloadedDatasetName,
                SourceName = DownloadedDataset.DatasetId,
                SourceId = datasetSourceInfoResponse.SourceId,
                SelectedFilters = JsonConvert.SerializeObject(DownloadedDataset.AssociatedFilter),
                DatasetAlias = SelectedData,
                ChartType = SelectedChartType.ToString(),
                XAxis = SelectedXAxis,
                YAxis = SelectedYAxis,
                SharedTo = SharedTo,
                CreatedBy = username,
                CreatedDate = DateTime.UtcNow,
            };

            await SharedDatasetService.InsertSharedDatasetAsync(sharedDataset);

            SharedTo = string.Empty;

            Busy = false;
        }

        public async Task OnSaveFavoriteClick()
        {
            Busy = true;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(TokenProvider.AccessToken);
            var username = jwtToken.Claims.First(claim => claim.Type == "preferred_username").Value;
            var favoriteDataset = new FavoriteDataset()
            {
                DatasetName = DownloadedDatasetName,
                DatasetAlias = SelectedData,
                DatasetId = Guid.Parse(DownloadedDatasetUuId!),
                ChartType = SelectedChartType.ToString(),
                XAxis = SelectedXAxis,
                YAxis = SelectedYAxis,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = username,
            };

            await FavoriteDatasetService.InsertFavoriteDatasetAsync(favoriteDataset);

            ShowSaveFavorite = false;

            Busy = false;
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
    }
}
