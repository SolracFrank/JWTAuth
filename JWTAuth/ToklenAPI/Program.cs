using Application;
using Infrastructure;
using Microsoft.OpenApi.Models;
using ToklenAPI.Filters;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;


// Add services to the container.

//add infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

//add application
builder.Services.AddApplication();


//Add controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

//Add Identity


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//Swagger
builder.Services.AddSwaggerGen(options =>
{
    var securitySchema = new OpenApiSecurityScheme
    {
        Description =
        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securitySchema);

    var securityRequirement = new OpenApiSecurityRequirement
        {
             { securitySchema, new[] { "Bearer" } }
        };
    options.AddSecurityRequirement(securityRequirement);

    var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

    // integrate xml comments
    //options.IncludeXmlComments(filePath);
});

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

if (isDevelopment)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
