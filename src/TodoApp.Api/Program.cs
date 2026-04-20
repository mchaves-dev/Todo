using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Infra.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>((provider, opt) =>
{
    var interceptor = provider.GetRequiredService<AuditableInterceptor>();

    opt.UseInMemoryDatabase("mem");
    opt.AddInterceptors(interceptor);
});

builder.Services.AddSingleton<AuditableInterceptor>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "API para cadastro, consulta, conclusao, arquivamento e copia de itens de tarefa."
    });
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
    });
}

app.UseHttpsRedirection();
app.MapEndpoints();

await app
    .RunAsync()
    .ConfigureAwait(false);
