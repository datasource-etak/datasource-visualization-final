using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public record WorkflowModel : BaseModel
    {
        [JsonPropertyName("result")]
        public List<WorkflowResultModel>? Result { get; set; }

        [JsonPropertyName("operators")]
        public List<List<WorkflowOperatorModel>>? Operators { get; set; }
    }

    public record WorkflowResultModel : BaseModel
    {
        [JsonPropertyName("data")]
        public List<Dictionary<string, object>>? Data { get; set; }

        [JsonPropertyName("uuid")]
        public Guid? Uuid { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public record WorkflowOperatorModel : BaseModel
    {
        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("uuid")]
        public Guid? Uuid { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
