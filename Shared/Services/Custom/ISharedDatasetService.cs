using System.Collections.Generic;
using BlazorDatasource.Shared.Domain.Custom;
using System.Threading.Tasks;

namespace BlazorDatasource.Shared.Services.Custom
{
    public partial interface ISharedDatasetService
    {
        Task InsertSharedDatasetAsync(SharedDataset sharedDataset);
        Task UpdateSharedDatasetAsync(SharedDataset sharedDataset);
        Task<IList<SharedDataset>> GetAllSharedDatasetsByUsernameAsync(string username);
        Task<IList<SharedDataset>> GetAllSharedDatasetsBySharedToAsync(string username);
    }
}
