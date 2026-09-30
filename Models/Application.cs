using System;

namespace JobApp.Models
{
    // REQ-004: represents a single application of a candidate to a job.
    public class Application
    {
        private int id;
        private User candidate;
        private Job job;
        // Design doc section 16 ("פורמט תאריך"): "DateTime פנימי" - kept as an actual DateTime
        // here (not a pre-formatted string), so GetApplicationDate() below can format it to the
        // agreed dd/MM/yyyy HH:mm display format, consistent with Report and AdminAction.
        private DateTime applicationDate;
        private string status; // NEW / UNDER_REVIEW / INTERVIEW / ACCEPTED / REJECTED

        // A new application always gets NEW status and the current date/time (design doc, section 5.5).
        public Application(int id, User candidate, Job job)
        {
            this.id = id;
            this.candidate = candidate;
            this.job = job;
            this.applicationDate = DateTime.Now;
            this.status = "NEW";
        }

        public int GetId() => id;
        public User GetCandidate() => candidate;
        public Job GetJob() => job;

        // Design doc section 16 display format: dd/MM/yyyy HH:mm.
        public string GetApplicationDate() => applicationDate.ToString("dd/MM/yyyy HH:mm");
        public string GetStatus() => status;

        // Changed only after validation in JobSystem (REQ-006, KAN-10 - a teammate's story).
        public void SetStatus(string status) => this.status = status;

        public override string ToString()
        {
            string jobTitle = job != null ? job.GetTitle() : "";
            return $"Application #{id} | {jobTitle} | {GetApplicationDate()} | {status}";
        }
    }
}
