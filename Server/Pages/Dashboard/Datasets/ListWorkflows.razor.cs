using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class ListWorkflows : ComponentBase
    {
        [Inject]
        protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected EditSuccess EditSuccessState { get; set; } = default!;

        public bool HideEmptyDatasets { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected WorkflowSubmittedListModel? Results { get; set; }

        protected bool Loading { get; set; }

        protected bool Busy;

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            Page = 1;
            PageSize = PagingDefaults.DefaultGridPageSize;
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
            if (Loading || Page < 1)
            {
                return;
            }

            if (Busy)
            {
                return;
            }

            Loading = true;

            var remoteWorkflowsResult = await Client.GetAllWorkflows();
            if (remoteWorkflowsResult.IsSuccessStatusCode)
            {
                var results = await remoteWorkflowsResult.Content.ReadFromJsonAsync<List<WorkflowSubmittedModel>>();
                if (results is null)
                {
                    return;
                }

                var workflowsQueryable = results.AsQueryable();
                var pagedWorkflowsResults = workflowsQueryable.ToPagedList(Page - 1, PageSize);

                Results = new WorkflowSubmittedListModel().PrepareToGrid(pagedWorkflowsResults, () =>
                {
                    return pagedWorkflowsResults.Select(workflow => new WorkflowSubmittedModel()
                    {
                        Uuid = workflow.Uuid,
                        Alias = workflow.Alias,
                        Description = workflow.Description,
                        DatasetId = workflow.DatasetId,
                        Features = workflow.Features,
                        Target = workflow.Target,
                        Status = workflow.Status,
                        SubmittedAt = workflow.SubmittedAt,
                        CompletedAt = workflow.CompletedAt,
                        WorkflowTypeId = workflow.WorkflowTypeId
                    });
                });
            }
            else
            {
                Error = true;
                ErrorMessage = remoteWorkflowsResult.ReasonPhrase ?? T["Workflows.Downloaded.Error"];
            }

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

        protected async Task FilterDatasets()
        {
            Page = 1;
            HideEmptyDatasets = !HideEmptyDatasets;
            await ReloadAsync();
        }
    }
}
