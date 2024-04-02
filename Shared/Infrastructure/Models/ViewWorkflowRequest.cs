using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class ViewWorkflowRequest
    {
        [Required]
        [JsonPropertyName("uuid")]
        public string? WorkflowUuid { get; set; } = default!;
    }
}
