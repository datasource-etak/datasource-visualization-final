using System.Collections.Generic;
using BlazorDatasource.Shared.Domain.Custom;
using System.Threading.Tasks;

namespace BlazorDatasource.Shared.Services.Custom
{
    public partial interface ISourceService
    {
        Task InsertSourcesAsync(List<Source> sources);
        Task<IList<Source>> GetAllAsync();
    }
}
