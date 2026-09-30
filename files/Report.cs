using System;

namespace JobApp.Models
{
    // REQ-011: represents a candidate's report about a job posting.
    public class Report
    {
        private int id;
        private User reporter;
        private Job job;
        private string reason;
        private string reportDate;
        private string status; // NEW / HANDLED

        // A new report always gets NEW status and the current date/time (matches Application's pattern).
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

        // Changed only after handling in JobSystem (REQ-007, KAN-11).
        public void SetStatus(string status) => this.status = status;

        public override string ToString()
        {
            string jobTitle = job != null ? job.GetTitle() : "";
            return $"Report #{id} | Job: {jobTitle} | {reportDate} | {status} | Reason: {reason}";
        }
    }
}
