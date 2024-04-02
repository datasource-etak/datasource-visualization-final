using BlazorDatasource.Shared.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorDatasource.Shared.Domain.Custom;

namespace BlazorDatasource.Shared.Services.Custom
{
    public class SearchQueryService: ISearchQueryService
    {
        private readonly IRepository<SearchQuery> _searchQueryRepository;

        public SearchQueryService(IRepository<SearchQuery> searchQueryRepository)
        {
            _searchQueryRepository = searchQueryRepository;
        }

        public virtual async Task InsertSearchQueryAsync(SearchQuery searchQuery)
        {
            if (searchQuery == null)
                throw new ArgumentNullException(nameof(searchQuery));

            await _searchQueryRepository.InsertAsync(searchQuery);
        }

        public async Task<IList<SearchQuery>> GetAllSearchQueryByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            return await _searchQueryRepository.GetAllAsync(x => x.Where(y => y.CreatedBy == username));
        }
    }
}
