using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Shared.Infrastructure.Models
{
    public class WorkflowInitializeResponse
    {
        [JsonPropertyName("uuid")]
        public Guid? Uuid { get; set; }

        [JsonPropertyName("stages")]
        public List<WorkflowInitializeStageResponse>? Stages { get; set; }
    }

    public class WorkflowInitializeStageResponse
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("ordering")]
        public int? Ordering { get; set; }

        [JsonPropertyName("minimumSelection")]
        public int? MinimumSelection { get; set; }

        [JsonPropertyName("maximumSelection")]
        public int? MaximumSelection { get; set; }

        [JsonPropertyName("workflowTypeId")]
        public int? WorkflowTypeId { get; set; }

        [JsonPropertyName("allowDuplicateOperatorsWithOtherParameters")]
        public bool? AllowDuplicateOperatorsWithOtherParameters { get; set; }

        [JsonPropertyName("operators")]
        public List<WorkflowInitializeOperatorResponse>? Operators { get; set; }
    }

    public class WorkflowInitializeOperatorResponse
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("sharedRecipeId")]
        public int? SharedRecipeId { get; set; }

        [JsonPropertyName("stageId")]
        public int? StageId { get; set; }

        [JsonPropertyName("parameters")]
        public List<WorkflowInitializeParameterResponse>? Parameters { get; set; }
    }

    public class WorkflowInitializeParameterResponse
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("operatorId")]
        public int? OperatorId { get; set; }

        [JsonPropertyName("restrictions")]
        public WorkflowInitializeRestrictionResponse? Restrictions { get; set; }
    }

    public class WorkflowInitializeRestrictionResponse
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("evaluate")]
        public WorkflowInitializeEvaluateResponse? Evaluate { get; set; }
    }

    public class WorkflowInitializeEvaluateResponse
    {
        [JsonPropertyName("maximum")]
        public double? Maximum { get; set; }

        [JsonPropertyName("minimum")]
        public double? Minimum { get; set; }

        [JsonPropertyName("choice_list")]
        public List<string>? ChoiceList { get; set; }
    }
}
