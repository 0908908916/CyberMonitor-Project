using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CHTProject.Data; // 引用 ApiDbContext 的命名空間
using CHTProject.Models; // 引用 LogItem 的命名空間
using System.Net.NetworkInformation; // 引用 Ping 類別的命名空間

namespace CHTProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly ApiDbContext _context;
    public LogController(ApiDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.Logs.ToListAsync());

    // --- 新增：根據 ID 取得單一 Log 項目 ---
    [HttpPost]
    public async Task<IActionResult> Create(LogItem item)
    {
        _context.Logs.Add(item);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    // --- 新增：Ping 設備狀態 ---
    [HttpGet("ping/{id}")]
    public async Task<IActionResult> PingDevice(int id)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null || string.IsNullOrEmpty(log.IP)) return NotFound();

        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(log.IP, 2000); // 逾時設為 2 秒
            bool isAlive = reply.Status == IPStatus.Success;
            return Ok(new { isAlive = isAlive, responseTime = isAlive ? $"{reply.RoundtripTime}ms" : "N/A" });
        }
        catch
        {
            return Ok(new { isAlive = false });
        }
    }

    // --- 新增：更新 Log 項目 ---
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, LogItem item)
    {
        var existing = await _context.Logs.FindAsync(id);
        if (existing == null) return NotFound();

        existing.DeviceName = item.DeviceName;
        existing.Status = item.Status;
        existing.Message = item.Message;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // --- 新增：刪除 Log 項目 ---
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null) return NotFound();
        _context.Logs.Remove(log);
        await _context.SaveChangesAsync();
        return Ok();
    }

    // --- 新增：AI 分析介面 ---
    [HttpGet("analyze/{id}")]
    public async Task<IActionResult> Analyze(int id)
    {
        var log = await _context.Logs.FindAsync(id);
        if (log == null) return NotFound();

        string advice = "";

        // 💡 檢查這裡！必須符合你資料庫存的字串
        if (log.Status.Contains("斷線") || log.Status.Contains("❌") || log.IP == "0.0.0.0")
        {
            advice = $"🚨 [資安警訊] 設備 {log.DeviceName} ({log.IP}) 偵測到斷線！建議檢查實體線路及防火牆規則。";
        }
        else
        {
            advice = $"✅ [健康檢查] 設備 {log.DeviceName} 連線正常，目前無須採取行動。";
        }

        return Ok(new { analysis = advice });
    }
}