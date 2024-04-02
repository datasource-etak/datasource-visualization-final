using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowResponse
    {
        [JsonPropertyName("result")]
        public List<WorkflowResult> Result { get; set; }

        [JsonPropertyName("operators")]
        public List<List<WorkflowOperator>> Operators { get; set; }
    }

    public class WorkflowResult
    {
        [JsonPropertyName("data")]
        public List<Dictionary<string, object>> Data { get; set; }

        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class WorkflowOperator
    {
        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
