using BancoKRT.Application.Services;
using BancoKRT.Api.ExceptionHandling;
using BancoKRT.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Erro de validação",
            Detail = "A requisição contém dados inválidos.",
            Status = StatusCodes.Status400BadRequest
        };

        return new BadRequestObjectResult(problemDetails);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"];

        return controllerName switch
        {
            "PixLimitAccounts" => ["Gestao de Limites"],
            "PixTransactions" => ["Transacoes"],
            _ => [controllerName ?? "API"]
        };
    });
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IPixLimitAccountService, PixLimitAccountService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
