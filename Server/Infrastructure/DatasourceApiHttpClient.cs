using System;
using Azure.Core;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Infrastructure.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorDatasource.Server.Infrastructure
{
    /// <summary>
    /// Represents the HTTP client to request our own Datasource api found at project BlazorDatasource.Api
    /// </summary>
    public partial class DatasourceApiHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly TokenProvider _tokenProvider;

        #endregion

        #region Ctor

        public DatasourceApiHttpClient(HttpClient client,
                                       TokenProvider tokenProvider)
        {
            _httpClient = client;
            _tokenProvider = tokenProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create account
        /// </summary>
        /// <param name="model">SignUpRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<HttpResponseMessage> CreateAccount(SignUpRequest model)
        {
            return await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.Register, model);
        }

        /// <summary>
        /// Sign in with account
        /// </summary>
        /// <param name="model">SignInRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<ServiceResponse<SignInResponse?>> SignInAccount(SignInRequest model)
        {
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.Authorize, value: model);
            if (result.IsSuccessStatusCode)
            {
                return new ServiceResponse<SignInResponse?>()
                {
                    Data = await result.Content.ReadFromJsonAsync<SignInResponse>(),
                    Success = result.IsSuccessStatusCode,
                    Message = result.ReasonPhrase ?? string.Empty
                };
            }
            else
            {
                return new ServiceResponse<SignInResponse?>()
                {
                    Data = null,
                    Success = result.IsSuccessStatusCode,
                    Message = result.ReasonPhrase ?? string.Empty
                };
            }

        }

        /// <summary>
        /// Search datasets
        /// </summary>
        /// <param name="model">SearchRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<HttpResponseMessage> SearchDatasets(SearchRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.SearchDataset, value: model);
            return result;
        }

        /// <summary>
        /// Get the filters for a specific dataset result from the search
        /// </summary>
        /// <param name="model">SearchFiltersRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<HttpResponseMessage> GetSearchFilters(SearchFiltersRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.GetDatasetFilters, value: model);
            return result;
        }

        /// <summary>
        /// Downloads a dataset
        /// </summary>
        /// <param name="model">DownloadDatasetRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<HttpResponseMessage> DownloadDataset(DownloadDatasetRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.DownloadDataset, value: model);
            return result;
        }

        /// <summary>
        /// Gets all the downloaded datasets for a specific user
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<HttpResponseMessage> GetAllDownloadedDatasets()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.GetAsync(requestUri: Constants.ApiRoutePaths.AllDatasets);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<HttpResponseMessage> GetDatasetByUUId(ViewDatasetRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.ViewDataset, value: model);
            return result;
        }

        public virtual async Task<HttpResponseMessage> GetJoinTypes()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.GetAsync(requestUri: Constants.ApiRoutePaths.JoinDatasetTypes);
            return result;
        }

        public virtual async Task<HttpResponseMessage> GetJoinDatatypes()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.GetAsync(requestUri: Constants.ApiRoutePaths.JoinDatasetDatatypes);
            return result;
        }
        public virtual async Task<HttpResponseMessage> JoinDataset(JoinRequest request)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.JoinDataset, value: request);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetDatasetStatusByUUId(ViewDatasetRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.DatasetStatus, value: model);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetDatasetById(string datasetId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.DatasetById, value: datasetId);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetDatasetByAlias(string datasetAlias)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.DatasetById, value: datasetAlias);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetDatasetSourceInfoById(string datasetId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.DatasetSourceInfoById, value: datasetId);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetWorkflowTypes()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.GetAsync(requestUri: Constants.ApiRoutePaths.WorkflowTypes);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetWorkflowDatasetsById(string? workflowId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.WorkflowDatasets, value: workflowId);
            return result;
        }
        public virtual async Task<HttpResponseMessage> InitializeWorkflow(WorkflowInitializeRequest request)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.WorkflowInitialize, value: request);
            return result;
        }
        public virtual async Task<HttpResponseMessage> SubmitWorkflow(WorkflowSubmitRequest request)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.WorkflowSubmit, value: request);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetWorkflowStatusByUuid(ViewWorkflowRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.WorkflowStatus, value: model);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetWorkflowProgressByUuid(ViewWorkflowRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.WorkflowProgress, value: model);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetWorkflowByUuid(ViewWorkflowRequest model)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.PostAsJsonAsync(requestUri: Constants.ApiRoutePaths.ViewWorkflow, value: model);
            return result;
        }
        public virtual async Task<HttpResponseMessage> GetAllWorkflows()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenProvider.AccessToken);
            var result = await _httpClient.GetAsync(requestUri: Constants.ApiRoutePaths.AllWorkflows);
            return result;
        }

        #endregion
    }
}
