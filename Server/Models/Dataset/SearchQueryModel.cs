using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public partial record SearchQueryModel : BaseModel
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; } = default!;

        [Required]
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; } = default!;

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;

        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;

        [Required]
        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } = default!;

        [Required]
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = default!;
    }
}
