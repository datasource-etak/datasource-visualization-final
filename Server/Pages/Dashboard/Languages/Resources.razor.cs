using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Extensions;
using BlazorDatasource.Server.Models.Localization;
using BlazorDatasource.Shared.Domain.Localization;
using BlazorDatasource.Shared.Services.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorDatasource.Server.Pages.Dashboard.Languages
{
    public partial class Resources : ComponentBase
    {
        [Inject]
        protected ILanguageService LanguageService { get; set; } = default!;

        [Inject]
        protected ILocalizationService LocalizationService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected EditSuccess EditSuccessState { get; set; } = default!;

        /// <summary>
        /// Id of Language to get the resources
        /// </summary>
        [Parameter]
        public int LanguageId { get; set; }

        /// <summary>
        /// Language to show resources for
        /// </summary>
        protected LanguageModel? Language { get; set; }

        /// <summary>
        /// Search resource name
        /// </summary>
        public string SearchResourceName { get; set; } = default!;

        /// <summary>
        /// Search resource value
        /// </summary>
        public string SearchResourceValue { get; set; } = default!;

        /// <summary>
        /// Gets or sets a page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets a page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of available page sizes
        /// </summary>
        public string? AvailablePageSizes { get; set; }

        /// <summary>
        /// Resources list
        /// </summary>
        protected LocaleResourceListModel? LocaleResources { get; set; }

        /// <summary>
        /// Avoid multiple concurrent requests when loading.
        /// </summary>
        protected bool Loading { get; set; }

        /// <summary>
        /// Avoid concurrent requests
        /// </summary>
        protected bool Busy;

        /// <summary>
        /// An error occurred in the update
        /// </summary>
        protected bool Error;

        /// <summary>
        /// Error message
        /// </summary>
        protected string ErrorMessage = string.Empty;

        /// <summary>
        /// Create language resource
        /// </summary>
        private LocaleResourceModel AddResourceString = new();

        /// <summary>
        /// Start it up
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task OnInitializedAsync()
        {
            Loading = true;

            try
            {
                var language = await LanguageService.GetLanguageByIdAsync(LanguageId);
                if (language is null)
                    return;

                Language = new LanguageModel()
                {
                    Id = language.Id,
                    Name = language.Name,
                    LanguageCulture = language.LanguageCulture,
                    UniqueSeoCode = language.UniqueSeoCode,
                    FlagImageFileName = language.FlagImageFileName,
                    DisplayOrder = language.DisplayOrder,
                    Published = language.Published
                };

                // Paging
                Page = 1;
                PageSize = PagingDefaults.DefaultGridPageSize;
                AvailablePageSizes = PagingDefaults.GridPageSizes;
            }
            finally
            {
                Loading = false;
            }

            await ReloadAsync();
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Triggered for any paging update.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        //protected override async Task OnParametersSetAsync()
        //{
        //    await ReloadAsync();
        //    await base.OnParametersSetAsync();
        //}

        /// <summary>
        /// Back to list.
        /// </summary>
        protected void Cancel()
        {
            NavigationManager.NavigateTo("/dashboard/languages");
        }

        /// <summary>
        /// Reload page
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
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

            var language = await LanguageService.GetLanguageByIdAsync(LanguageId);
            if (language is null)
                return;

            //get locale resources
            var localeResources = (await LocalizationService.GetAllResourceValuesAsync(LanguageId))
                .OrderBy(localeResource => localeResource.Key)
                .AsQueryable();

            //filter locale resources
            if (!string.IsNullOrEmpty(SearchResourceName))
                localeResources = localeResources.Where(l => l.Key.ToLowerInvariant().Contains(SearchResourceName.ToLowerInvariant()));

            if (!string.IsNullOrEmpty(SearchResourceValue))
                localeResources = localeResources.Where(l => l.Value.Value.ToLowerInvariant().Contains(SearchResourceValue.ToLowerInvariant()));

            var pagedLocaleResources = localeResources.ToPagedList(Page - 1, PageSize);

            //prepare list model
            LocaleResources = new LocaleResourceListModel().PrepareToGrid(pagedLocaleResources, () =>
            {
                //fill in model values from the entity
                return pagedLocaleResources.Select(localeResource => new LocaleResourceModel
                {
                    LanguageId = language.Id,
                    Id = localeResource.Value.Key,
                    ResourceName = localeResource.Key,
                    ResourceValue = localeResource.Value.Value
                });
            });

            Loading = false;
        }

        /// <summary>
        /// OnPageSizeChanged event
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task OnPageSizeChanged(ChangeEventArgs args)
        {
            if (args.Value is not null)
            {
                Page = 1;
                PageSize = Convert.ToInt32(args.Value);
                await ReloadAsync();
            }
        }

        /// <summary>
        /// OnSearchResourceNameChanged event
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task OnSearchResourceNameChanged(ChangeEventArgs args)
        {
            if (args.Value is not null)
            {
                Page = 1;
                SearchResourceName = (string)args.Value;
                await ReloadAsync();
            }
        }

        /// <summary>
        /// OnSearchResourceNameChanged event
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task OnSearchResourceValueChanged(ChangeEventArgs args)
        {
            if (args.Value is not null)
            {
                Page = 1;
                SearchResourceValue = (string)args.Value;
                await ReloadAsync();
            }
        }

        /// <summary>
        /// Create language resource
        /// </summary>
        /// <param name="success">Success when model is valid</param>
        /// <param name="continueEditing">True if we need to stay on the same page or go back to list</param>
        /// <returns>Task</returns>
        protected async Task ResourceAddAsync(bool success, bool continueEditing)
        {
            if (Busy)
            {
                return;
            }

            if (!success)
            {
                // we still need to edit the model
                EditSuccessState.Success = false;
                Error = false;
                return;
            }

            Busy = true; //async

            if (Language is not null && AddResourceString is not null)
            {
                if (AddResourceString.ResourceName is null)
                {
                    return;
                }
                if (AddResourceString.ResourceValue is null)
                {
                    return;
                }

                AddResourceString.ResourceName = AddResourceString.ResourceName.Trim();
                AddResourceString.ResourceValue = AddResourceString.ResourceValue.Trim();

                //Datasource auto create the keys so res will never be null
                var res = await LocalizationService.GetLocaleStringResourceByNameAsync(AddResourceString.ResourceName, Language.Id);
                if (res == null)
                {
                    //fill entity from model
                    var resource = new LocaleStringResource()
                    {
                        ResourceName = AddResourceString.ResourceName,
                        ResourceValue = AddResourceString.ResourceValue,
                        LanguageId = Language.Id
                    };

                    await LocalizationService.InsertLocaleStringResourceAsync(resource);

                    EditSuccessState.Success = true;
                    Error = false;
                    Busy = false;

                    EditSuccessState.SuccessMessage = await LocalizationService.GetResourceAsync("Languages.Resources.Create.Success");

                    AddResourceString = new();
                    await ReloadAsync();
                }
                else
                {
                    res.ResourceValue = AddResourceString.ResourceValue;
                    await LocalizationService.UpdateLocaleStringResourceAsync(res);

                    EditSuccessState.Success = true;
                    Error = false;
                    Busy = false;

                    EditSuccessState.SuccessMessage = await LocalizationService.GetResourceAsync("Languages.Resources.Create.Success");

                    AddResourceString = new();
                    await ReloadAsync();
                    //EditSuccessState.Success = false;
                    //Error = true;
                    //ErrorMessage = string.Format(await LocalizationService.GetResourceAsync("Languages.Resources.NameAlreadyExists"), AddResourceString.ResourceName);

                    //Busy = false;
                    //return;
                }
            }
            else
            {
                EditSuccessState.Success = false;
                Error = true;
                ErrorMessage = await LocalizationService.GetResourceAsync("Languages.Resources.Create.Error");
                Busy = false;
            }
        }

        /// <summary>
        /// Update language resource
        /// </summary>
        /// <param name="success">Success when model is valid</param>
        /// <param name="continuEditing">True if we need to stay on the same page or go back to list</param>
        /// <param name="model">Language resource to update</param>
        /// <returns></returns>
        protected async Task ResourceUpdateAsync(bool success, bool continueEditing, LocaleResourceModel model)
        {
            if (Busy)
            {
                return;
            }

            if (!success)
            {
                EditSuccessState.Success = false;
                Error = false;
                return;
            }

            Busy = true;

            if (Language is not null && model is not null)
            {
                if (model.ResourceName is null)
                {
                    return;
                }

                if (model.ResourceValue is null)
                {
                    return;
                }

                model.ResourceName = model.ResourceName.Trim();
                model.ResourceValue = model.ResourceValue.Trim();

                var resource = await LocalizationService.GetLocaleStringResourceByIdAsync(model.Id);
                if (resource is not null)
                {
                    // if the resourceName changed, ensure it isn't being used by another resource
                    if (!resource.ResourceName.Equals(model.ResourceName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var res = await LocalizationService.GetLocaleStringResourceByNameAsync(model.ResourceName, Language.Id);
                        if (res != null && res.Id != resource.Id)
                        {
                            EditSuccessState.Success = false;
                            Error = true;
                            ErrorMessage = string.Format(await LocalizationService.GetResourceAsync("Languages.Resources.NameAlreadyExists"), res.ResourceName);
                            Busy = false;
                            return;
                        }
                    }

                    //fill entity from model
                    resource.ResourceName = model.ResourceName;
                    resource.ResourceValue = model.ResourceValue;

                    await LocalizationService.UpdateLocaleStringResourceAsync(resource);

                    EditSuccessState.Success = true;
                    Error = false;
                    Busy = false;

                    EditSuccessState.SuccessMessage = await LocalizationService.GetResourceAsync("Languages.Resources.Edit.Success");
                    await ReloadAsync();
                }
                else
                {
                    EditSuccessState.Success = false;
                    Error = true;
                    ErrorMessage = string.Format(await LocalizationService.GetResourceAsync("Languages.Resources.NotFound"), model.Id);
                    Busy = false;
                }
            }
            else
            {
                EditSuccessState.Success = false;
                Error = true;
                ErrorMessage = await LocalizationService.GetResourceAsync("Languages.Resources.Update.Error");
                Busy = false;
            }
        }

        /// <summary>
        /// Delete language resource
        /// </summary>
        /// <param name="resourceId">Language resource identifier to delete</param>
        /// <returns>Task</returns>
        protected async Task ResourceDelete(int resourceId)
        {
            if (Busy)
            {
                return;
            }

            Busy = true;

            var resource = await LocalizationService.GetLocaleStringResourceByIdAsync(resourceId);
            if (resource is not null)
            {
                await LocalizationService.DeleteLocaleStringResourceAsync(resource);

                EditSuccessState.Success = true;
                Error = false;
                Busy = false;

                EditSuccessState.SuccessMessage = await LocalizationService.GetResourceAsync("Languages.Resources.Delete.Success");
                await ReloadAsync();
            }
            else
            {
                EditSuccessState.Success = false;
                Error = true;
                ErrorMessage = string.Format(await LocalizationService.GetResourceAsync("Languages.Resources.NotFound"), resourceId);
                Busy = false;
            }
        }
    }
}
