// ============================================================
// תוספות ל-JobSystem.cs עבור KAN-11, KAN-12, KAN-15, KAN-18
// נכתב מול הקוד האמיתי של JobSystem.cs, Job.cs, User.cs שנשלחו בצ'אט.
// שימו לב: IsBlank() ו-FindJobById() כבר קיימים ב-JobSystem - לא לשכפל אותם.
// ============================================================

// ---------- שלב 1: להוסיף using בראש הקובץ (אם עוד לא קיים) ----------
// using JobApp.Models;   // רק אם Report/AdminAction יושבים ב-namespace הזה, כמו שאר המחלקות

// ---------- שלב 2: להוסיף שדות חדשים, ליד ה-applications הקיימים ----------
/*
        private const int MaxReports = 200;
        private const int MaxAdminActions = 500;

        private Report[] reports;
        private int reportCount;

        private AdminAction[] adminActions;
        private int adminActionCount;

        private int nextReportId;
*/

// ---------- שלב 3: להוסיף אתחול בתוך הבנאי JobSystem(), ליד אתחול ה-applications ----------
/*
            reports = new Report[MaxReports];
            reportCount = 0;

            adminActions = new AdminAction[MaxAdminActions];
            adminActionCount = 0;

            nextReportId = 1;
*/

// ---------- שלב 4: המתודות עצמן - להוסיף בתוך המחלקה JobSystem, אחרי SubmitApplication ----------

        // ---------- REQ-011 / KAN-15: createReport ----------
        // אותה תבנית בדיוק כמו SubmitApplication: bool + out errorMessage, בלי Console.WriteLine.
        public bool CreateReport(User reporter, int jobId, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (reporter == null || reporter.GetUserType() != "CANDIDATE")
            {
                errorMessage = "Only job seekers can report a job.";
                return false;
            }

            if (reporter.IsSuspended())
            {
                errorMessage = "This account has been suspended. Contact the system administrator.";
                return false;
            }

            Job job = FindJobById(jobId);
            if (job == null || job.GetStatus() == "REMOVED")
            {
                errorMessage = "Job not found or has already been removed.";
                return false;
            }

            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to report a job.";
                return false;
            }

            if (reportCount >= MaxReports)
            {
                errorMessage = "The system is full, no more reports can be added.";
                return false;
            }

            Report report = new Report(nextReportId, reporter, job, reason);
            reports[reportCount] = report;
            reportCount++;
            nextReportId++;

            return true;
        }

        // Helper for Program: reports linked to a specific job, e.g. to show before removing it.
        public Report[] GetReportsForJob(int jobId, out int count)
        {
            Report[] result = new Report[reportCount];
            count = 0;
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetJob().GetId() == jobId)
                {
                    result[count] = reports[i];
                    count++;
                }
            }
            return result;
        }

        // ---------- REQ-007 / KAN-11: removeJob ----------
        public bool RemoveJob(User admin, int jobId, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only an admin can remove a job.";
                return false;
            }

            Job job = FindJobById(jobId);
            if (job == null)
            {
                errorMessage = "Job not found.";
                return false;
            }

            if (job.GetStatus() == "REMOVED")
            {
                errorMessage = "This job has already been removed.";
                return false;
            }

            if (IsBlank(reason))
            {
                errorMessage = "A reason is required to remove a job.";
                return false;
            }

            // Logical delete only - the job stays in the array for record-keeping,
            // it just stops appearing as OPEN (GetOpenJobs already filters on IsOpen()).
            job.SetStatus("REMOVED");

            AddAdminAction(admin, "REMOVE_JOB", $"Job #{job.GetId()} - {job.GetTitle()}", reason);

            // Any still-open (NEW) reports on this job are now resolved by the removal.
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetJob() == job && reports[i].GetStatus() == "NEW")
                {
                    reports[i].SetStatus("HANDLED");
                }
            }

            return true;
        }

        // ---------- REQ-007 / KAN-11: handleReport ----------
        public bool HandleReport(User admin, int reportId, bool removeJobToo, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only an admin can handle reports.";
                return false;
            }

            Report report = FindReportById(reportId);
            if (report == null)
            {
                errorMessage = "Report not found.";
                return false;
            }

            if (report.GetStatus() != "NEW")
            {
                errorMessage = "This report was already handled.";
                return false;
            }

            report.SetStatus("HANDLED");
            AddAdminAction(admin, "HANDLE_REPORT",
                $"Report #{report.GetId()} on Job #{report.GetJob().GetId()}", reason);

            if (removeJobToo && report.GetJob().GetStatus() != "REMOVED")
            {
                // Re-uses RemoveJob, which adds its own REMOVE_JOB admin action
                // and marks any other NEW reports on the same job as HANDLED too.
                string removeError;
                RemoveJob(admin, report.GetJob().GetId(), reason, out removeError);
            }

            return true;
        }

        // ---------- REQ-008 / KAN-12: suspendUser / unsuspendUser ----------
        public bool SuspendUser(User admin, int targetUserId, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only an admin can suspend a user.";
                return false;
            }

            User target = FindUserById(targetUserId);
            if (target == null)
            {
                errorMessage = "User not found.";
                return false;
            }

            if (target.GetUserType() == "ADMIN")
            {
                errorMessage = "An admin account cannot be suspended.";
                return false;
            }

            if (target.IsSuspended())
            {
                errorMessage = "This user is already suspended.";
                return false;
            }

            if (IsBlank(reason))
            {
                errorMessage = "A reason is required.";
                return false;
            }

            target.SetSuspended(true);
            AddAdminAction(admin, "SUSPEND", $"User #{target.GetId()} - {target.GetName()}", reason);

            return true;
        }

        public bool UnsuspendUser(User admin, int targetUserId, string reason, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only an admin can unsuspend a user.";
                return false;
            }

            User target = FindUserById(targetUserId);
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

            if (IsBlank(reason))
            {
                errorMessage = "A reason is required.";
                return false;
            }

            target.SetSuspended(false);
            AddAdminAction(admin, "UNSUSPEND", $"User #{target.GetId()} - {target.GetName()}", reason);

            return true;
        }

        // ---------- REQ-014 / KAN-18: admin log ----------
        // Returns the data only; Program.cs is responsible for printing it (same split as GetOpenJobs).
        public AdminAction[] GetAdminLog(User admin, out string errorMessage)
        {
            errorMessage = "";

            if (admin == null || admin.GetUserType() != "ADMIN")
            {
                errorMessage = "Only an admin can view the admin log.";
                return null;
            }

            AdminAction[] result = new AdminAction[adminActionCount];
            for (int i = 0; i < adminActionCount; i++)
            {
                result[i] = adminActions[i];
            }
            return result;
        }

        // ---------- Helper methods (same pattern as FindJobById) ----------

        private Report FindReportById(int id)
        {
            for (int i = 0; i < reportCount; i++)
            {
                if (reports[i].GetId() == id)
                {
                    return reports[i];
                }
            }
            return null;
        }

        private User FindUserById(int id)
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

        private void AddAdminAction(User admin, string actionType, string target, string reason)
        {
            if (adminActionCount >= adminActions.Length)
            {
                return; // log is full - silently skip rather than break the calling action
            }
            adminActions[adminActionCount] = new AdminAction(admin, actionType, target, reason);
            adminActionCount++;
        }
