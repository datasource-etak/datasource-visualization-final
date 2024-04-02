using System;
using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorDatasource.Server.Helpers;
using System.Linq;
using BlazorDatasource.Server.Models.Common;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Join : ComponentBase
    {
        [Inject] protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject] protected MainHelper MainHelper { get; set; } = default!;

        [Inject]
        protected EditSuccess EditSuccessState { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        protected bool Loading { get; set; }

        protected bool LockDownloadedDatasets { get; set; } = false;

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected List<DownloadedDatasetModel>? DownloadedDatasets { get; set; } = new();

        protected List<DownloadedDatasetModel> SelectedDownloadedDatasets { get; set; } = new();

        protected Dictionary<Guid, List<string>> SelectedDatasetKeys { get; set; } = new();

        protected List<string> JoinTypes { get; set; } = new();

        protected string? JoinType { get; set; }

        protected List<string> JoinDatatypes { get; set; } = new();

        protected List<string> JoinKeys { get; set; } = new();

        protected string JoinAlias { get; set; } = string.Empty;

        protected string JoinKey { get; set; } = string.Empty;

        protected List<JoinDataset> JoinDatasets { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            Loading = true;

            var joindatatypesResult = await Client.GetJoinDatatypes();
            if (joindatatypesResult.IsSuccessStatusCode)
            {
                JoinDatatypes = (await joindatatypesResult.Content.ReadFromJsonAsync<List<string>>())!;
                if (JoinDatatypes is null)
                {
                    return;
                }
            }
            else
            {
                Error = true;
                ErrorMessage = (joindatatypesResult.ReasonPhrase ?? T["Datasets.JoinDatatypes.Error"])!;
            }

            var joinTypesResult = await Client.GetJoinTypes();
            if (joinTypesResult.IsSuccessStatusCode)
            {
                JoinTypes = (await joinTypesResult.Content.ReadFromJsonAsync<List<string>>())!;
                if (JoinTypes is null)
                {
                    return;
                }
            }
            else
            {
                Error = true;
                ErrorMessage = (joinTypesResult.ReasonPhrase ?? T["Datasets.JoinTypes.Error"])!;
            }

            var remoteDownloadedDatasetResult = await Client.GetAllDownloadedDatasets();
            if (remoteDownloadedDatasetResult.IsSuccessStatusCode)
            {
                DownloadedDatasets = await remoteDownloadedDatasetResult.Content
                    .ReadFromJsonAsync<List<DownloadedDatasetModel>>();
                if (DownloadedDatasets is null)
                {
                    return;
                }

                DownloadedDatasets = (from datasetTimelineResult in DownloadedDatasets
                    where datasetTimelineResult.Status == "DOWNLOADED"
                    select datasetTimelineResult).ToList();
            }
            else
            {
                Error = true;
                ErrorMessage = (remoteDownloadedDatasetResult.ReasonPhrase ?? T["Datasets.Downloaded.Error"])!;
            }

            Loading = false;

            await base.OnInitializedAsync();
        }

        protected void OnSelectDatasetValueChanged(ChangeEventArgs e)
        {
            SelectedDownloadedDatasets.Clear();

            var input = e.Value as string[];

            foreach (var value in input!)
            {
                SelectedDownloadedDatasets.Add(DownloadedDatasets!.Find(i => i.Uuid == Guid.Parse(value))!);
            }

            Console.WriteLine(SelectedDownloadedDatasets);
        }

        protected void OnSelectJoinTypeValueChanged(ChangeEventArgs e)
        {
        }

        protected async Task StartJoin()
        {
            Loading = true;
            LockDownloadedDatasets = true;

            foreach (var dataset in SelectedDownloadedDatasets)
            {
                var remoteViewDatasetRequest = new ViewDatasetRequest()
                {
                    DatasetUUId = dataset.Uuid.ToString()
                };
                var remoteViewDatasetResult = await Client.GetDatasetByUUId(remoteViewDatasetRequest);
                if (remoteViewDatasetResult.IsSuccessStatusCode)
                {
                    var datasetTimelineResults = await remoteViewDatasetResult.Content
                        .ReadFromJsonAsync<List<Dictionary<string, object>>>();
                    if (datasetTimelineResults is null)
                    {
                        return;
                    }

                    var keys = MainHelper.GetDatasetKeys(datasetTimelineResults);
                    SelectedDatasetKeys.Add(dataset.Uuid!, keys);

                    var joinDatasetKeys = new List<JoinDatasetKey>();
                    keys.ForEach(key =>
                    {
                        joinDatasetKeys.Add(new JoinDatasetKey()
                        {
                            OriginalKey = key,
                            NewKey = null,
                            NewDatatype = null
                        });
                    });

                    JoinDatasets.Add(new JoinDataset()
                    {
                        DatasetId = dataset.Uuid,
                        NewKeys = joinDatasetKeys
                    });
                }
                else
                {
                    Error = true;
                    ErrorMessage = remoteViewDatasetResult.ReasonPhrase ?? T["Datasets.Downloaded.View.Error"];
                }
            }

            Loading = false;
        }

        protected async Task CreateJoin()
        {
            Loading = true;

            var joinRequest = new JoinRequest()
            {
                JoinDatasets = new List<JoinRequestDataset>(),
                JoinType = JoinType,
                JoinAlias = JoinAlias,
                JoinKeys = JoinKeys
            };

            foreach (var dataset in JoinDatasets)
            {
                var datasetModel = SelectedDownloadedDatasets.Find(i => i.Uuid == dataset.DatasetId)!;
                var columns = new Dictionary<string, Dictionary<string, string>>();
                foreach (var key in dataset.NewKeys!)
                {
                    if (key.NewKey is not null && key.NewDatatype is not null)
                    {
                        columns.Add(key.OriginalKey!, new Dictionary<string, string>()
                        {
                            { "new_name", key.NewKey! },
                            { "cast_type", key.NewDatatype! }
                        });
                    }
                }

                joinRequest.JoinDatasets.Add(new JoinRequestDataset()
                {
                    Alias = datasetModel.Alias,
                    Columns = columns
                });
            }

            var result = await Client.JoinDataset(joinRequest);
            if (result.IsSuccessStatusCode)
            {
                var downloadedDatasetUuId = await result.Content.ReadFromJsonAsync<DownloadDatasetResponse>();
                if (downloadedDatasetUuId is not null)
                {
                    EditSuccessState.Success = true;
                    EditSuccessState.SuccessMessage = T["Datasets.Search.Filters.DownloadDataset.Success"];
                    NavigationManager.NavigateTo($"/dashboard/datasets/downloaded");
                }
                else
                {
                    EditSuccessState.Success = false;
                    Error = true;
                    ErrorMessage = T["Datasets.Join.JoinDataset.Error"];
                }
            }
            else
            {
                Error = true;
                ErrorMessage = result.ReasonPhrase ?? T["Datasets.Join.JoinDataset.Error"];
            }

            Loading = false;
        }

        protected async Task AddJoinKey(string key)
        {
            JoinKeys.Add(key);
            JoinKey = string.Empty;
        }

        protected async Task DeleteJoinKey(string key)
        {
            JoinKeys.Remove(key);
        }
    }
}
