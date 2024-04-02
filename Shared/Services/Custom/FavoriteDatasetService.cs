using BlazorDatasource.Shared.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorDatasource.Shared.Domain.Custom;

namespace BlazorDatasource.Shared.Services.Custom
{
    public class FavoriteDatasetService : IFavoriteDatasetService
    {
        private readonly IRepository<FavoriteDataset> _favoriteDatasetRepository;

        public FavoriteDatasetService(IRepository<FavoriteDataset> favoriteDatasetRepository)
        {
            _favoriteDatasetRepository = favoriteDatasetRepository;
        }

        public virtual async Task InsertFavoriteDatasetAsync(FavoriteDataset favoriteDataset)
        {
            if (favoriteDataset == null)
                throw new ArgumentNullException(nameof(favoriteDataset));

            var dbItem = await _favoriteDatasetRepository
                .GetAllAsync(x => x.Where(
                    y => y.DatasetId == favoriteDataset.DatasetId 
                         && y.DatasetAlias == favoriteDataset.DatasetAlias
                         && y.DatasetName == favoriteDataset.DatasetName
                         && y.XAxis == favoriteDataset.XAxis
                         && y.YAxis == favoriteDataset.YAxis
                         && y.ChartType == favoriteDataset.ChartType
                         && y.CreatedBy == favoriteDataset.CreatedBy));

            if (!dbItem.Any())
            {
                await _favoriteDatasetRepository.InsertAsync(favoriteDataset);
            }
        }

        public async Task<IList<FavoriteDataset>> GetAllFavoriteDatasetsByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            return await _favoriteDatasetRepository.GetAllAsync(x => x.Where(y => y.CreatedBy == username).OrderByDescending(x => x.CreatedDate));
        }
    }
}
