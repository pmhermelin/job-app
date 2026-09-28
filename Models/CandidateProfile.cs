namespace JobApp.Models
{
    // REQ-009: represents a job seeker's professional profile and text resume.
    public class CandidateProfile
    {
        private User candidate;
        private string education;
        private string experience;
        private string skills;
        private string summary;

        public CandidateProfile(User candidate)
        {
            this.candidate = candidate;
            this.education = "";
            this.experience = "";
            this.skills = "";
            this.summary = "";
        }

        // Fully used in KAN-13 (updateCandidateProfile); here an empty profile is created at registration.
        public void UpdateProfile(string education, string experience, string skills, string summary)
        {
            this.education = education;
            this.experience = experience;
            this.skills = skills;
            this.summary = summary;
        }

        // Returns true when all required fields are non-empty (needed to submit an application - KAN-8).
        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(education)
                && !string.IsNullOrWhiteSpace(experience)
                && !string.IsNullOrWhiteSpace(skills)
                && !string.IsNullOrWhiteSpace(summary);
        }

        public User GetCandidate() => candidate;
        public string GetEducation() => education;
        public string GetExperience() => experience;
        public string GetSkills() => skills;
        public string GetSummary() => summary;

        public override string ToString()
        {
            return $"Education: {education} | Experience: {experience} | Skills: {skills} | Summary: {summary}";
        }
    }
}
