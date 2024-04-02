using Autofac.Extensions.DependencyInjection;
using BlazorDatasource.Api;
using BlazorDatasource.Shared;
using BlazorDatasource.Shared.Infrastructure;
using BlazorDatasource.Shared.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("DOTNET_SYSTEM_NET_DISABLEIPV6",
    builder.Configuration["UseIpv6"] == "yes" ? "0" : "1");

// Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

IdentityModelEventSource.ShowPII = true;

// Serilog
// with output template
// outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.Enrich.FromLogContext()
        .WriteTo.File("Logs/Errors.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose, shared: true)
        .WriteTo.Console();
});

// Context
builder.Services.AddDbContextPool<IDbContext, EntityObjectContext>(optionsBuilder =>
{
    optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction =>
{
    setupAction.EnableAnnotations();
    setupAction.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "2.0",
        Title = "BlazorDatasource.Api",
    });
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    setupAction.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// HttpClient
// Datastore external url
string dataStoreExternalApiUrl = builder.Configuration["DatastoreExternalApiUrl"] ??
    throw new InvalidOperationException("Datastore External Api Url is not configured");

// Issuer
string issuer = builder.Configuration["Issuer"] ??
    throw new InvalidOperationException("Issuer is not configured");

var tokenValidationParameters = new TokenValidationParameters
{
    // Require expiration time
    RequireExpirationTime = true,

    // Issuer validation
    ValidateIssuer = true,
    ValidIssuer = issuer,

    // Audience validation
    ValidateAudience = true,
    ValidAudience = "account",

    // Lifetime validation
    ValidateLifetime = true,

    // Allow for some drift in server time
    // (a lower value is better; we recommend two minutes or less)
    ClockSkew = TimeSpan.FromMinutes(2),

    // Valid types of access token
    ValidTypes = new[] { "JWT" }
};

// Httpclient to call the external datasource api
builder.Services.AddHttpClient(Options.DefaultName, configureClient =>
{
    configureClient.BaseAddress = new(dataStoreExternalApiUrl);
    configureClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(System.Net.Mime.MediaTypeNames.Application.Json));
});

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Enable for production
    options.RequireHttpsMetadata = false;

    // Authority
    options.Authority = issuer;

    // Do not map claims
    options.MapInboundClaims = false;

    // Token validation parameters
    options.TokenValidationParameters = tokenValidationParameters;

    // Store the access token
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            string message = string.Empty;
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            message += FlattenException(context.Exception);
            context.Fail(message);
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction =>
    {
        setupAction.SwaggerEndpoint("/swagger/v2/swagger.json", "BlazorDatasource.Api v2");
        setupAction.DefaultModelsExpandDepth(-1);
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Authorize endpoint (sign in)
app.MapPost(pattern: Constants.ApiRoutePaths.Authorize, handler: async (IHttpClientFactory httpClientFactory,
                                                                        [FromBody] SignInRequest request) =>
{
    using HttpClient client = httpClientFactory.CreateClient();
    Log.Information("Request: {0}", JsonSerializer.Serialize(request));
    var response = await client.PostAsJsonAsync("/api/users/signin", request);
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<SignInResponse>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

// Register endpoint (sign up)
app.MapPost(Constants.ApiRoutePaths.Register, handler: async (IHttpClientFactory httpClientFactory,
                                                              [FromBody] SignUpRequest request) =>
{
    using HttpClient client = httpClientFactory.CreateClient();
    Log.Information("Request: {0}", JsonSerializer.Serialize(request));
    var response = await client.PostAsJsonAsync("/api/users/signup", request);
    return response;
});

// Datasets for specific user get_all_dataset(username)
app.MapGet(pattern: Constants.ApiRoutePaths.AllDatasets, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext) =>
{

    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/description");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<DatasetResponse>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Datasets search with term
app.MapPost(pattern: Constants.ApiRoutePaths.SearchDataset, handler: async (IHttpClientFactory httpClientFactory,
                                                                            HttpContext httpContext,
                                                                            [FromBody] SearchRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/online/search?q={request.Keyword}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<SearchResultDataset>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Datasets get the search filters for a specific dataset from Constants.ApiRoutePaths.SearchDataset
app.MapPost(pattern: Constants.ApiRoutePaths.GetDatasetFilters, handler: async (IHttpClientFactory httpClientFactory,
                                                                                HttpContext httpContext,
                                                                                [FromBody] SearchFiltersRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/online/search/{request.SourceId}/{request.DatasetId}/filters");
    if (response.IsSuccessStatusCode)
    {
        // var result = await response.Content.ReadFromJsonAsync<List<SearchFiltersResultFilter>>();
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Datasets download a dataset using the selected filters from Constants.ApiRoutePaths.GetDatasetFilters
app.MapPost(pattern: Constants.ApiRoutePaths.DownloadDataset, handler: async (IHttpClientFactory httpClientFactory,
                                                                              HttpContext httpContext,
                                                                              [FromBody] DownloadDatasetRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    Log.Information("Request: {0}", JsonSerializer.Serialize(request));
    var response = await client.PostAsJsonAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/download/{request.SourceId}/{request.DatasetId}",
        request.FilterRequest);
    if (response.IsSuccessStatusCode)
    {
        var downloadedDataset = await response.Content.ReadAsStringAsync();
        return Results.Ok(new DownloadDatasetResponse()
        {
            DatasetUUId = downloadedDataset
        });
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Datasets view a specific dataset by uuid
app.MapPost(pattern: Constants.ApiRoutePaths.ViewDataset, handler: async (IHttpClientFactory httpClientFactory,
                                                                          HttpContext httpContext,
                                                                          [FromBody] ViewDatasetRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/fetch/by-id?id={request.DatasetUUId}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

// Datasets view all datasets by uuid
app.MapPost(pattern: Constants.ApiRoutePaths.AllDatasetsForSpecificFamily, handler: async (IHttpClientFactory httpClientFactory,
                                                                                           HttpContext httpContext,
                                                                                           [FromBody] ViewDatasetsByFamilyRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datastore/{httpContext.User.GetDisplayName()}/select?filters=dataset_id:{request.DatasetFamilyId}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<ResultTimeline>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

// Get join dataset available types
app.MapGet(pattern: Constants.ApiRoutePaths.JoinDatasetTypes, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext) =>
{

    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/join/types");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<string>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Get join dataset available datatypes
app.MapGet(pattern: Constants.ApiRoutePaths.JoinDatasetDatatypes, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext) =>
{

    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/join/datatypes");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<string>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Dataset join
app.MapPost(pattern: Constants.ApiRoutePaths.JoinDataset, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] JoinRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    Log.Information("Request: {0}", JsonSerializer.Serialize(request));
    var response = await client.PostAsJsonAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/join",
        request);
    if (response.IsSuccessStatusCode)
    {
        var downloadedDataset = await response.Content.ReadAsStringAsync();
        return Results.Ok(new DownloadDatasetResponse()
        {
            DatasetUUId = downloadedDataset
        });
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

// Get status
app.MapPost(pattern: Constants.ApiRoutePaths.DatasetStatus, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] ViewDatasetRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/status/by-id?id={request.DatasetUUId}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        return Results.Ok(JsonSerializer.Serialize(result));
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.DatasetById, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] string request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/description/by-id?id={request}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<DatasetResponse>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.DatasetByAlias, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] string request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/description/by-alias?alias={request}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<DatasetResponse>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.DatasetSourceInfoById, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] string request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/datasets/{httpContext.User.GetDisplayName()}/sourceinfo?id={request}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<DatasetSourceInfoResponse>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapGet(pattern: Constants.ApiRoutePaths.WorkflowTypes, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext) =>
{

    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.GetAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/types");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<WorkflowTypeResponse>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

app.MapPost(pattern: Constants.ApiRoutePaths.WorkflowDatasets, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] string request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/types/{request}/datasets");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<WorkflowDatasetResponse>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.WorkflowInitialize, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] WorkflowInitializeRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.PostAsJsonAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/types/{request.WorkflowTypeId}/initialize", request);
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<WorkflowInitializeResponse>();
        Log.Information("Response: {0}", JsonSerializer.Serialize(result));
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.WorkflowSubmit, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] WorkflowSubmitRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.PostAsJsonAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/submit", request);
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        Log.Information("Response: {0}", JsonSerializer.Serialize(result));
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.WorkflowStatus, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] ViewWorkflowRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/status?id={request.WorkflowUuid}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        return Results.Ok(JsonSerializer.Serialize(result));
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.WorkflowProgress, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] ViewWorkflowRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/progress?id={request.WorkflowUuid}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        return Results.Ok(JsonSerializer.Serialize(result));
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapPost(pattern: Constants.ApiRoutePaths.ViewWorkflow, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext,
    [FromBody] ViewWorkflowRequest request) =>
{
    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var response = await client.GetAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/results/report?id={request.WorkflowUuid}");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<WorkflowResponse>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
});

app.MapGet(pattern: Constants.ApiRoutePaths.AllWorkflows, handler: async (IHttpClientFactory httpClientFactory,
    HttpContext httpContext) =>
{

    var accessToken = await httpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, Constants.TokenTypes.AccessToken);
    using HttpClient client = httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.GetAsync($"/api/workflows/{httpContext.User.GetDisplayName()}/all");
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<List<WorkflowSubmittedResponse>>();
        return Results.Ok(result);
    }
    return Results.StatusCode((int)response.StatusCode);
}).RequireAuthorization();

app.Run();

static string FlattenException(Exception? exception)
{
    var stringBuilder = new StringBuilder();

    while (exception != null)
    {
        stringBuilder.AppendLine(exception.Message);
        stringBuilder.AppendLine(exception.StackTrace);

        exception = exception.InnerException;
    }
    return stringBuilder.ToString();
}