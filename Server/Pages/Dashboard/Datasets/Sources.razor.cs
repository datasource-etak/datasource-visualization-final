using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Services.Custom;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;
using BlazorDatasource.Server.Models.Dataset;
using System;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Sources : ComponentBase
    {
        [Inject]
        protected ISourceService SourceService { get; set; } = default!;

        [Inject]
        protected TokenProvider TokenProvider { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string? AvailablePageSizes { get; set; }

        protected SourceListModel? SourceListModel { get; set; }

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

            var sources = await SourceService.GetAllAsync();
            var sourcesModels = sources.Select(source => new SourceModel
            {
                Id = source.Id,
                Name = source.Name,
                SourceId = source.SourceId,
                CreatedDate = source.CreatedDate
            });

            var pagedSources = sourcesModels.AsQueryable().ToPagedList(Page - 1, PageSize);

            SourceListModel = new SourceListModel().PrepareToGrid(pagedSources, () =>
            {
                return pagedSources.Select(source => new SourceModel
                {
                    SourceId = source.SourceId,
                    Name = source.Name,
                    CreatedDate = source.CreatedDate,
                    Id = source.Id,
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
