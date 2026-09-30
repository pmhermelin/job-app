using System;
using JobApp.Models;

namespace JobApp
{
    // Central system class (design doc, section 5.8).
    // Holds all data arrays and implements the business logic and permissions.
    public class JobSystem
    {
        private const int MaxUsers = 100;
        private const int MaxCompanies = 100;
        private const int MaxJobs = 200;
        private const int MaxApplications = 500;
        private const int MaxReports = 100; // design doc section 3.3
        private const int MaxAdminActions = 300; // design doc section 3.3

        private User[] users;
        private int userCount;

        private Company[] companies;
        private int companyCount;

        private Job[] jobs;
        private int jobCount;

        private Application[] applications;
        private int applicationCount;

        private Report[] reports;
        private int reportCount;
        private AdminAction[] adminActions;
        private int adminActionCount;

        private int nextUserId;
        private int nextCompanyId;
        private int nextJobId;
        private int nextApplicationId;
        private int nextReportId;

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

            reports = new Report[MaxReports];
            reportCount = 0;
            adminActions = new AdminAction[MaxAdminActions];
            adminActionCount = 0;

            nextUserId = 1;
            nextCompanyId = 1;
            nextJobId = 1;
            nextApplicationId = 1;
            nextReportId = 1;

            SeedAdmin();
        }

        // The admin account is created in the constructor; ADMIN cannot be chosen at registration (design doc, section 4.4).
        // KAN-108: known limitation - passwords (including this one) are stored and compared as plain
        // text (see User.CheckPassword). Acceptable for this course project's scope, documented here
        // so it is not mistaken for an oversight.
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

            // KAN-105: trim surrounding whitespace so a stray space typed at registration
            // never causes a mismatch later at login.
            name = name?.Trim();
            email = email?.Trim();

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

            // KAN-107: reject a second '@' (e.g. "a@b@c.com") - only one is valid.
            if (email.LastIndexOf('@') != at)
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

            // KAN-106: consistent with SubmitApplication - a suspended candidate cannot update their profile.
            if (candidate.IsSuspended())
            {
                errorMessage = "This account has been suspended. Contact the system administrator.";
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

            // KAN-116: found in KAN-12 self-review - a suspended employer is already locked out of
            // managing their jobs (IsActiveEmployer blocks PublishJob/EditJob/CloseJob), but before
            // this check their still-OPEN jobs kept accepting new applications. Same "suspended =
            // frozen" principle, applied here too - existing applications are untouched either way.
            if (job.GetEmployer() != null && job.GetEmployer().IsSuspended())
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

        // ---------- REQ-011 / KAN-15: createReport ----------
        // A job seeker reports an existing job with a reason. One report per candidate per job
        // (design doc section 16), capacity is checked, and the report always starts as NEW
        // (handling a report - REQ-007 / KAN-11 - is a separate story/branch).
        // errorMessage is returned via out so Program can show a clear message without throwing exceptions.
        public bool CreateReport(User candidate, int jobId, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (candidate == null || candidate.GetUserType() != "CANDIDATE")
            {
                errorMessage = "Only job seekers can report a job.";
                return false;
            }

            if (candidate.IsSuspended())
            {
                errorMessage = "This account has been suspended. Contact the system administrator.";
                return false;
            }

            // A job that was already administratively removed can no longer be reported (nothing left to act on).
            Job job = FindJobById(jobId);
            if (job == null || job.GetStatus() == "REMOVED")
            {
                errorMessage = "Job not found.";
                return false;
            }

            reason = reason?.Trim();
            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to report a job.";
                return false;
            }

            // KAN-115: design doc section 16 ("דיווח כפול") - one report per candidate per job,
            // period. Mirrors the duplicate-application check in SubmitApplication, but unlike
            // that check this one is permanent (not conditioned on the report's status).
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetReporter() == candidate && reports[i].GetJob() == job)
                {
                    errorMessage = "You have already reported this job.";
                    return false;
                }
            }

            // Capacity + id-overflow guard, consistent with PublishJob's nextJobId check (section 6.5).
            if (reportCount >= MaxReports || nextReportId == int.MaxValue)
            {
                errorMessage = "The system is full, no more reports can be added.";
                return false;
            }

            Report report = new Report(nextReportId, candidate, job, reason);
            reports[reportCount] = report;
            reportCount++;
            nextReportId++;

            return true;
        }

        // Helper for tests: verify reportCount is unchanged after a failed report.
        public int GetReportCount() => reportCount;

        // ---------- REQ-008 / KAN-12: suspendUser / unsuspendUser ----------
        // Logic per design doc section 6.10. Looks a user up by id (numeric) or by email,
        // since the design doc allows either. Every successful suspend/unsuspend is logged
        // as an AdminAction (design doc section 5.7) - this is also what first requires the
        // AdminAction model to exist in the codebase (previously only KAN-11 needed it).

        // Helper for tests and for KAN-11/KAN-18 to inspect the log without exposing the array itself.
        public int GetAdminActionCount() => adminActionCount;

        // ---------- REQ-014 / KAN-18: printAdminLog ----------
        // Design doc section 6.16. ADMIN-only, read-only - there is deliberately no way to
        // delete or edit a logged AdminAction (design doc section 5.7: "בלתי ניתנת לעריכה"),
        // so this is the only access point Program gets to the array's contents.
        public AdminAction[] GetAdminLog(User admin, out int count, out string errorMessage)
        {
            count = 0;
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can view the admin log.";
                return null;
            }

            AdminAction[] results = new AdminAction[adminActionCount];
            for (int i = 0; i < adminActionCount; i++)
            {
                results[i] = adminActions[i];
            }
            count = adminActionCount;
            return results;
        }

        // KAN-12 review (Zohar): Program shows the found user's details/status before the admin
        // confirms suspend/unsuspend, so it needs a permission-checked way to look one up on its
        // own - this is that entry point. FindUserByIdOrEmail itself stays private; only this
        // admin-only, read-only wrapper is exposed (encapsulation, design doc section 3.1).
        public User FindUserForAdmin(User admin, string identifier, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can look up a user.";
                return null;
            }

            User target = FindUserByIdOrEmail(identifier);
            if (target == null)
            {
                errorMessage = "User not found.";
            }
            return target;
        }

        private User FindUserByIdOrEmail(string identifier)
        {
            if (IsBlank(identifier))
            {
                return null;
            }

            identifier = identifier.Trim();

            if (int.TryParse(identifier, out int id))
            {
                for (int i = 0; i < userCount; i++)
                {
                    if (users[i].GetId() == id)
                    {
                        return users[i];
                    }
                }
                return null;
            }

            for (int i = 0; i < userCount; i++)
            {
                if (users[i].GetEmail().Equals(identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return users[i];
                }
            }
            return null;
        }

        private void AddAdminAction(User admin, string actionType, string target, string reason)
        {
            // Capacity is checked by the caller before any state changes, so this never partially fails.
            AdminAction action = new AdminAction(admin, actionType, target, reason);
            adminActions[adminActionCount] = action;
            adminActionCount++;
        }

        public bool SuspendUser(User admin, string userIdentifier, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can suspend a user.";
                return false;
            }

            User target = FindUserByIdOrEmail(userIdentifier);
            if (target == null)
            {
                errorMessage = "User not found.";
                return false;
            }

            // The system administrator account can never be suspended (design doc section 6.10).
            if (target.GetUserType() == "ADMIN")
            {
                errorMessage = "The system administrator account cannot be suspended.";
                return false;
            }

            if (target.IsSuspended())
            {
                errorMessage = "This user is already suspended.";
                return false;
            }

            reason = reason?.Trim();
            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to suspend a user.";
                return false;
            }

            if (adminActionCount >= MaxAdminActions)
            {
                errorMessage = "The system is full, no more admin actions can be logged.";
                return false;
            }

            target.SetSuspended(true);
            AddAdminAction(admin, "SUSPEND", $"User #{target.GetId()} ({target.GetEmail()})", reason);
            return true;
        }

        public bool UnsuspendUser(User admin, string userIdentifier, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can unsuspend a user.";
                return false;
            }

            User target = FindUserByIdOrEmail(userIdentifier);
            if (target == null)
            {
                errorMessage = "User not found.";
                return false;
            }

            if (!target.IsSuspended())
            {
                errorMessage = "This user is not suspended.";
                return false;
            }

            reason = reason?.Trim();
            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to unsuspend a user.";
                return false;
            }

            if (adminActionCount >= MaxAdminActions)
            {
                errorMessage = "The system is full, no more admin actions can be logged.";
                return false;
            }

            target.SetSuspended(false);
            AddAdminAction(admin, "UNSUSPEND", $"User #{target.GetId()} ({target.GetEmail()})", reason);
            return true;
        }

        // ---------- REQ-007 / KAN-11: removeJob / handleReport ----------
        // Design doc sections 6.9 and 6.17. Both actions are ADMIN-only, both log an AdminAction
        // (REMOVE_JOB / HANDLE_REPORT respectively - section 5.7), and removing a job always
        // closes out every still-NEW report linked to it (section 6.9, step 6), whether the job
        // was removed directly or as part of handling one particular report. Both reuse
        // AddAdminAction and the capacity/permission patterns already established by KAN-12 above.

        private Report FindReportById(int reportId)
        {
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetId() == reportId)
                {
                    return reports[i];
                }
            }
            return null;
        }

        // For Program to show the reports linked to a job before the admin decides whether to
        // remove it (design doc section 6.9, step 2).
        public Report[] GetReportsForJob(User admin, int jobId, out int count, out string errorMessage)
        {
            count = 0;
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can view reports.";
                return null;
            }

            Report[] results = new Report[reportCount];
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetJob() != null && reports[i].GetJob().GetId() == jobId)
                {
                    results[count] = reports[i];
                    count++;
                }
            }
            return results;
        }

        // For Program to let the admin pick a report to handle (design doc section 6.17, step 1).
        public Report[] GetNewReports(User admin, out int count, out string errorMessage)
        {
            count = 0;
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can view reports.";
                return null;
            }

            Report[] results = new Report[reportCount];
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetStatus() == "NEW")
                {
                    results[count] = reports[i];
                    count++;
                }
            }
            return results;
        }

        public bool RemoveJob(User admin, int jobId, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can remove a job.";
                return false;
            }

            Job job = FindJobById(jobId);
            if (job == null || job.GetStatus() == "REMOVED")
            {
                errorMessage = "Job not found or already removed.";
                return false;
            }

            reason = reason?.Trim();
            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to remove a job.";
                return false;
            }

            if (adminActionCount >= MaxAdminActions)
            {
                errorMessage = "The system is full, no more admin actions can be logged.";
                return false;
            }

            RemoveJobInternal(admin, job, reason);
            return true;
        }

        // Shared by RemoveJob and HandleReport (when the admin also chooses to remove the job
        // while handling a report) - design doc section 6.9, steps 4-6. The caller must already
        // have validated ADMIN permission, the job, the reason, and capacity for this AdminAction.
        private void RemoveJobInternal(User admin, Job job, string reason)
        {
            job.Remove();
            AddAdminAction(admin, "REMOVE_JOB", $"Job #{job.GetId()} ({job.GetTitle()})", reason);

            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetJob() == job && reports[i].GetStatus() == "NEW")
                {
                    reports[i].SetStatus("HANDLED");
                }
            }
        }

        // removeJobToo lets the admin also remove the reported job in the same action (design doc
        // section 6.17, step 3: "מסומן כטופל בלבד או הסרת המשרה"). When true, this logs a second
        // AdminAction (REMOVE_JOB) in addition to HANDLE_REPORT (step 5), and both must fit within
        // capacity before either is written - no partial logging.
        public bool HandleReport(User admin, int reportId, bool removeJobToo, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only a system administrator can handle a report.";
                return false;
            }

            Report report = FindReportById(reportId);
            if (report == null || report.GetStatus() != "NEW")
            {
                errorMessage = "Report not found or already handled.";
                return false;
            }

            reason = reason?.Trim();
            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to handle a report.";
                return false;
            }

            Job job = report.GetJob();
            bool willRemoveJob = removeJobToo && job != null && job.GetStatus() != "REMOVED";
            int requiredActions = willRemoveJob ? 2 : 1;

            if (adminActionCount + requiredActions > MaxAdminActions)
            {
                errorMessage = "The system is full, no more admin actions can be logged.";
                return false;
            }

            report.SetStatus("HANDLED");
            AddAdminAction(admin, "HANDLE_REPORT",
                $"Report #{report.GetId()} (Job #{(job != null ? job.GetId().ToString() : "?")})", reason);

            if (willRemoveJob)
            {
                RemoveJobInternal(admin, job, reason);
            }

            return true;
        }


        // ---------- REQ-005 / KAN-9: publish, edit and close jobs ----------
        public bool PublishJob(User employer, string title, string description,
                               string field, string location, string jobType,
                               out int createdJobId, out string errorMessage)
        {
            createdJobId = 0;
            errorMessage = "";
            if (!IsActiveEmployer(employer))
            {
                errorMessage = "Only an active employer can publish a job.";
                return false;
            }

            if (!HasValidJobDetails(title, description, field, location, jobType))
            {
                errorMessage = "Complete every field and choose a valid job type.";
                return false;
            }

            Company company = FindCompanyForEmployer(employer);
            if (company == null)
            {
                errorMessage = "No company is linked to this employer.";
                return false;
            }

            if (jobCount >= MaxJobs || nextJobId == int.MaxValue)
            {
                errorMessage = "The system is full, no more jobs can be added.";
                return false;
            }

            int openJobCount = 0;
            for (int i = 0; i < jobCount; i++)
            {
                if (jobs[i].GetEmployer() == employer && jobs[i].IsOpen())
                    openJobCount++;
            }
            if (openJobCount >= 20)
            {
                errorMessage = "An employer can have at most 20 open jobs.";
                return false;
            }

            // Create only after validation; failure leaves the array and counters untouched.
            Job job = new Job(nextJobId, title.Trim(), description.Trim(),
                              field.Trim(), location.Trim(), jobType, company, employer);
            jobs[jobCount] = job;
            jobCount++;
            createdJobId = nextJobId;
            nextJobId++;
            return true;
        }

        public bool EditJob(User employer, int jobId, string title, string description,
                            string field, string location, string jobType,
                            out string errorMessage)
        {
            errorMessage = "";
            if (!IsActiveEmployer(employer))
            {
                errorMessage = "Only an active employer can edit a job.";
                return false;
            }

            Job job = FindJobById(jobId);
            if (job == null || job.GetEmployer() != employer || !job.IsOpen())
            {
                errorMessage = "Open job not found or it does not belong to you.";
                return false;
            }

            if (!HasValidJobDetails(title, description, field, location, jobType))
            {
                errorMessage = "Complete every field and choose a valid job type.";
                return false;
            }

            return job.UpdateDetails(title, description, field, location, jobType);
        }

        public bool CloseJob(User employer, int jobId, out string errorMessage)
        {
            errorMessage = "";
            if (!IsActiveEmployer(employer))
            {
                errorMessage = "Only an active employer can close a job.";
                return false;
            }

            Job job = FindJobById(jobId);
            if (job == null || job.GetEmployer() != employer || !job.Close())
            {
                errorMessage = "Open job not found or it does not belong to you.";
                return false;
            }
            return true;
        }

        private static bool IsActiveEmployer(User employer) =>
            employer != null && employer.GetUserType() == "EMPLOYER" &&
            !employer.IsSuspended();

        private static bool HasValidJobDetails(string title, string description,
                                               string field, string location,
                                               string jobType) =>
            !string.IsNullOrWhiteSpace(title) &&
            !string.IsNullOrWhiteSpace(description) &&
            !string.IsNullOrWhiteSpace(field) &&
            !string.IsNullOrWhiteSpace(location) && Job.IsValidJobType(jobType);

        private Company FindCompanyForEmployer(User employer)
        {
            for (int i = 0; i < companyCount; i++)
            {
                if (companies[i].GetEmployer() == employer)
                    return companies[i];
            }
            return null;
        }

        // REQ-003 / KAN-7: all supplied filters must match an open job.
        public Job[] SearchJobs(string keyword, string field, string location,
                                string jobType, out int resultCount)
        {
            Job[] results = new Job[jobCount];
            resultCount = 0;
            for (int i = 0; i < jobCount; i++)
            {
                Job job = jobs[i];
                if (!job.IsOpen()) continue;
                if (!string.IsNullOrWhiteSpace(keyword) &&
                    !ContainsIgnoreCase(job.GetTitle(), keyword) &&
                    !ContainsIgnoreCase(job.GetDescription(), keyword)) continue;
                if (!string.IsNullOrWhiteSpace(field) &&
                    !ContainsIgnoreCase(job.GetField(), field)) continue;
                if (!string.IsNullOrWhiteSpace(location) &&
                    !ContainsIgnoreCase(job.GetLocation(), location)) continue;
                if (!string.IsNullOrWhiteSpace(jobType) &&
                    !string.Equals(job.GetJobType(), jobType.Trim(), StringComparison.OrdinalIgnoreCase)) continue;

                results[resultCount] = job;
                resultCount++;
            }
            return results;
        }

        private static bool ContainsIgnoreCase(string value, string filter) =>
            value != null && value.IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;

        // REQ-010 / KAN-14: hide administratively removed jobs from public viewers.
        public Job GetVisibleJobById(User viewer, int jobId)
        {
            Job job = FindJobById(jobId);
            if (job == null) return null;
            if (job.GetStatus() == "REMOVED" &&
                (viewer == null || viewer.GetUserType() == "CANDIDATE")) return null;
            if (viewer != null && viewer.GetUserType() == "EMPLOYER" &&
                job.GetEmployer() != viewer) return null;
            return job;
        }

        // REQ-006 / KAN-10: only the job owner may inspect candidate profiles.
        public Application[] GetApplicantsForJob(User employer, int jobId,
                                                 out int count, out string errorMessage)
        {
            count = 0;
            errorMessage = "";
            Job job = FindJobById(jobId);
            if (!IsActiveEmployer(employer) || job == null || job.GetEmployer() != employer)
            {
                errorMessage = "Job not found or it does not belong to you.";
                return Array.Empty<Application>();
            }

            Application[] matches = new Application[applicationCount];
            for (int i = 0; i < applicationCount; i++)
            {
                if (applications[i].GetJob() == job)
                    matches[count++] = applications[i];
            }
            return matches;
        }

        public bool UpdateApplicationStatus(User employer, int applicationId,
                                            string newStatus, out string errorMessage)
        {
            errorMessage = "";
            if (!IsActiveEmployer(employer))
            {
                errorMessage = "Only an active employer can update applications.";
                return false;
            }
            if (newStatus != "NEW" && newStatus != "UNDER_REVIEW" &&
                newStatus != "INTERVIEW" && newStatus != "ACCEPTED" &&
                newStatus != "REJECTED")
            {
                errorMessage = "Invalid application status.";
                return false;
            }
            for (int i = 0; i < applicationCount; i++)
            {
                Application application = applications[i];
                if (application.GetId() != applicationId) continue;
                if (application.GetJob().GetEmployer() != employer)
                {
                    errorMessage = "Application not found or it does not belong to your job.";
                    return false;
                }

                // KAN-11 self-review: once an admin removes a job (REQ-007, section 6.9) it is
                // no longer legitimate - the employer should not be able to keep moving its
                // applicants forward (e.g. to ACCEPTED) as if nothing happened. CLOSED jobs are
                // unaffected: managing applicants after a normal close is expected behavior.
                if (application.GetJob().GetStatus() == "REMOVED")
                {
                    errorMessage = "This job was removed by an administrator; its applications can no longer be updated.";
                    return false;
                }

                application.SetStatus(newStatus);
                return true;
            }
            errorMessage = "Application not found or it does not belong to your job.";
            return false;
        }

        // Candidates may inspect only their own application statuses.
        public Application[] GetMyApplications(User candidate, out int count)
        {
            count = 0;
            if (candidate == null || candidate.GetUserType() != "CANDIDATE" ||
                candidate.IsSuspended()) return Array.Empty<Application>();

            Application[] mine = new Application[applicationCount];
            for (int i = 0; i < applicationCount; i++)
            {
                if (applications[i].GetCandidate() == candidate)
                    mine[count++] = applications[i];
            }
            return mine;
        }

        // REQ-012 / KAN-16: keep ownership and application counting in JobSystem.
        public Job[] GetEmployerJobs(User employer, out int count)
        {
            count = 0;
            if (!IsActiveEmployer(employer)) return Array.Empty<Job>();
            Job[] owned = new Job[jobCount];
            for (int i = 0; i < jobCount; i++)
            {
                if (jobs[i].GetEmployer() == employer)
                    owned[count++] = jobs[i];
            }
            return owned;
        }

        public int CountApplicationsForEmployerJob(User employer, int jobId)
        {
            Job job = FindJobById(jobId);
            if (!IsActiveEmployer(employer) || job == null || job.GetEmployer() != employer)
                return 0;
            int count = 0;
            for (int i = 0; i < applicationCount; i++)
            {
                if (applications[i].GetJob() == job) count++;
            }
            return count;
        }
    }
}
