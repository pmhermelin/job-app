namespace JobApp.Models
{
    // REQ-001 / REQ-005: represents a single company managed by an employer.
    public class Company
    {
        private int id;
        private string name;
        private string description;
        private User employer;

        public Company(int id, string name, string description, User employer)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.employer = employer;
        }

        public int GetId() => id;
        public string GetName() => name;
        public string GetDescription() => description;
        public User GetEmployer() => employer;

        public void SetName(string name) => this.name = name;
        public void SetDescription(string description) => this.description = description;

        public override string ToString()
        {
            return $"Company #{id} | {name} | {description}";
        }
    }
}
