using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class DatasetSourceInfoResponse
    {
        [JsonPropertyName("getAllDatasetIdsUrl")]
        public string GetAllDatasetIdsUrl { get; set; }
        
        [JsonPropertyName("parseAllDatasetIdsResponsePath")]
        public string ParseAllDatasetIdsResponsePath { get; set; }

        [JsonPropertyName("sourceName")]
        public string SourceName { get; set; }

        [JsonPropertyName("sourceId")]
        public int SourceId { get; set; }

        [JsonPropertyName("filters")]
        public string Filters { get; set; }

        [JsonPropertyName("download")]
        public string Download { get; set; }
    }
}
