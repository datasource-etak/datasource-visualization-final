using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Downloaded : ComponentBase
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

        protected DownloadedDatasetListModel? Results { get; set; }

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

            var remoteDownloadedDatasetResult = await Client.GetAllDownloadedDatasets();
            if (remoteDownloadedDatasetResult.IsSuccessStatusCode)
            {
                var downloadedDatasetResults = await remoteDownloadedDatasetResult.Content.ReadFromJsonAsync<List<DownloadedDatasetModel>>();
                if (downloadedDatasetResults is null)
                {
                    return;
                }

                if(HideEmptyDatasets)
                {
                    downloadedDatasetResults = (from dataset in downloadedDatasetResults
                                               where dataset.Status == "DOWNLOADED"
                                               select dataset).ToList();
                }

                var downloadedDatasetsQueryable = downloadedDatasetResults.AsQueryable();
                var pagedDownloadedDatasetsResults = downloadedDatasetsQueryable.ToPagedList(Page - 1, PageSize);

                Results = new DownloadedDatasetListModel().PrepareToGrid(pagedDownloadedDatasetsResults, () =>
                {
                    return pagedDownloadedDatasetsResults.Select(downloadedDataset => new DownloadedDatasetModel
                    {
                        DatasetName = downloadedDataset.DatasetName,
                        DatasetDescription = downloadedDataset.DatasetDescription,
                        DatasetId = downloadedDataset.DatasetId,
                        Alias = downloadedDataset.Alias,
                        Uuid = downloadedDataset.Uuid,
                        SourceName = downloadedDataset.SourceName,
                        Status = downloadedDataset.Status,
                        AssociatedFilter = downloadedDataset.AssociatedFilter
                    });
                });
            }
            else
            {
                Error = true;
                ErrorMessage = remoteDownloadedDatasetResult.ReasonPhrase ?? T["Datasets.Downloaded.Error"];
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
