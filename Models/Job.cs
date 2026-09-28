namespace JobApp.Models
{
    // REQ-005: represents a job posting linked to the company and employer that published it.
    // Full publish/edit/close flow (REQ-005, KAN-9) belongs to a teammate's story; only the
    // fields and read-only behavior needed for KAN-8 (submitApplication) are implemented here.
    public class Job
    {
        private int id;
        private string title;
        private string description;
        private string field;
        private string location;
        private string jobType;
        private string status; // OPEN / CLOSED / REMOVED
        private Company company;
        private User employer;

        // A new job is always created as OPEN (design doc, section 5.4).
        public Job(int id, string title, string description, string field, string location,
                   string jobType, Company company, User employer)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.field = field;
            this.location = location;
            this.jobType = jobType;
            this.status = "OPEN";
            this.company = company;
            this.employer = employer;
        }

        public int GetId() => id;
        public string GetTitle() => title;
        public string GetDescription() => description;
        public string GetField() => field;
        public string GetLocation() => location;
        public string GetJobType() => jobType;
        public Company GetCompany() => company;
        public User GetEmployer() => employer;

        public string GetStatus() => status;
        public void SetStatus(string status) => this.status = status;

        public bool IsOpen() => status == "OPEN";

        public override string ToString()
        {
            string companyName = company != null ? company.GetName() : "";
            return $"Job #{id} | {title} | {companyName} | {field} | {location} | {jobType} | {status}";
        }
    }
}
