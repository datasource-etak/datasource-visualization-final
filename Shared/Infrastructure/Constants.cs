using System.Collections.Generic;

namespace BlazorDatasource.Shared.Infrastructure
{
    public static class Constants
    {
        public static class ApiRoutePaths
        {
            public const string DiscoveryConfiguration = ".well-known/openid-configuration";

            public const string AccountPathPrefix = "account";

            public const string Authorize = AccountPathPrefix + "/authorize";
            public const string Register = AccountPathPrefix + "/register";

            public const string DatasetPathPrefix = "dataset";

            public const string AllDatasets = DatasetPathPrefix + "/all";
            public const string DatasetById = DatasetPathPrefix + "/fetch-by-id";
            public const string DatasetByAlias = DatasetPathPrefix + "/fetch-by-alias";
            public const string AllDatasetsForSpecificFamily = DatasetPathPrefix + "/family";
            public const string ViewDataset = DatasetPathPrefix + "/view";
            public const string DatasetSourceInfoById = DatasetPathPrefix + "/source-info";

            public const string SearchDataset = DatasetPathPrefix + "/search";
            public const string GetDatasetFilters = DatasetPathPrefix + "/search/filters";
            public const string DownloadDataset = DatasetPathPrefix + "/download";
            public const string JoinDataset = DatasetPathPrefix + "/join";
            public const string JoinDatasetTypes = DatasetPathPrefix + "/join/types";
            public const string JoinDatasetDatatypes = DatasetPathPrefix + "/join/datatypes";
            public const string DatasetStatus = DatasetPathPrefix + "/status";

            public const string WorkflowTypes = "workflow/types";
            public const string WorkflowDatasets = "workflow/datasets";
            public const string WorkflowInitialize = "workflow/initialize";
            public const string WorkflowSubmit = "workflow/submit";
            public const string WorkflowStatus = "workflow/status";
            public const string WorkflowProgress = "workflow/progress";
            public const string ViewWorkflow = "workflow/view";
            public const string AllWorkflows= "workflow/all";
        }

        public static class TokenTypes
        {
            public const string RefreshToken = "refresh_token";
            public const string AccessToken = "access_token";
        }

        public static readonly List<string> SupportedTokenTypes = new()
        {
            TokenTypes.RefreshToken,
            TokenTypes.AccessToken
        };
    }
}
