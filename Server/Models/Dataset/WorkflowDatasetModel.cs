using BlazorDatasource.Shared.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public partial record WorkflowDatasetModel : BaseModel
    {
        [JsonPropertyName("needs_target")]
        public bool NeedsTarget { get; set; }

        [JsonPropertyName("datasets")]
        public List<WorkflowDatasetDetailModel>? WorkflowDatasetDetailModels { get; set; }
    }

    public partial record WorkflowDatasetDetailModel : BaseModel
    {
        [JsonPropertyName("dataset_description")]
        public string? DatasetDescription { get; set; }

        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("dataset_name")]
        public string? DatasetName { get; set; }

        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("columns")]
        public Dictionary<string, string>? Columns { get; set; }
    }
}
