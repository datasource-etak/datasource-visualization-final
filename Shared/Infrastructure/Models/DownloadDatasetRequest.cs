using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    /// <summary>
    /// Represents a download data request
    /// </summary>
    public class DownloadDatasetRequest
    {
        [Required]
        [JsonPropertyName("source_id")]
        public string SourceId { get; set; } = default!;

        [Required]
        [JsonPropertyName("dataset_id")]
        public string DatasetId { get; set; } = default!;

        [Required]
        [JsonPropertyName("filterRequest")]
        public FilterRequest FilterRequest { get; set; } = new();
    }

    public class FilterRequest
    {
        [Required]
        [JsonPropertyName("selectedFilters")]
        public Dictionary<string, List<string>> SelectedFilters { get; set; } = new();
        
        [Required]
        [JsonPropertyName("alias")]
        public string Alias { get; set; }
    }
}
