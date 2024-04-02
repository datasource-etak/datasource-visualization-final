using System.Collections.Generic;
using BlazorDatasource.Shared.Domain.Custom;
using System.Threading.Tasks;

namespace BlazorDatasource.Shared.Services.Custom
{
    public partial interface ISearchQueryService
    {
        Task InsertSearchQueryAsync(SearchQuery searchQuery);
        Task<IList<SearchQuery>> GetAllSearchQueryByUsernameAsync(string username);
    }
}
