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
    public partial class ViewWorkflow : ComponentBase
    {
        [Parameter]
        public string? WorkflowUuId { get; set; }

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

        protected WorkflowModel? Workflow { get; set; } = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected bool Loading { get; set; }

        protected bool Busy;

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected List<Dictionary<string, object>>? DatasetTimelineResults;

        protected List<string>? AxisList = new();

        protected ChartType SelectedChartType { get; set; } = ChartType.Bar;

        protected string? SelectedXAxis { get; set; }

        protected string? SelectedYAxis { get; set; }

        protected string? SelectedData { get; set; }

        protected bool ShowSaveFavorite { get; set; } = true;

        protected string? Status { get; set; } = string.Empty;

        protected string? Progress { get; set; } = string.Empty;

        protected string? SharedTo { get; set; }

        protected List<WorkflowResultModel> WorkflowResultTables { get; set; } = new List<WorkflowResultModel>();

        protected List<WorkflowResultModel> WorkflowResultDatasets { get; set; } = new List<WorkflowResultModel>();

        protected List<WorkflowOperatorModel> WorkflowOperatorTables { get; set; } = new List<WorkflowOperatorModel>();

        protected List<WorkflowOperatorModel> WorkflowOperatorDatasets { get; set; } = new List<WorkflowOperatorModel>();

        protected List<WorkflowOperatorModel> WorkflowOperatorCharts { get; set; } = new List<WorkflowOperatorModel>();

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
            if (WorkflowUuId is null)
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

            var remoteViewWorkflowRequest = new ViewWorkflowRequest()
            {
                WorkflowUuid = WorkflowUuId
            };

            var statusResult = await Client.GetWorkflowStatusByUuid(remoteViewWorkflowRequest);
            if (statusResult.IsSuccessStatusCode)
            {
                var temp = await statusResult.Content.ReadFromJsonAsync<string>();
                Status = temp?.Replace("\"", "");
            }

            var progressResult = await Client.GetWorkflowProgressByUuid(remoteViewWorkflowRequest);
            if (progressResult.IsSuccessStatusCode)
            {
                var temp = await progressResult.Content.ReadFromJsonAsync<string>();
                var progress = double.Parse(temp?.Replace("\"", "")!, CultureInfo.InvariantCulture);
                Progress = $"{progress:F2}%";
            }

            await Task.Delay(2000);

            var workflow = await Client.GetWorkflowByUuid(remoteViewWorkflowRequest);
            if (workflow.IsSuccessStatusCode)
            {
                Workflow = await workflow.Content.ReadFromJsonAsync<WorkflowModel>();

                if (Workflow?.Result is null)
                {
                    return;
                }

                WorkflowResultTables = Workflow.Result.Where(i => i.Type == "table").ToList();
                WorkflowResultDatasets = Workflow.Result.Where(i => i.Type == "dataset").ToList();

                if (Workflow.Operators != null)
                {
                    foreach (var workflowOperator in Workflow.Operators)
                    {
                        WorkflowOperatorTables.AddRange(workflowOperator.Where(i => i.Type == "table"));
                        WorkflowOperatorDatasets.AddRange(workflowOperator.Where(i => i.Type == "dataset"));
                        WorkflowOperatorCharts.AddRange(workflowOperator.Where(i => i.Type == "barchart"));
                    }
                }

                //if (Workflow.Result.Any(i => i.Type == "table"))
                //{
                //    DatasetTimelineResults = Workflow.Result.FirstOrDefault(i => i.Type == "table")!.Data;

                //    var datasetTimelineResultsQueryable = DatasetTimelineResults!.AsQueryable();
                //    var pagedDatasetTimelineResults = datasetTimelineResultsQueryable.ToPagedList(Page - 1, PageSize);

                //    Results = new DatasetTimelineListModel().PrepareToGrid(pagedDatasetTimelineResults, () =>
                //    {
                //        return pagedDatasetTimelineResults.Select(datasetTimeline => new DatasetTimelineModel
                //        {
                //            Properties = datasetTimeline.Select(property => new TimelineProperty
                //            {
                //                Key = property.Key,
                //                Value = property.Value.ToString()
                //            }).ToList()
                //        });
                //    });
                //}
            }

            //await Task.Delay(2000);
            //var remoteViewDatasetResult = await Client.GetDatasetByUUId(remoteViewDatasetRequest);
            //if (remoteViewDatasetResult.IsSuccessStatusCode)
            //{
            //    DatasetTimelineResults = await remoteViewDatasetResult.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
            //    if (DatasetTimelineResults is null)
            //    {
            //        return;
            //    }

            //    AxisList = MainHelper.GetChartAxisLabels(DatasetTimelineResults, null);
            //    var datasetTimelineResultsQueryable = DatasetTimelineResults.AsQueryable();
            //    var pagedDatasetTimelineResults = datasetTimelineResultsQueryable.ToPagedList(Page - 1, PageSize);

            //    Results = new DatasetTimelineListModel().PrepareToGrid(pagedDatasetTimelineResults, () =>
            //    {
            //        return pagedDatasetTimelineResults.Select(datasetTimeline => new DatasetTimelineModel
            //        {
            //            Properties = datasetTimeline.Select(property => new TimelineProperty
            //            {
            //                Key = property.Key,
            //                Value = property.Value.ToString()
            //            }).ToList()
            //        });
            //    });

            //    ChartData.Clear();
            //    foreach (var item in Results.Data)
            //    {
            //        string? chartDataTupleKey = null;
            //        decimal chartDataTupleValue = decimal.Zero;
            //        foreach (var property in item.Properties)
            //        {

            //            if (property.Key == "time")
            //            {
            //                chartDataTupleKey = property.Value;
            //            }
            //            else if (property.Key == DownloadedDatasetAlias)
            //            {
            //                decimal.TryParse(property.Value, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out chartDataTupleValue);
            //            }
            //        }

            //        ChartData.Add(new KeyValuePair<string, decimal>(chartDataTupleKey ?? "", chartDataTupleValue));
            //    }

            //    ChartData = ChartData.OrderBy(keyValuePair => keyValuePair.Key).ToList();

            //    if (ParamHorizontalAxis is not null && ParamVerticalAxis is not null && ParamData is not null &&
            //        ParamChartType is not null)
            //    {
            //        SelectedXAxis = ParamHorizontalAxis;
            //        SelectedYAxis = ParamVerticalAxis;
            //        SelectedData = ParamData;
            //        Enum.TryParse(ParamChartType, out ChartType chartType);
            //        SelectedChartType = chartType;
            //        ShowSaveFavorite = false;
            //        GenerateChart(SelectedData, SelectedXAxis, SelectedYAxis);
            //    }
            //}
            //else
            //{
            //    Error = true;
            //    ErrorMessage = remoteViewDatasetResult.ReasonPhrase ?? T["Datasets.Downloaded.View.Error"];
            //}

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
