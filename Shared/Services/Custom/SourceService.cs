using BlazorDatasource.Shared.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorDatasource.Shared.Domain.Custom;

namespace BlazorDatasource.Shared.Services.Custom
{
    public class SourceService: ISourceService
    {
        private readonly IRepository<Source> _sourceRepository;

        public SourceService(IRepository<Source> sourceRepository)
        {
            _sourceRepository = sourceRepository;
        }

        public async Task InsertSourcesAsync(List<Source> sources)
        {
            var dbSources = await _sourceRepository.GetAllAsync((Func<IQueryable<Source>, IQueryable<Source>>)null, true);
            await _sourceRepository.InsertRangeAsync(sources.Except(dbSources, new SourceComparer()).ToList());
        }

        public async Task<IList<Source>> GetAllAsync()
        {
            return await _sourceRepository.GetAllAsync((Func<IQueryable<Source>, IQueryable<Source>>)null, true);
        }
    }
    public class SourceComparer : IEqualityComparer<Source>
    {
        public bool Equals(Source x, Source y)
        {
            if (x == null && y == null)
                return true;
            else if (x == null || y == null)
                return false;
            else
                return x.Name == y.Name && x.SourceId == y.SourceId;
        }

        public int GetHashCode(Source obj)
        {
            return obj.Name.GetHashCode() ^ obj.SourceId.GetHashCode();
        }
    }
}
