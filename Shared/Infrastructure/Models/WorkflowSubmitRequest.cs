using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowSubmitRequest
    {
        [JsonPropertyName("uuid")]
        public Guid? Uuid { get; set; }

        [JsonPropertyName("operators")]
        public List<WorkflowSubmitOperatorRequest>? Operators { get; set; }
    }

    public class WorkflowSubmitOperatorRequest
    {
        [JsonPropertyName("operatorId")]
        public int? OperatorId { get; set; }

        [JsonPropertyName("parameters")]
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
