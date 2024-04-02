using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowDatasetResponse
    {
        [JsonPropertyName("needs_target")]
        public bool NeedsTarget { get; set; }

        [JsonPropertyName("datasets")]
        public List<WorkflowDatasetDetailResponse> WorkflowDatasetDetailResponses { get; set; }
    }

    public class WorkflowDatasetDetailResponse
    {
        [JsonPropertyName("dataset_description")]
        public string DatasetDescription { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

        [JsonPropertyName("dataset_name")]
        public string DatasetName { get; set; }

        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("columns")]
        public Dictionary<string, string> Columns { get; set; }
    }
}
