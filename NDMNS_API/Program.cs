using NDMNS_API.AppHubs;
using NDMNS_API.Repositories;
using NDMNS_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<SystemHealthCheckService>();
builder.Services.AddHttpClient<WhatsAppService>();
builder.Services.AddHttpClient<HelperService>(
    (sp, client) =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var baseUrl = configuration["WhatsAppService:BaseUrl"] ?? "http://localhost:3000";
        client.BaseAddress = new Uri(baseUrl);
    }
);

builder.Services.AddScoped<HelpdeskRepository>();
builder.Services.AddScoped<EmailRepository>();
builder.Services.AddScoped<ShiftRepository>();
builder.Services.AddScoped<SiteRepository>();
builder.Services.AddScoped<PicRepository>();
builder.Services.AddScoped<DowntimeRepository>();
builder.Services.AddScoped<IspRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<NetworkRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SettingRepository>();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddScoped<SystemHealthCheckService>();

builder.Services.AddSingleton<PingService>();

builder.Services.AddHostedService(provider => provider.GetRequiredService<PingService>());
builder.Services.AddHostedService<SystemHealthService>();
builder.Services.AddHttpClient<SystemHealthService>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapHub<MonitoringHub>("/hubs/monitoring").RequireCors("AllowAll");
app.MapHub<WhatsAppHub>("/hubs/whatsapp").RequireCors("AllowAll");

app.Run();
