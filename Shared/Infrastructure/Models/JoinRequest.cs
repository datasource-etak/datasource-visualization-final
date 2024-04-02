using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class JoinRequest
    {

        [JsonPropertyName("datasets")]
        public List<JoinRequestDataset>? JoinDatasets { get; set; }

        [JsonPropertyName("type")]
        public string? JoinType { get; set; }

        [JsonPropertyName("alias")]
        public string? JoinAlias { get; set; }

        [JsonPropertyName("keys")]
        public List<string>? JoinKeys { get; set; }
    }
    public class JoinRequestDataset
    {
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("columns")]
        public Dictionary<string, Dictionary<string, string>>? Columns { get; set; }
    }

    
}
