namespace IssueHunter.Configuration;

public class IssuePollingOptions
{
    public const string SectionName = "IssuePolling";

    public bool Enabled { get; set; } = false;
    public bool RunOnStartup { get; set; } = false;
    public int IntervalMinutes { get; set; } = 15;
}