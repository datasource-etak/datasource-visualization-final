using System;

namespace BlazorDatasource.Shared.Domain.Custom
{
    public class FavoriteDataset : BaseEntity
    {
        public Guid DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string DatasetAlias { get; set; }
        public string XAxis { get; set; }
        public string YAxis { get; set; }
        public string ChartType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

    }
}
