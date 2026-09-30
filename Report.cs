using System;

/// <summary>
/// Represents a report filed by a user about a suspicious or invalid job posting.
/// Design doc reference: section 5.6.
/// </summary>
public class Report
{
    private int id;
    private User reporter;
    private Job job;
    private string reason;
    private string reportDate;
    private string status; // NEW / HANDLED

    public Report(int id, User reporter, Job job, string reason)
    {
        this.id = id;
        this.reporter = reporter;
        this.job = job;
        this.reason = reason;
        this.reportDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        this.status = "NEW";
    }

    public int GetId() => id;
    public User GetReporter() => reporter;
    public Job GetJob() => job;
    public string GetReason() => reason;
    public string GetReportDate() => reportDate;
    public string GetStatus() => status;

    /// <summary>
    /// Marks this report as handled by an admin. Called only from JobSystem.
    /// </summary>
    public void MarkHandled()
    {
        status = "HANDLED";
    }

    public override string ToString()
    {
        return $"Report #{id} | Status: {status}\n" +
               $"Reporter: {reporter.GetName()}\n" +
               $"Job: #{job.GetId()} - {job.GetTitle()}\n" +
               $"Reason: {reason}\n" +
               $"Date: {reportDate}";
    }
}
