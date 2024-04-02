using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowTypeResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
