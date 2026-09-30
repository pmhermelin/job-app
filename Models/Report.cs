using System;

namespace JobApp.Models
{
    // REQ-011: represents a candidate's report of an existing job, with the reason they gave.
    public class Report
    {
        private int id;
        private User reporter;
        private Job job;
        private string reason;
        private string reportDate;
        private string status; // NEW / HANDLED (design doc section 5.6) - handling a report is REQ-007 / KAN-11

        // A new report always gets NEW status and the current date/time (mirrors Application - design doc 5.5).
        public Report(int id, User reporter, Job job, string reason)
        {
            this.id = id;
            this.reporter = reporter;
            this.job = job;
            this.reason = reason;
            this.reportDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            this.status = "NEW";
        }

        public int GetId() => id;
        public User GetReporter() => reporter;
        public Job GetJob() => job;
        public string GetReason() => reason;
        public string GetReportDate() => reportDate;
        public string GetStatus() => status;

        // Changed only after validation in JobSystem (REQ-007 / KAN-11 - a teammate's story).
        public void SetStatus(string status) => this.status = status;

        public override string ToString()
        {
            string jobTitle = job != null ? job.GetTitle() : "";
            return $"Report #{id} | {jobTitle} | {reportDate} | {status} | Reason: {reason}";
        }
    }
}
