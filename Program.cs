using ComputerVisionAPI.Network;
using ComputerVisionAPI.Services.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();
builder.Services.AddHttpClient();
builder.Services.Configure<ComputerVisionSettings>(builder.Configuration.GetSection("ComputerVision"));
    
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options =>
{
    options.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
});

app.UseHttpsRedirection();
app.MapControllers();
app.Urls.Add("http://0.0.0.0:5000");
app.Run();
