using System;
using System.Collections.Generic;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Services.Custom;
using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlazorDatasource.Shared.Infrastructure.Models;
using BlazorDatasource.Server.Infrastructure;
using System.Net.Http.Json;
using BlazorDatasource.Server.Helpers;
using BlazorDatasource.Shared.Domain.Custom;
using Microsoft.AspNetCore.Routing;
using BlazorDatasource.Server.Models.Dataset;

namespace BlazorDatasource.Server.Pages.Dashboard
{
    public partial class Dashboard : ComponentBase
    {
        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject]
        protected IFavoriteDatasetService FavoriteDatasetService { get; set; } = default!;

        [Inject]
        protected ISearchQueryService SearchQueryService { get; set; } = default!;

        [Inject]
        protected MainHelper MainHelper { get; set; } = default!;

        [Inject]
        protected LinkGenerator LinkGenerator { get; set; } = default!;

        protected string? Username { get; set; }
        protected bool Busy = true;

        protected ChartType DefaultChartType { get; set; } = default!;
        protected List<ChartType> SelectedChartTypes { get; set; } = new();
        protected List<string> ChartTitles { get; set; } = new();
        protected List<ChartDataMultiple> ChartDataMultiples { get; set; } = new();
        protected IList<FavoriteDataset>? FavoriteDatasets { get; set; }
        protected IList<SearchQuery>? SearchQueries { get; set; }
        protected IList<DownloadedDatasetModel>? DownloadedDatasets { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Busy = true;

            //var handler = new JwtSecurityTokenHandler();
            //var jwtToken = handler.ReadJwtToken(TokenProvider.AccessToken);
            //Username = jwtToken.Claims.First(claim => claim.Type == "preferred_username").Value;

            if (TokenProvider.AccessToken is null)
            {
                var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authenticationState.User;
                if (user.Identity is not null && user.Identity.IsAuthenticated)
                {
                    Username = user.Identity.Name;
                }
            }
            else
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(TokenProvider.AccessToken);
                Username = jwtToken.Claims.First(claim => claim.Type == "preferred_username").Value;
            }


            SearchQueries = await SearchQueryService.GetAllSearchQueryByUsernameAsync(Username);
            FavoriteDatasets = await FavoriteDatasetService.GetAllFavoriteDatasetsByUsernameAsync(Username);

            var remoteDownloadedDatasetResult = await Client.GetAllDownloadedDatasets();
            if (remoteDownloadedDatasetResult.IsSuccessStatusCode)
            {
                var downloadedDatasetResults = await remoteDownloadedDatasetResult.Content
                    .ReadFromJsonAsync<List<DownloadedDatasetModel>>();
                if (downloadedDatasetResults is not null)
                {
                    DownloadedDatasets = (from dataset in downloadedDatasetResults
                        where dataset.Status == "DOWNLOADED"
                        select dataset).ToList();
                }
            }

            foreach (var favorite in FavoriteDatasets.OrderByDescending(i => i.CreatedDate))
            {

                Enum.TryParse(favorite.ChartType, out ChartType chartType);
                SelectedChartTypes.Add(chartType);
                ChartTitles.Add(favorite.DatasetName!);
                //get the dataset data
                var remoteViewDatasetRequest = new ViewDatasetRequest()
                {
                    DatasetUUId = favorite.DatasetId.ToString()
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
                    var result = MainHelper.GetChartDataMultiple(
                        datasetTimelineResults!,
                        favorite.DatasetAlias!,
                        favorite.XAxis!,
                        favorite.YAxis!);
    
                    var chartDataMultiple = new ChartDataMultiple
                    {
                        Id = Guid.NewGuid(),
                        Labels = result.Labels,
                        Data = result.Data
                    };
                    
                    ChartDataMultiples.Add(chartDataMultiple);
                }
                else
                {
                    if (remoteViewDatasetResult.StatusCode is System.Net.HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    {
                        NavigationManager.NavigateTo(LinkGenerator.GetPathByPage("/Account/Logout", values: new { area = "Identity" })!, true);
                    }
                }
            }


            Busy = false;

            await base.OnInitializedAsync();
        }
    }
}
