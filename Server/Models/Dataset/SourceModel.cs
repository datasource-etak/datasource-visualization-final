using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public partial record SourceModel : BaseModel
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; } = default!;

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [Required]
        [JsonPropertyName("sourceId")]
        public string SourceId { get; set; } = default!;

        [Required]
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = default!;
    }
}
