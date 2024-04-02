using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public record DownloadedDatasetModel : BaseModel
    {
        [JsonPropertyName("dataset_description")]
        public string? DatasetDescription { get; set; }

        [JsonPropertyName("associated_filter")]
        public Dictionary<string, List<string>>? AssociatedFilter { get; set; }

        [JsonPropertyName("dataset_id")]
        public string? DatasetId { get; set; }

        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("dataset_name")]
        public string? DatasetName { get; set; }

        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("source_name")]
        public string? SourceName { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
