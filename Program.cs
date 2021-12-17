using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using ODataIssue406;
using ODataIssue406.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var batchHandler = new DefaultODataBatchHandler();
batchHandler.MessageQuotas.MaxNestingDepth = 3;
batchHandler.MessageQuotas.MaxOperationsPerChangeset = 10;

builder.Services
  .AddControllers()
  .AddJsonOptions(opts =>
  {
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  })
  .AddOData(opts =>
  {
    opts.AddRouteComponents("v1", GetEdmModel(), batchHandler)
      .EnableQueryFeatures();
    opts.RouteOptions.EnableControllerNameCaseInsensitive = true;
    opts.RouteOptions.EnableKeyAsSegment = true;
    opts.RouteOptions.EnableKeyInParenthesis = false;
    opts.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
    opts.RouteOptions.EnableUnqualifiedOperationCall = true;
    opts.UrlKeyDelimiter = Microsoft.OData.ODataUrlKeyDelimiter.Slash;
    opts.QuerySettings.EnableSkipToken = false;
    opts.QuerySettings.MaxTop = 100;
    opts.QuerySettings.EnableExpand = false;
  });
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MyAppContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Version = "v1",
    Title = "API"
  });

  c.CustomOperationIds(apiDesc =>
  {
    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
  });

  c.UseAllOfToExtendReferenceSchemas();

  c.ResolveConflictingActions(a => a.First());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseODataRouteDebug();
}
else
{
  app.UseExceptionHandler(appBuilder =>
  {
    appBuilder.Run(async context =>
    {
      // ensure generic 500 status code on fault.
      context.Response.StatusCode = StatusCodes.Status500InternalServerError; ;
      await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
    });
  });
}

app.UseSwagger(c =>
{
  c.RouteTemplate = "openapi/{documentName}.json";
  c.PreSerializeFilters.Add((swagger, httpReq) =>
  {
    swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
  });
});
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/openapi/v1.json", "API");
  c.RoutePrefix = "swagger";
  c.DocumentTitle = "API";
  c.DisplayOperationId();
  c.DisplayRequestDuration();
  c.EnableFilter();
  c.ShowExtensions();
  c.ShowCommonExtensions();
  c.EnableValidator();

});

app.UseHttpsRedirection();

app.UseODataBatching();

app.UseRouting();

app.MapControllers();

app.Run();


IEdmModel GetEdmModel()
{
  var builder = new ODataConventionModelBuilder();
  builder.EnableLowerCamelCase();
  builder.Namespace = "API";
  builder.ContainerName = "API";

  builder.EntitySet<Customer>("customers");
  builder.EntitySet<Order>("orders");

  return builder.GetEdmModel();
}
