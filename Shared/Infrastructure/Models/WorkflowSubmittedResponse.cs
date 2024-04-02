using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowSubmittedResponse
    {
        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("alias")]
        public string Alias { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("datasetId")]
        public Guid DatasetId { get; set; }

        [JsonPropertyName("features")]
        public List<string> Features { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("submittedAt")]
        public string SubmittedAt { get; set; }

        [JsonPropertyName("completedAt")]
        public string CompletedAt { get; set; }

        [JsonPropertyName("workflowTypeId")]
        public int WorkflowTypeId { get; set; }
    }
}
