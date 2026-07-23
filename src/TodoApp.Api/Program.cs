using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TodoApp.Api.Aplication.Auth;
using TodoApp.Api.Aplication.Endpoints;
using TodoApp.Api.Features.Todo.Realtime;
using TodoApp.Api.Infra.Database;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>((provider, opt) =>
{
    var interceptor = provider.GetRequiredService<AuditableInterceptor>();

    opt.UseInMemoryDatabase("mem");
    opt.AddInterceptors(interceptor);
});

builder.Services.AddSingleton<AuditableInterceptor>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.Configure<AuthTokenOptions>(builder.Configuration.GetSection(AuthTokenOptions.SectionName));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITodoRealtimeNotifier, TodoRealtimeNotifier>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasmDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5025", "https://localhost:7027")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(AuthConstants.Scheme)
    .AddScheme<AuthenticationSchemeOptions, AccessTokenAuthenticationHandler>(AuthConstants.Scheme, _ => { });
builder.Services.AddAuthorization();

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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Cole apenas o JWT. O Swagger envia como Authorization: Bearer {token}."
    });
    options.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(new OpenApiSecuritySchemeReference("Bearer", document, null), []);

        return requirement;
    });
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
app.UseCors("BlazorWasmDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<TodoHub>("/hubs/todos");
app.MapEndpoints();

await app
    .RunAsync()
    .ConfigureAwait(false);
