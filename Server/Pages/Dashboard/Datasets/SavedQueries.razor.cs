using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Services.Custom;
using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using BlazorDatasource.Server.Models.Dataset;
using System;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class SavedQueries : ComponentBase
    {

        [Inject]
        protected ISearchQueryService SearchQueryService { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected SearchQueryListModel? SearchQueryListModel { get; set; }
        
        protected bool Loading { get; set; }

        protected bool Busy;

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

            var searchQueries = await SearchQueryService.GetAllSearchQueryByUsernameAsync(username);
            var searchQueriesModels = searchQueries.Select(searchQuery => new SearchQueryModel
            {
                Id = searchQuery.Id,
                Name = searchQuery.Name,
                Keyword = searchQuery.Keyword,
                CreatedBy = searchQuery.CreatedBy,
                CreatedDate = searchQuery.CreatedDate
            });

            var pagedSearchQueries = searchQueriesModels.AsQueryable().ToPagedList(Page - 1, PageSize);

            SearchQueryListModel = new SearchQueryListModel().PrepareToGrid(pagedSearchQueries, () =>
            {
                return pagedSearchQueries.Select(searchQuery => new SearchQueryModel
                {
                    Keyword = searchQuery.Keyword,
                    Name = searchQuery.Name,
                    CreatedBy = searchQuery.CreatedBy,
                    CreatedDate = searchQuery.CreatedDate,
                    Id = searchQuery.Id,
                    Description = searchQuery.Description
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
    }
}
