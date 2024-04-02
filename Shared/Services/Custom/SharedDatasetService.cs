using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorDatasource.Shared.Domain.Custom;

namespace BlazorDatasource.Shared.Services.Custom
{
    public class SharedDatasetService : ISharedDatasetService
    {
        private readonly IRepository<SharedDataset> _sharedDatasetRepository;

        public SharedDatasetService(IRepository<SharedDataset> sharedDatasetRepository)
        {
            _sharedDatasetRepository = sharedDatasetRepository;
        }

        public virtual async Task InsertSharedDatasetAsync(SharedDataset sharedDataset)
        {
            if (sharedDataset == null)
                throw new ArgumentNullException(nameof(sharedDataset));

            var dbItem = await _sharedDatasetRepository
                .GetAllAsync(x => x.Where(
                    y => y.DatasetId == sharedDataset.DatasetId
                         && y.SourceName == sharedDataset.SourceName
                         && y.SourceId == sharedDataset.SourceId
                         && y.SelectedFilters == sharedDataset.SelectedFilters
                         && y.DatasetAlias == sharedDataset.DatasetAlias
                         && y.DatasetName == sharedDataset.DatasetName
                         && y.XAxis == sharedDataset.XAxis
                         && y.YAxis == sharedDataset.YAxis
                         && y.CreatedBy == sharedDataset.CreatedBy
                         && y.SharedTo == sharedDataset.SharedTo));

            if (!dbItem.Any())
            {
                await _sharedDatasetRepository.InsertAsync(sharedDataset);
            }
        }

        public virtual async Task UpdateSharedDatasetAsync(SharedDataset sharedDataset)
        {
            if (sharedDataset == null)
                throw new ArgumentNullException(nameof(sharedDataset));

            var dbItem = await _sharedDatasetRepository.GetByIdAsync(sharedDataset.Id);

            if (dbItem is null)
            {
                throw new NullReferenceException(nameof(sharedDataset));
            }

            dbItem.DatasetId = sharedDataset.DatasetId;
            dbItem.SourceName = sharedDataset.SourceName;
            dbItem.SourceId = sharedDataset.SourceId;
            dbItem.SelectedFilters = sharedDataset.SelectedFilters;
            dbItem.DatasetName = sharedDataset.DatasetName;
            dbItem.DatasetAlias = sharedDataset.DatasetAlias;
            dbItem.XAxis = sharedDataset.XAxis;
            dbItem.YAxis = sharedDataset.YAxis;
            dbItem.ChartType = sharedDataset.ChartType;
            dbItem.IsGenerated = sharedDataset.IsGenerated;
            dbItem.NewDatasetId = sharedDataset.NewDatasetId;
            dbItem.NewDatasetName = sharedDataset.NewDatasetName;
            dbItem.NewDatasetAlias = sharedDataset.NewDatasetAlias;
            dbItem.GeneratedDate = sharedDataset.GeneratedDate;
            dbItem.DatasetExists = sharedDataset.DatasetExists;

            await _sharedDatasetRepository.UpdateAsync(dbItem);
        }

        public async Task<IList<SharedDataset>> GetAllSharedDatasetsByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            return await _sharedDatasetRepository.GetAllAsync(x => x.Where(y => y.CreatedBy == username).OrderByDescending(x => x.CreatedDate));
        }

        public async Task<IList<SharedDataset>> GetAllSharedDatasetsBySharedToAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            return await _sharedDatasetRepository.GetAllAsync(x => x.Where(y => y.SharedTo == username).OrderByDescending(x => x.CreatedDate));
        }
    }
}
