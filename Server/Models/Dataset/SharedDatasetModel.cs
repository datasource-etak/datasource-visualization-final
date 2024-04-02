using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public partial record SharedDatasetModel : BaseModel
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; } = default!;

        [Required]
        [JsonPropertyName("datasetId")]
        public Guid DatasetId { get; set; } = default!;

        [Required]
        [JsonPropertyName("datasetName")]
        public string DatasetName { get; set; } = default!;

        [Required]
        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; } = default!;

        [Required]
        [JsonPropertyName("sourceId")]
        public int SourceId { get; set; } = default!;

        [Required]
        [JsonPropertyName("datasetAlias")]
        public string DatasetAlias { get; set; } = default!;

        [Required]
        [JsonPropertyName("selectedFilters")]
        public string SelectedFilters { get; set; } = default!;

        [Required]
        [JsonPropertyName("xAxis")]
        public string XAxis { get; set; } = default!;

        [Required]
        [JsonPropertyName("yAxis")]
        public string YAxis { get; set; } = default!;

        [Required]
        [JsonPropertyName("chartType")]
        public string ChartType { get; set; } = default!;

        [Required]
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = default!;

        [Required]
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = default!;

        [Required]
        [JsonPropertyName("sharedTo")]
        public string SharedTo { get; set; } = default!;

        [Required]
        [JsonPropertyName("isGenerated")]
        public bool IsGenerated { get; set; } = default!;

        [Required]
        [JsonPropertyName("newDatasetId")]
        public Guid NewDatasetId { get; set; } = default!;

        [Required]
        [JsonPropertyName("newDatasetName")]
        public string NewDatasetName { get; set; } = default!;

        [Required]
        [JsonPropertyName("newDatasetAlias")]
        public string NewDatasetAlias { get; set; } = default!;

        [Required]
        [JsonPropertyName("generatedDate")]
        public DateTime GeneratedDate { get; set; } = default!;

        [Required]
        [JsonPropertyName("datasetExists")]
        public bool DatasetExists { get; set; } = default!;
    }
}
