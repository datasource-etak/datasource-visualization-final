using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorDatasource.Shared.Domain.Custom;
using BlazorDatasource.Shared.Services.Custom;
using System.IdentityModel.Tokens.Jwt;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Search : ComponentBase
    {
        [Parameter]
        public string? InputKeyword { get; set; }
        [Inject]
        protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected StateContainer StateContainer { get; set; } = default!;

        [Inject]
        protected ISearchQueryService SearchQueryService { get; set; } = default!;

        [Inject]
        protected ISourceService SourceService { get; set; } = default!;

        public string Keyword { get; set; } = default!;

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected DatasetListModel? Results { get; set; }

        protected bool Loading { get; set; }

        protected bool Busy;

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (InputKeyword is not null)
            {
                Keyword = InputKeyword;
            } 
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

            if (!string.IsNullOrEmpty(Keyword))
            {
                var remoteSearchRequest = new SearchRequest()
                {
                    Keyword = !string.IsNullOrEmpty(Keyword) ? Keyword : InputKeyword
                };

                var remoteSearchResult = await Client.SearchDatasets(remoteSearchRequest);
                if (remoteSearchResult.IsSuccessStatusCode)
                {
                    var searchResults = await remoteSearchResult.Content.ReadFromJsonAsync<List<DatasetModel>>();
                    if (searchResults is null)
                    {
                        return;
                    }

                    var searchResultsQueryable = searchResults.AsQueryable();
                    var pagedSearchResults = searchResultsQueryable.ToPagedList(Page - 1, PageSize);

                    Results = new DatasetListModel().PrepareToGrid(pagedSearchResults, () =>
                    {
                        return pagedSearchResults.Select(dataset => new DatasetModel
                        {
                            DatasetId = dataset.DatasetId,
                            DatasetName = dataset.DatasetName,
                            DatasetDescription = dataset.DatasetDescription,
                            SourceId = dataset.SourceId,
                            SourceName = dataset.SourceName
                        });
                    });

                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(TokenProvider.AccessToken);
                        var username = jwtToken.Claims.First(claim => claim.Type == "preferred_username").Value;
                        var searchQuery = new SearchQuery()
                        {
                            Keyword = Keyword,
                            Name = "Search Datasets with input string: " + Keyword,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = username,
                        };

                        await SearchQueryService.InsertSearchQueryAsync(searchQuery);
                    }

                    List<Source> sourceList = searchResults
                        .GroupBy(x => new { x.SourceName, x.SourceId })
                        .Select(g => g.First())
                        .ToList()
                        .Select(x => new Source { Name = x.SourceName, SourceId = x.SourceId, CreatedDate = DateTime.UtcNow})
                        .ToList();

                    await SourceService.InsertSourcesAsync(sourceList);
                }
                else
                {
                    Error = true;
                    ErrorMessage = remoteSearchResult.ReasonPhrase ?? T["Datasets.Search.Error"];
                }
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

        protected async Task SearchKeyPress(KeyboardEventArgs ev)
        {
            if (ev.Key == "Enter")
            {
                await ReloadAsync();
            }
        }

        protected void ViewFilters(DatasetModel selectedDataset)
        {
            StateContainer.Dataset = selectedDataset;
            NavigationManager.NavigateTo($"/dashboard/datasets/search/filters/");
        }
    }
}
