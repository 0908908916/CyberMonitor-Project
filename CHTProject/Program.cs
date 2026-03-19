using Microsoft.EntityFrameworkCore;
using CHTProject.Data; // 這是你存放 ApiDbContext 的地方
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 重要 檢查點 必須有這行 套到 Controllers 資料夾 ---
builder.Services.AddControllers();

// 註冊資料庫 
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 解決跨域問題 (CORS)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// 重要 檢查點 DB 必須有這行
// 1. 取得 appsettings.json 裡的原始連線字串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. 嘗試從環境變數 (例如 Docker 或 .env) 抓取密碼
var dbPass = Environment.GetEnvironmentVariable("DB_PASSWORD");

// 3. 如果有抓到環境變數密碼，就替換掉連線字串中的密碼
if (!string.IsNullOrEmpty(dbPass))
{
    // 這裡利用 Npgsql 的工具來安全地替換密碼，不會弄壞格式
    var connectionBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
    {
        Password = dbPass
    };
    connectionString = connectionBuilder.ConnectionString;
}

// 4. 正式註冊資料庫 (確保 AI 分析能抓到最新狀態)
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(connectionString));


// --- 房子蓋好了，接下來是使用設定 ---
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.UseCors(); // 解決跨域問題 (CORS)
// 重要 檢查點 2 必須有這行 必須有這行 套到 Controllers 資料夾 ---
app.MapControllers();
app.UseAuthorization();  // 解決跨域問題 (CORS)
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
