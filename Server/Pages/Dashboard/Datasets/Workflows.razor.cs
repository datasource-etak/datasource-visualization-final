using System;
using BlazorDatasource.Server.Infrastructure;
using BlazorDatasource.Server.Models.Dataset;
using BlazorDatasource.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorDatasource.Server.Helpers;
using System.Linq;
using System.Text.Json;
using BlazorDatasource.Server.Models.Common;
using BlazorDatasource.Server.Models.Extensions;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;

namespace BlazorDatasource.Server.Pages.Dashboard.Datasets
{
    public partial class Workflows : ComponentBase
    {
        [Inject] protected DatasourceApiHttpClient Client { get; set; } = default!;

        [Inject] protected MainHelper MainHelper { get; set; } = default!;

        [Inject] protected EditSuccess EditSuccessState { get; set; } = default!;

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

        protected bool Loading { get; set; }

        protected bool Error;

        protected string ErrorMessage = string.Empty;

        protected List<WorkflowTypeModel>? WorkflowTypeModels { get; set; } = new();

        protected int? SelectedWorkflowTypeId { get; set; }

        protected List<WorkflowDatasetDetailModel>? WorkflowDatasetDetailModels { get; set; } = new();

        protected Guid? SelectedWorkflowDatasetDetailModelUuid { get; set; }

        protected WorkflowInitializeRequest WorkflowInitializeRequest { get; set; } = new();

        protected WorkflowInitializeModel? WorkflowInitializeModel { get; set; }

        protected WorkflowSubmitRequest WorkflowSubmitRequest { get; set; } = new();

        protected Dictionary<int?, string?> InputSelections = new();

        protected override async Task OnInitializedAsync()
        {
            if (Loading)
            {
                return;
            }

            Loading = true;

            var workflowTypesResponse = await Client.GetWorkflowTypes();
            if (workflowTypesResponse.IsSuccessStatusCode)
            {
                WorkflowTypeModels = await workflowTypesResponse.Content.ReadFromJsonAsync<List<WorkflowTypeModel>>();
                if (WorkflowTypeModels is null)
                {
                    return;
                }

                //test
                //WorkflowInitializeModel = JsonSerializer.Deserialize<WorkflowInitializeModel>(
                //    "{\r\n    \"stages\": [\r\n        {\r\n            \"id\": 1,\r\n            \"name\": \"Dataset Split\",\r\n            \"description\": \"If needed, choose an operator to split your dataset into train and test\",\r\n            \"ordering\": 1,\r\n            \"minimumSelection\": 0,\r\n            \"maximumSelection\": 1,\r\n            \"workflowTypeId\": 1,\r\n            \"allowDuplicateOperatorsWithOtherParameters\": false,\r\n            \"operators\": [\r\n                {\r\n                    \"id\": 1,\r\n                    \"name\": \"TrainTestSplit\",\r\n                    \"description\": \"Use to split data into train and test set.\",\r\n                    \"sharedRecipeId\": 9,\r\n                    \"stageId\": 1,\r\n                    \"parameters\": [\r\n                        {\r\n                            \"id\": 1,\r\n                            \"name\": \"test_size\",\r\n                            \"type\": \"double\",\r\n                            \"operatorId\": 1,\r\n                            \"restrictions\": {\r\n                                \"type\": \"range\",\r\n                                \"evaluate\": {\r\n                                    \"maximum\": 0.99,\r\n                                    \"minimum\": 0.01\r\n                                }\r\n                            }\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"id\": 2,\r\n            \"name\": \"Dataset Scale\",\r\n            \"description\": \"If needed, choose an operator to scale your dataset(s)\",\r\n            \"ordering\": 2,\r\n            \"minimumSelection\": 1,\r\n            \"maximumSelection\": 1,\r\n            \"workflowTypeId\": 1,\r\n            \"allowDuplicateOperatorsWithOtherParameters\": false,\r\n            \"operators\": [\r\n                {\r\n                    \"id\": 2,\r\n                    \"name\": \"StandardScale\",\r\n                    \"description\": \"Use to standardize your data to N(0,1)\",\r\n                    \"sharedRecipeId\": 10,\r\n                    \"stageId\": 2,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 3,\r\n                    \"name\": \"MinmaxScale\",\r\n                    \"description\": \"Use to standardize your data between 0, 1\",\r\n                    \"sharedRecipeId\": 11,\r\n                    \"stageId\": 2,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 4,\r\n                    \"name\": \"NoScale\",\r\n                    \"description\": \"Use if you do not want to scale your data.\",\r\n                    \"sharedRecipeId\": 12,\r\n                    \"stageId\": 2,\r\n                    \"parameters\": []\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"id\": 3,\r\n            \"name\": \"Dataset Statistics\",\r\n            \"description\": \"If needed, choose one or more operators for statistical analysis on your train set\",\r\n            \"ordering\": 3,\r\n            \"minimumSelection\": 1,\r\n            \"maximumSelection\": 4,\r\n            \"workflowTypeId\": 1,\r\n            \"allowDuplicateOperatorsWithOtherParameters\": false,\r\n            \"operators\": [\r\n                {\r\n                    \"id\": 5,\r\n                    \"name\": \"Describe\",\r\n                    \"description\": \"Returns basic metrics per column (min, max).\",\r\n                    \"sharedRecipeId\": 13,\r\n                    \"stageId\": 3,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 6,\r\n                    \"name\": \"StatisticsCorr\",\r\n                    \"description\": \"Returns column correlation. Set with_target to true, if you need correlation between the target any other column. If false, computes correlation between each column pair.\",\r\n                    \"sharedRecipeId\": 14,\r\n                    \"stageId\": 3,\r\n                    \"parameters\": [\r\n                        {\r\n                            \"id\": 2,\r\n                            \"name\": \"with_target\",\r\n                            \"type\": \"boolean\",\r\n                            \"operatorId\": 6,\r\n                            \"restrictions\": {}\r\n                        }\r\n                    ]\r\n                },\r\n                {\r\n                    \"id\": 7,\r\n                    \"name\": \"StatisticsKurtosis\",\r\n                    \"description\": \"Returns train dataset kurtosis per column.\",\r\n                    \"sharedRecipeId\": 15,\r\n                    \"stageId\": 3,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 8,\r\n                    \"name\": \"StatisticsSkew\",\r\n                    \"description\": \"Returns train dataset skew per column.\",\r\n                    \"sharedRecipeId\": 16,\r\n                    \"stageId\": 3,\r\n                    \"parameters\": []\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"id\": 4,\r\n            \"name\": \"Regression Models\",\r\n            \"description\": \"Choose one or more regression models to examine. You may choose more instances of the same model with different parameters.\",\r\n            \"ordering\": 4,\r\n            \"minimumSelection\": 1,\r\n            \"maximumSelection\": -1,\r\n            \"workflowTypeId\": 1,\r\n            \"allowDuplicateOperatorsWithOtherParameters\": true,\r\n            \"operators\": [\r\n                {\r\n                    \"id\": 9,\r\n                    \"name\": \"LinearRegression\",\r\n                    \"description\": \"Use for a linear regression model\",\r\n                    \"sharedRecipeId\": 17,\r\n                    \"stageId\": 4,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 10,\r\n                    \"name\": \"PanelOls\",\r\n                    \"description\": \"Use for a panel OLS model with using entity or time effects.\",\r\n                    \"sharedRecipeId\": 18,\r\n                    \"stageId\": 4,\r\n                    \"parameters\": [\r\n                        {\r\n                            \"id\": 4,\r\n                            \"name\": \"entity_effects\",\r\n                            \"type\": \"boolean\",\r\n                            \"operatorId\": 10,\r\n                            \"restrictions\": {}\r\n                        },\r\n                        {\r\n                            \"id\": 5,\r\n                            \"name\": \"time_effects\",\r\n                            \"type\": \"boolean\",\r\n                            \"operatorId\": 10,\r\n                            \"restrictions\": {}\r\n                        },\r\n                        {\r\n                            \"id\": 3,\r\n                            \"name\": \"time_column\",\r\n                            \"type\": \"string\",\r\n                            \"operatorId\": 10,\r\n                            \"restrictions\": {\r\n                                \"type\": \"from_list\",\r\n                                \"evaluate\": {\r\n                                    \"choice_list\": [\r\n                                        \"country\",\r\n                                        \"year\"\r\n                                    ]\r\n                                }\r\n                            }\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"id\": 5,\r\n            \"name\": \"Regression Metrics\",\r\n            \"description\": \"Choose one or more metrics to evaluate your regression models.\",\r\n            \"ordering\": 5,\r\n            \"minimumSelection\": 1,\r\n            \"maximumSelection\": 3,\r\n            \"workflowTypeId\": 1,\r\n            \"allowDuplicateOperatorsWithOtherParameters\": false,\r\n            \"operators\": [\r\n                {\r\n                    \"id\": 11,\r\n                    \"name\": \"MeanSquaredError\",\r\n                    \"description\": \"Use for computing the MSE between predictions and targets.\",\r\n                    \"sharedRecipeId\": 19,\r\n                    \"stageId\": 5,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 12,\r\n                    \"name\": \"MetricsRSquared\",\r\n                    \"description\": \"Use for computing the R2 between predictions and targets.\",\r\n                    \"sharedRecipeId\": 20,\r\n                    \"stageId\": 5,\r\n                    \"parameters\": []\r\n                },\r\n                {\r\n                    \"id\": 13,\r\n                    \"name\": \"RootMeanSquaredError\",\r\n                    \"description\": \"Use for computing the RMSE between predictions and targets.\",\r\n                    \"sharedRecipeId\": 21,\r\n                    \"stageId\": 5,\r\n                    \"parameters\": []\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"uuid\": \"d59781d1-980e-4c48-8307-c6d7ec1d0a71\"\r\n}");
                //foreach (var stage in WorkflowInitializeModel!.Stages!)
                //{
                //    foreach (var op in stage.Operators!)
                //    {
                //        foreach (var param in op.Parameters!)
                //        {
                //            InputSelections.Add(param.Id, null);
                //        }
                //    }
                //}
            }
            else
            {
                Error = true;
                ErrorMessage = workflowTypesResponse.ReasonPhrase ?? T["Workflows.Types.Error"];
            }

            Loading = false;
        }

        public async Task Test()
        {

        }

        protected async Task OnSelectedWorkflowTypeChange(ChangeEventArgs e)
        {
            if (e.Value is null or "null")
            {
                SelectedWorkflowTypeId = null;
                SelectedWorkflowDatasetDetailModelUuid = null;
                WorkflowInitializeRequest = new WorkflowInitializeRequest();
                WorkflowDatasetDetailModels = new List<WorkflowDatasetDetailModel>();
            }
            else
            {
                SelectedWorkflowTypeId = Convert.ToInt32(e.Value);
                WorkflowInitializeRequest.WorkflowTypeId = SelectedWorkflowTypeId;

                var response = await Client.GetWorkflowDatasetsById(SelectedWorkflowTypeId.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var workflowDatasetModel = await response.Content.ReadFromJsonAsync<WorkflowDatasetModel>();
                    if (workflowDatasetModel is not null)
                    {
                        WorkflowDatasetDetailModels = workflowDatasetModel.WorkflowDatasetDetailModels;
                    }
                }
                else
                {
                    Error = true;
                    ErrorMessage = response.ReasonPhrase ?? T["Workflows.Types.Error"];
                }
            }

        }

        protected void OnSelectWorkflowFeatureChanged(ChangeEventArgs e)
        {
            WorkflowInitializeRequest.Features?.Clear();

            var input = e.Value as string[];

            foreach (var value in input!)
            {
                WorkflowInitializeRequest.Features ??= new List<string>();
                WorkflowInitializeRequest.Features.Add(value);
            }
        }

        protected void OnSelectedWorkflowDatasetDetailModelChange(ChangeEventArgs e)
        {
            if (e.Value is null or "null")
            {
                SelectedWorkflowDatasetDetailModelUuid = null;
                WorkflowInitializeRequest = new WorkflowInitializeRequest();
            }
            else
            {
                SelectedWorkflowDatasetDetailModelUuid = Guid.Parse(e.Value!.ToString()!);
                WorkflowInitializeRequest.Alias = null;
                WorkflowInitializeRequest.Description = null;
                WorkflowInitializeRequest.Features = null;
                WorkflowInitializeRequest.Target = null;

            }

            WorkflowInitializeRequest.DatasetId = SelectedWorkflowDatasetDetailModelUuid;
        }

        protected bool ShowInitializeButton()
        {
            if (WorkflowInitializeModel is not null)
            {
                return false;
            }

            if (WorkflowInitializeRequest.Alias is not null &&
                WorkflowInitializeRequest.Description is not null &&
                WorkflowInitializeRequest.DatasetId is not null &&
                WorkflowInitializeRequest.Features is not null &&
                WorkflowInitializeRequest.Target is not null &&
                WorkflowInitializeRequest.WorkflowTypeId is not null)
            {
                if (WorkflowInitializeRequest.Features.Count > 0 && WorkflowInitializeRequest.Target != "null")
                {
                    return true;
                }
            }

            return false;
        }

        protected async Task OnInitializeWorkflowClick(EventArgs e)
        {
            if (Loading)
            {
                return;
            }

            Loading = true;

            var workflowInitiliazeResponse = await Client.InitializeWorkflow(WorkflowInitializeRequest);
            if (workflowInitiliazeResponse.IsSuccessStatusCode)
            {
                WorkflowInitializeModel = await workflowInitiliazeResponse.Content.ReadFromJsonAsync<WorkflowInitializeModel>();
                if (WorkflowInitializeModel is null)
                {
                    return;
                }

                WorkflowSubmitRequest.Uuid = WorkflowInitializeModel.Uuid;
            }
            else
            {
                Error = true;
                ErrorMessage = workflowInitiliazeResponse.ReasonPhrase ?? T["Workflows.Initialize.Error"];
            }

            foreach (var stage in WorkflowInitializeModel!.Stages!)
            {
                foreach (var op in stage.Operators!)
                {
                    foreach (var param in op.Parameters!)
                    {
                        InputSelections.Add(param.Id, null);
                    }
                }
            }

            Loading = false;
        }

        protected async Task OnSubmitWorkflowClick(EventArgs e)
        {
            if (Loading)
            {
                return;
            }

            Loading = true;

            var json = JsonSerializer.Serialize(WorkflowSubmitRequest);

            var workflowSubmitResponse = await Client.SubmitWorkflow(WorkflowSubmitRequest);
            if (workflowSubmitResponse.IsSuccessStatusCode)
            {
                var response = await workflowSubmitResponse.Content.ReadFromJsonAsync<string>();
                if (response == string.Empty)
                {
                    return;
                }

                NavigationManager.NavigateTo($"/dashboard/datasets/workflows/view/" + response);
            }
            else
            {
                Error = true;
                ErrorMessage = workflowSubmitResponse.ReasonPhrase ?? T["Workflows.Submit.Error"];
            }

            Loading = false;
        }

        protected async Task OnOperatorClicked(WorkflowInitializeOperatorModel workflowInitializeOperatorModel)
        {
            WorkflowSubmitRequest.Operators ??= new List<WorkflowSubmitOperatorRequest>();
            var stage = WorkflowInitializeModel?.Stages
                ?.First(i => i.Id == workflowInitializeOperatorModel.StageId);

            if (WorkflowSubmitRequest.Operators.Any(x => x.OperatorId == workflowInitializeOperatorModel.Id))
            {
                if (stage!.AllowDuplicateOperatorsWithOtherParameters == false)
                {
                    return;
                }
            }

            if (workflowInitializeOperatorModel.Parameters is { Count: 0 })
            {
                WorkflowSubmitRequest.Operators.Add(new WorkflowSubmitOperatorRequest
                {
                    OperatorId = workflowInitializeOperatorModel.Id,
                    Parameters = null
                });
            }
            else
            {
                var parameters = new Dictionary<string, object>();
                var foundMissingParameterSelection = false;

                foreach (var parameter in workflowInitializeOperatorModel.Parameters!)
                {
                    if (InputSelections[parameter.Id] is null || InputSelections[parameter.Id] == string.Empty)
                    {
                        foundMissingParameterSelection = true;
                        break;
                    }

                    if (parameter.Type == "double")
                    {
                        if (parameter.Restrictions!.Evaluate is not null)
                        {
                            double paramValue = double.Parse(InputSelections[parameter.Id]!, CultureInfo.InvariantCulture);
                            if (paramValue < parameter.Restrictions.Evaluate.Minimum ||
                                paramValue > parameter.Restrictions.Evaluate.Maximum)
                            {
                                foundMissingParameterSelection = true;
                                break;
                            }

                            parameters.Add(parameter.Name!, double.Parse(InputSelections[parameter.Id]!, CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            parameters.Add(parameter.Name!, double.Parse(InputSelections[parameter.Id]!, CultureInfo.InvariantCulture));
                        }
                    }
                    else if (parameter.Type == "boolean")
                    {
                        parameters.Add(parameter.Name!, Convert.ToBoolean(InputSelections[parameter.Id]));
                    }
                    else
                    {
                        parameters.Add(parameter.Name!, InputSelections[parameter.Id]!);
                    }
                }

                if (foundMissingParameterSelection)
                {
                    return;
                }

                WorkflowSubmitRequest.Operators.Add(new WorkflowSubmitOperatorRequest
                {
                    OperatorId = workflowInitializeOperatorModel.Id,
                    Parameters = parameters
                });
            }
        }

        protected async Task OnOperatorRemoveClicked(int? operatorId)
        {
            WorkflowSubmitRequest.Operators?.Remove(WorkflowSubmitRequest.Operators.First(x => x.OperatorId == operatorId));
        }

        protected bool ShowSelectButton(WorkflowInitializeOperatorModel workflowInitializeOperatorModel)
        {
            var stage = WorkflowInitializeModel?.Stages
                ?.First(i => i.Id == workflowInitializeOperatorModel.StageId);

            var currentOperatorsOfStage = WorkflowSubmitRequest.Operators?
                .Where(x => stage!.Operators!.Any(i => i.Id == x.OperatorId));

            if (stage!.MaximumSelection == -1)
            {
                return true;
            }

            if (currentOperatorsOfStage?.Count() < stage.MaximumSelection || currentOperatorsOfStage is null)
            {
                return true;
            }

            return false;
        }

        protected bool IsStageValid(WorkflowInitializeStageModel stage)
        {
            var currentOperatorsOfStage = WorkflowSubmitRequest.Operators?
                .Where(x => stage.Operators!.Any(i => i.Id == x.OperatorId));

            if (currentOperatorsOfStage is null)
            {
                return stage.MinimumSelection < 1;
            }

            if (currentOperatorsOfStage?.Count() < stage.MinimumSelection)
            {
                return false;
            }

            return true;
        }

        protected bool AreStagesValid()
        {
            if (WorkflowInitializeModel is null)
            {
                return false;
            }

            foreach (var stage in WorkflowInitializeModel.Stages!)
            {
                if (!IsStageValid(stage))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
