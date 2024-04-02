using System.Collections.Generic;
using BlazorDatasource.Shared.Domain.Custom;
using System.Threading.Tasks;

namespace BlazorDatasource.Shared.Services.Custom
{
    public partial interface IFavoriteDatasetService
    {
        Task InsertFavoriteDatasetAsync(FavoriteDataset favoriteDataset);
        Task<IList<FavoriteDataset>> GetAllFavoriteDatasetsByUsernameAsync(string username);
    }
}
