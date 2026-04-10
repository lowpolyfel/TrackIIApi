namespace Trackii.Api.Models;

public sealed class ScrapItem
{
    public uint Id { get; set; }
    public uint WipStepExecutionId { get; set; }
    public uint TotalQtyScrapped { get; set; }
    public ScrapDisposition Disposition { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual WipStepExecution? WipStepExecution { get; set; }
    public virtual ICollection<ScrapLog> ScrapLogs { get; set; } = new List<ScrapLog>();
}
