using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorDatasource.Server.Models.Dataset
{
    public partial record WorkflowInitializeModel : BaseModel
    {
        [JsonPropertyName("uuid")]
        public Guid? Uuid { get; set; }

        [JsonPropertyName("stages")]
        public List<WorkflowInitializeStageModel>? Stages { get; set; }
    }

    public partial record WorkflowInitializeStageModel : BaseModel
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
        public List<WorkflowInitializeOperatorModel>? Operators { get; set; }
    }

    public partial record WorkflowInitializeOperatorModel : BaseModel
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
        public List<WorkflowInitializeParameterModel>? Parameters { get; set; }
    }

    public partial record WorkflowInitializeParameterModel : BaseModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("operatorId")]
        public int? OperatorId { get; set; }

        [JsonPropertyName("restrictions")]
        public WorkflowInitializeRestrictionModel? Restrictions { get; set; }
    }

    public partial record WorkflowInitializeRestrictionModel : BaseModel
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("evaluate")]
        public WorkflowInitializeEvaluateModel? Evaluate { get; set; }
    }

    public partial record WorkflowInitializeEvaluateModel : BaseModel
    {
        [JsonPropertyName("maximum")]
        public double? Maximum { get; set; }

        [JsonPropertyName("minimum")]
        public double? Minimum { get; set; }

        [JsonPropertyName("choice_list")]
        public List<string>? ChoiceList { get; set; }
    }
}
