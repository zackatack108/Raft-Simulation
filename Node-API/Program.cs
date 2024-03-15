using Node_API;
using Node_API.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddSingleton(builder.Logging);

builder.Services.AddSingleton<Node>();
builder.Services.AddScoped<LogHandler>();
builder.Services.AddScoped<ElectionHandler>();
builder.Services.AddSingleton<RaftHandler>();

var app = builder.Build();

var node = app.Services.GetRequiredService<Node>();

var raftHandler = app.Services.GetRequiredService<RaftHandler>();
raftHandler.Initialize();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(policy =>
    policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );

app.UseAuthorization();

app.MapControllers();

app.Run();
