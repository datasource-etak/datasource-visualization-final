using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Common
{
    public class JoinDataset
    {
        public Guid DatasetId { get; set; }
        public List<JoinDatasetKey>? NewKeys { get; set; }
    }

    public class JoinDatasetKey
    {
        public string? OriginalKey { get; set; }
        public string? NewKey { get; set; }
        public string? NewDatatype { get; set; }
    }
}
