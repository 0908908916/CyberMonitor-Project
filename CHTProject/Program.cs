using Microsoft.EntityFrameworkCore;
using CHTProject.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. 註冊服務
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 解決跨域問題 (CORS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// 抓取連線字串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiDbContext>(o => o.UseNpgsql(connectionString));

var app = builder.Build();

// 2. 自動建表與重試邏輯 (核心修正)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    for (int i = 0; i < 15; i++)
    {
        try
        {
            var context = services.GetRequiredService<ApiDbContext>();
            context.Database.EnsureCreated();
            Console.WriteLine("✅ [Success] 資料庫連線成功，監控表已就緒！");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ [Wait] 第 {i + 1} 次嘗試連線資料庫中...");
            Thread.Sleep(5000); // 等待 5 秒後再試
        }
    }
}

// 3. 中間件配置
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();
app.MapControllers();
app.Run();