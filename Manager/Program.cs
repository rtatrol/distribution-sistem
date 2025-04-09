
using Manager.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("WorkerOptions"));
builder.Services.AddTransient<ITimeoutService, TimeoutService>();
builder.Services.AddSingleton<IWorkerTaskService, WorkerTaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
