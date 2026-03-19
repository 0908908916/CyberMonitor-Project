namespace CHTProject.Models;

public class LogItem
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty; // 確保有這一行
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}