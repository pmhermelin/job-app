using System;
using JobApp.Models;

namespace JobApp
{
    // Central system class (design doc, section 5.8).
    // Holds all data arrays and implements the business logic and permissions.
    // Team note: only KAN-5 (registerUser) is implemented so far. The other stories (KAN-6, KAN-13,
    // KAN-8, KAN-17, plus Zohar's and Avraham's jobs/applications/reports stories) will add fields and
    // methods to this same class on other branches - expect merge conflicts, coordinate with the team.
    public class JobSystem
    {
        private const int MaxUsers = 100;
        private const int MaxCompanies = 100;
        private const int MaxJobs = 200;
        private const int MaxApplications = 500;

        private User[] users;
        private int userCount;

        private Company[] companies;
        private int companyCount;

        private Job[] jobs;
        private int jobCount;

        private Application[] applications;
        private int applicationCount;

        private int nextUserId;
        private int nextCompanyId;
        private int nextJobId;
        private int nextApplicationId;

        public JobSystem()
        {
            users = new User[MaxUsers];
            userCount = 0;

            companies = new Company[MaxCompanies];
            companyCount = 0;

            jobs = new Job[MaxJobs];
            jobCount = 0;

            applications = new Application[MaxApplications];
            applicationCount = 0;

            nextUserId = 1;
            nextCompanyId = 1;
            nextJobId = 1;
            nextApplicationId = 1;

            SeedAdmin();
            SeedDemoJobs();
        }

        // Publishing jobs (REQ-005, KAN-9) is a teammate's story and not implemented yet.
        // A couple of OPEN demo jobs are seeded here only so KAN-8 (submitApplication) has
        // something to apply to and can actually be tested manually; remove/replace once
        // publishJob() exists.
        private void SeedDemoJobs()
        {
            Company demoCompany = new Company(nextCompanyId, "Demo Company", "Seeded for testing KAN-8.", null);
            companies[companyCount] = demoCompany;
            companyCount++;
            nextCompanyId++;

            Job job1 = new Job(nextJobId, "Backend Developer", "Work on the server side.", "Software", "Tel Aviv", "Full-time", demoCompany, null);
            jobs[jobCount] = job1;
            jobCount++;
            nextJobId++;

            Job job2 = new Job(nextJobId, "QA Intern", "Manual and automated testing.", "QA", "Haifa", "Internship", demoCompany, null);
            jobs[jobCount] = job2;
            jobCount++;
            nextJobId++;
        }

        // The admin account is created in the constructor; ADMIN cannot be chosen at registration (design doc, section 4.4).
        private void SeedAdmin()
        {
            User admin = new User(nextUserId, "System Admin", "admin@jobapp.local", "Admin#2026", "ADMIN");
            users[userCount] = admin;
            userCount++;
            nextUserId++;
        }

        // ---------- REQ-001 / KAN-5: registerUser ----------
        // Logic per design doc section 6.1.
        // errorMessage is returned via out so Program can show a clear message without throwing exceptions.
        public User RegisterUser(string name, string email, string password, string userType,
                                  string companyName, string companyDescription,
                                  out string errorMessage)
        {
            errorMessage = "";

            if (IsBlank(name))
            {
                errorMessage = "Name cannot be empty.";
                return null;
            }

            if (!IsValidEmail(email))
            {
                errorMessage = "Email address is not valid.";
                return null;
            }

            if (!IsEmailUnique(email))
            {
                errorMessage = "This email address is already registered.";
                return null;
            }

            if (!IsStrongPassword(password))
            {
                errorMessage = "Password must be at least 8 characters, with at least one letter and one digit.";
                return null;
            }

            if (userType != "CANDIDATE" && userType != "EMPLOYER")
            {
                errorMessage = "You cannot register as a system administrator.";
                return null;
            }

            if (userCount >= MaxUsers)
            {
                errorMessage = "The system is full, no more users can be added.";
                return null;
            }

            if (userType == "EMPLOYER")
            {
                if (IsBlank(companyName))
                {
                    errorMessage = "Company name cannot be empty.";
                    return null;
                }
                if (companyCount >= MaxCompanies)
                {
                    errorMessage = "The system is full, no more companies can be added.";
                    return null;
                }
            }

            // All checks passed - create everything in one shot, no partial changes on failure.
            User newUser = new User(nextUserId, name, email, password, userType);

            if (userType == "CANDIDATE")
            {
                CandidateProfile profile = new CandidateProfile(newUser);
                newUser.SetCandidateProfile(profile);
            }
            else // EMPLOYER
            {
                Company company = new Company(nextCompanyId, companyName, companyDescription, newUser);
                companies[companyCount] = company;
                companyCount++;
                nextCompanyId++;
            }

            users[userCount] = newUser;
            userCount++;
            nextUserId++;

            return newUser;
        }

        // ---------- Helper / validation methods (design doc, section 7) ----------

        public bool IsBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        // Checks for '@' followed later by a '.', and that the email isn't blank.
        public bool IsValidEmail(string email)
        {
            if (IsBlank(email))
            {
                return false;
            }

            int at = email.IndexOf('@');
            if (at <= 0)
            {
                return false;
            }

            int dot = email.IndexOf('.', at);
            return dot > at + 1 && dot < email.Length - 1;
        }

        public bool IsEmailUnique(string email)
        {
            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetEmail().Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        // At least 8 characters, at least one letter and one digit.
        public bool IsStrongPassword(string password)
        {
            if (IsBlank(password) || password.Length < 8)
            {
                return false;
            }

            bool hasLetter = false;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (char.IsLetter(c))
                {
                    hasLetter = true;
                }
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
            }

            return hasLetter && hasDigit;
        }

        // Helper for tests (KAN-20): verify userCount is unchanged after a failed registration.
        public int GetUserCount() => userCount;

        // ---------- REQ-002 / KAN-6: login ----------
        // Logic per design doc section 6.2.
        // errorMessage is returned via out so Program can show a clear message without throwing exceptions.
        public User Login(string email, string password, out string errorMessage)
        {
            errorMessage = "";

            if (IsBlank(email) || IsBlank(password))
            {
                errorMessage = "Email and password are required.";
                return null;
            }

            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetEmail().Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    if (!users[i].CheckPassword(password))
                    {
                        // Same generic message as "not found" - do not reveal which part was wrong.
                        errorMessage = "Invalid email or password.";
                        return null;
                    }

                    if (users[i].IsSuspended())
                    {
                        errorMessage = "This account has been suspended. Contact the system administrator.";
                        return null;
                    }

                    return users[i];
                }
            }

            errorMessage = "Invalid email or password.";
            return null;
        }

        // ---------- REQ-009 / KAN-13: updateCandidateProfile ----------
        // Logic per design doc section 6.11.
        // errorMessage is returned via out so Program can show a clear message without throwing exceptions.
        public bool UpdateCandidateProfile(User candidate, string education, string experience,
                                            string skills, string summary, out string errorMessage)
        {
            errorMessage = "";

            // Permission check (design doc 6.11 + section 4.3 permissions table):
            // a candidate updates only the profile linked to their own account.
            if (candidate == null || candidate.GetUserType() != "CANDIDATE")
            {
                errorMessage = "Only job seekers can update a candidate profile.";
                return false;
            }

            if (IsBlank(education))
            {
                errorMessage = "Education cannot be empty.";
                return false;
            }

            if (IsBlank(experience))
            {
                errorMessage = "Experience cannot be empty.";
                return false;
            }

            if (IsBlank(skills))
            {
                errorMessage = "Skills cannot be empty.";
                return false;
            }

            if (IsBlank(summary))
            {
                errorMessage = "Professional summary cannot be empty.";
                return false;
            }

            CandidateProfile profile = candidate.GetCandidateProfile();
            if (profile == null)
            {
                errorMessage = "No candidate profile found for this account.";
                return false;
            }

            // All checks passed - update only after every value is valid (design doc 6.11, step 4).
            profile.UpdateProfile(education, experience, skills, summary);
            return true;
        }

        // ---------- REQ-004 / KAN-8: submitApplication ----------
        // Logic per design doc section 6.4.

        public Job FindJobById(int jobId)
        {
            for (int i = 0; i < jobCount; i++)
            {
                if (jobs[i].GetId() == jobId)
                {
                    return jobs[i];
                }
            }
            return null;
        }

        // Helper for Program: only OPEN jobs are offered for application.
        public Job[] GetOpenJobs(out int count)
        {
            Job[] open = new Job[jobCount];
            count = 0;
            for (int i = 0; i < jobCount; i++)
            {
                if (jobs[i].IsOpen())
                {
                    open[count] = jobs[i];
                    count++;
                }
            }
            return open;
        }

        // errorMessage is returned via out so Program can show a clear message without throwing exceptions.
        public bool SubmitApplication(User candidate, int jobId, out string errorMessage)
        {
            errorMessage = "";

            // Step 1: candidate must be CANDIDATE type and not suspended.
            if (candidate == null || candidate.GetUserType() != "CANDIDATE")
            {
                errorMessage = "Only job seekers can submit applications.";
                return false;
            }

            if (candidate.IsSuspended())
            {
                errorMessage = "This account has been suspended. Contact the system administrator.";
                return false;
            }

            // Step 2-3: job must exist and be OPEN.
            Job job = FindJobById(jobId);
            if (job == null || !job.IsOpen())
            {
                errorMessage = "Job not found or is not open for applications.";
                return false;
            }

            // Step 4: profile must exist and be complete.
            CandidateProfile profile = candidate.GetCandidateProfile();
            if (profile == null || !profile.IsComplete())
            {
                errorMessage = "Please complete your profile (education, experience, skills, summary) before applying.";
                return false;
            }

            // Step 5: no duplicate application by the same candidate to the same job.
            for (int i = 0; i < applicationCount; i++)
            {
                if (applications[i].GetCandidate() == candidate && applications[i].GetJob() == job)
                {
                    errorMessage = "You have already applied to this job.";
                    return false;
                }
            }

            // Step 6: capacity check.
            if (applicationCount >= MaxApplications)
            {
                errorMessage = "The system is full, no more applications can be added.";
                return false;
            }

            // Step 7: all checks passed - create the application (NEW status, set in the constructor).
            Application application = new Application(nextApplicationId, candidate, job);
            applications[applicationCount] = application;
            applicationCount++;
            nextApplicationId++;

            return true;
        }
    }
}
