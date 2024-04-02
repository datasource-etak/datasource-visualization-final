using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowInitializeRequest
    {
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("datasetId")]
        public Guid? DatasetId { get; set; }

        [JsonPropertyName("features")]
        public List<string>? Features { get; set; }

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("workflowTypeId")]
        public int? WorkflowTypeId { get; set; }
    }
}
