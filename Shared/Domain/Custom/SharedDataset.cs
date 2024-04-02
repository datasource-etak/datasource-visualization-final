using System;

namespace BlazorDatasource.Shared.Domain.Custom
{
    public class SharedDataset : BaseEntity
    {
        public Guid DatasetId { get; set; }
        public string DatasetName { get; set; }
        public string SourceName { get; set; }
        public int SourceId { get; set; }
        public string DatasetAlias { get; set; }
        public string SelectedFilters { get; set; }
        public string XAxis { get; set; }
        public string YAxis { get; set; }
        public string ChartType { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string SharedTo { get; set; }
        public bool IsGenerated { get; set; }
        public Guid NewDatasetId { get; set; }
        public string NewDatasetName { get; set; }
        public string NewDatasetAlias { get; set; }
        public DateTime GeneratedDate { get; set; }
        public bool DatasetExists { get; set; }
    }
}
