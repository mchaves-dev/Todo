using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Database;
using TodoApp.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>((provider, opt) =>
{
    var interceptor = provider.GetRequiredService<AuditableInterceptor>();

    opt.UseInMemoryDatabase("mem");
    opt.AddInterceptors(interceptor);
});

builder.Services.AddSingleton<AuditableInterceptor>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapEndpoints();

await app
    .RunAsync()
    .ConfigureAwait(false);
