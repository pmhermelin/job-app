using System;
using JobApp.Models;

namespace JobApp
{
    // Entry point; displays menus and forwards actions to JobSystem (design doc, section 8).
    // Program does not access the data arrays directly and does not change data - display and navigation only.
    class Program
    {
        static void Main(string[] args)
        {
            // Force UTF-8 output/input so special characters display correctly across terminals.
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            JobSystem system = new JobSystem();
            bool running = true;

            while (running)
            {
                Console.WriteLine();
                Console.WriteLine("==== Job Listing Management System ====");
                Console.WriteLine("1 - Register");
                Console.WriteLine("2 - Login");
                Console.WriteLine("3 - Search open jobs");
                Console.WriteLine("4 - Exit");
                Console.Write("Choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        HandleRegister(system);
                        break;
                    case "2":
                        HandleLogin(system);
                        break;
                    case "3":
                        HandleSearchJobs(system, null);
                        break;
                    case "4":
                        running = false;
                        Console.WriteLine("Goodbye!");
                        break;
                    default:
                        // Invalid choice (including non-numeric text) - message and back to menu, no crash (T-12).
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        // REQ-001 / KAN-5: capture registration details, call JobSystem.RegisterUser, display the result.
        static void HandleRegister(JobSystem system)
        {
            Console.WriteLine();
            Console.WriteLine("-- Register --");

            Console.Write("Full name: ");
            string name = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Password (8+ chars, at least one letter and one digit): ");
            string password = Console.ReadLine();

            Console.WriteLine("Account type:");
            Console.WriteLine("1 - Job seeker");
            Console.WriteLine("2 - Employer");
            Console.Write("Choice: ");
            string typeChoice = Console.ReadLine();

            string userType;
            if (typeChoice == "1")
            {
                userType = "CANDIDATE";
            }
            else if (typeChoice == "2")
            {
                userType = "EMPLOYER";
            }
            else
            {
                Console.WriteLine("Invalid choice. Registration cancelled.");
                return;
            }

            string companyName = "";
            string companyDescription = "";

            if (userType == "EMPLOYER")
            {
                Console.Write("Company name: ");
                companyName = Console.ReadLine();
                Console.Write("Company description: ");
                companyDescription = Console.ReadLine();
            }

            User newUser = system.RegisterUser(name, email, password, userType,
                                                companyName, companyDescription,
                                                out string errorMessage);

            if (newUser == null)
            {
                Console.WriteLine($"Registration failed: {errorMessage}");
            }
            else
            {
                Console.WriteLine("Registration successful!");
                Console.WriteLine(newUser);
            }
        }

        // REQ-002 / KAN-6: capture login details, call JobSystem.Login, display the result.
        static void HandleLogin(JobSystem system)
        {
            Console.WriteLine();
            Console.WriteLine("-- Login --");

            Console.Write("Email: ");
            string email = Console.ReadLine();

            Console.Write("Password: ");
            string password = Console.ReadLine();

            User loggedInUser = system.Login(email, password, out string errorMessage);

            if (loggedInUser == null)
            {
                Console.WriteLine($"Login failed: {errorMessage}");
                return;
            }

            Console.WriteLine("Login successful!");
            Console.WriteLine(loggedInUser);

            // Role-specific menus are implemented incrementally: candidates can now update their
            // profile (REQ-009 / KAN-13); other role menus (KAN-8 and teammates' stories) are still pending.
            // Logging out is handled by HandleLogout below (REQ-013 / KAN-17).
            RunLoggedInSession(system, loggedInUser);
        }

        // KAN-101 / KAN-102: each logged-in menu number identifies one action across all roles.
        // KAN-15 / KAN-114: "5 - Report a job" fills the number that was left unused after the
        // KAN-101/102 renumbering, so the menu has no more gaps.
        // loggedInUser is a local variable (not a field), so once this method returns
        // there is no way to reach a protected action without logging in again (T-13).
        static void RunLoggedInSession(JobSystem system, User loggedInUser)
        {
            bool isCandidate = loggedInUser.GetUserType() == "CANDIDATE";
            bool isEmployer = loggedInUser.GetUserType() == "EMPLOYER";

            bool loggedIn = true;
            while (loggedIn)
            {
                Console.WriteLine();
                Console.WriteLine($"==== Logged in as {loggedInUser.GetName()} ({loggedInUser.GetUserType()}) ====");
                Console.WriteLine("1 - View my account info");
                if (isCandidate)
                {
                    // REQ-009 / KAN-13: only job seekers may update a candidate profile (design doc 4.3).
                    Console.WriteLine("2 - Update my profile");
                    // REQ-004 / KAN-8: only job seekers may submit applications (design doc 4.3).
                    Console.WriteLine("3 - Apply to a job");
                    // REQ-011 / KAN-15: only job seekers may report a job (design doc 4.3).
                    Console.WriteLine("5 - Report a job");
                    Console.WriteLine("6 - Search open jobs");
                    Console.WriteLine("7 - My applications");
                }
                if (isEmployer)
                {
                    Console.WriteLine("8 - Publish a job");
                    Console.WriteLine("9 - Edit a job");
                    Console.WriteLine("10 - Close a job");
                    Console.WriteLine("11 - My jobs");
                    Console.WriteLine("12 - View applicants for a job");
                    Console.WriteLine("13 - Update application status");
                }
                Console.WriteLine("4 - Logout");
                Console.Write("Choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine(loggedInUser);
                        break;
                    case "2" when isCandidate:
                        HandleUpdateCandidateProfile(system, loggedInUser);
                        break;
                    case "3" when isCandidate:
                        HandleSubmitApplication(system, loggedInUser);
                        break;
                    case "5" when isCandidate:
                        HandleReportJob(system, loggedInUser);
                        break;
                    case "6" when isCandidate:
                        HandleSearchJobs(system, loggedInUser);
                        break;
                    case "7" when isCandidate:
                        HandleMyApplications(system, loggedInUser);
                        break;
                    case "8" when isEmployer:
                        HandlePublishJob(system, loggedInUser);
                        break;
                    case "9" when isEmployer:
                        HandleEditJob(system, loggedInUser);
                        break;
                    case "10" when isEmployer:
                        HandleCloseJob(system, loggedInUser);
                        break;
                    case "11" when isEmployer:
                        HandleEmployerJobs(system, loggedInUser);
                        break;
                    case "12" when isEmployer:
                        HandleApplicants(system, loggedInUser);
                        break;
                    case "13" when isEmployer:
                        HandleApplicationStatus(system, loggedInUser);
                        break;
                    case "4":
                        loggedIn = false;
                        HandleLogout(loggedInUser);
                        break;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }

        // REQ-009 / KAN-13: capture profile details, call JobSystem.UpdateCandidateProfile, display the result.
        static void HandleUpdateCandidateProfile(JobSystem system, User loggedInUser)
        {
            Console.WriteLine();
            Console.WriteLine("-- Update my profile --");

            Console.Write("Education: ");
            string education = Console.ReadLine();

            Console.Write("Experience: ");
            string experience = Console.ReadLine();

            Console.Write("Skills: ");
            string skills = Console.ReadLine();

            Console.Write("Professional summary: ");
            string summary = Console.ReadLine();

            bool success = system.UpdateCandidateProfile(loggedInUser, education, experience, skills, summary,
                                                           out string errorMessage);

            if (!success)
            {
                Console.WriteLine($"Profile update failed: {errorMessage}");
                return;
            }

            Console.WriteLine("Profile updated successfully!");
            Console.WriteLine(loggedInUser.GetCandidateProfile());
        }

        // REQ-004 / KAN-8: list open jobs, capture a job id, call JobSystem.SubmitApplication, display the result.
        static void HandleSubmitApplication(JobSystem system, User loggedInUser)
        {
            Console.WriteLine();
            Console.WriteLine("-- Apply to a job --");

            Job[] openJobs = system.GetOpenJobs(out int openCount);

            if (openCount == 0)
            {
                Console.WriteLine("There are no open jobs right now.");
                return;
            }

            for (int i = 0; i < openCount; i++)
            {
                Console.WriteLine(openJobs[i]);
            }

            Console.Write("Job id to apply to: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }

            bool success = system.SubmitApplication(loggedInUser, jobId, out string errorMessage);

            if (!success)
            {
                Console.WriteLine($"Application failed: {errorMessage}");
                return;
            }

            Console.WriteLine("Application submitted successfully!");
        }

        // REQ-011 / KAN-15: list open jobs (same as HandleSubmitApplication), capture a job id and
        // reason, call JobSystem.CreateReport, display the result.
        // Note: CreateReport also allows reporting a CLOSED job (only REMOVED is blocked) - this list
        // shows only OPEN jobs, same as everywhere else a candidate browses jobs (search/apply), so a
        // CLOSED job can still be reported if its id is already known (e.g. from Application history).
        static void HandleReportJob(JobSystem system, User loggedInUser)
        {
            Console.WriteLine();
            Console.WriteLine("-- Report a job --");

            Job[] openJobs = system.GetOpenJobs(out int openCount);

            if (openCount > 0)
            {
                for (int i = 0; i < openCount; i++)
                {
                    Console.WriteLine(openJobs[i]);
                }
            }

            Console.Write("Job id to report: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }

            Console.Write("Reason: ");
            string reason = Console.ReadLine();

            bool success = system.CreateReport(loggedInUser, jobId, reason, out string errorMessage);

            if (!success)
            {
                Console.WriteLine($"Report failed: {errorMessage}");
                return;
            }

            Console.WriteLine("Report submitted successfully. Thank you - our team will review it.");
        }

        // REQ-003 / KAN-7: Enter skips any filter; matching remains in JobSystem.
        static void HandleSearchJobs(JobSystem system, User viewer)
        {
            Console.WriteLine("-- Search open jobs (press Enter to skip a filter) --");
            Console.Write("Keyword: ");
            string keyword = Console.ReadLine();
            Console.Write("Field: ");
            string field = Console.ReadLine();
            Console.Write("Location: ");
            string location = Console.ReadLine();
            Console.Write("Job type (Full-time / Part-time / Temporary / Internship): ");
            string jobType = Console.ReadLine();

            Job[] results = system.SearchJobs(keyword, field, location, jobType, out int count);
            if (count == 0)
            {
                Console.WriteLine("No open jobs match your search.");
                return;
            }
            for (int i = 0; i < count; i++)
                Console.WriteLine(results[i]);

            Console.Write("Enter a job id to view its details, or press Enter to return: ");
            string selection = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(selection)) return;
            if (!int.TryParse(selection, out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }
            HandleViewJobDetails(system, viewer, jobId);
        }

        // REQ-010 / KAN-14: show full details and offer application only to a candidate.
        static void HandleViewJobDetails(JobSystem system, User viewer, int jobId)
        {
            Job job = system.GetVisibleJobById(viewer, jobId);
            if (job == null)
            {
                Console.WriteLine("Job not found or unavailable.");
                return;
            }

            Console.WriteLine($"Job #{job.GetId()}");
            Console.WriteLine($"Title: {job.GetTitle()}");
            Console.WriteLine($"Company: {job.GetCompany()?.GetName() ?? "Unknown"}");
            Console.WriteLine($"Description: {job.GetDescription()}");
            Console.WriteLine($"Field: {job.GetField()}");
            Console.WriteLine($"Location: {job.GetLocation()}");
            Console.WriteLine($"Job type: {job.GetJobType()}");
            Console.WriteLine($"Status: {job.GetStatus()}");

            if (viewer == null || viewer.GetUserType() != "CANDIDATE" || !job.IsOpen())
            {
                Console.WriteLine("Press Enter to return.");
                Console.ReadLine();
                return;
            }

            Console.Write("1 - Apply to this job, Enter - Return: ");
            if (Console.ReadLine() == "1")
            {
                if (system.SubmitApplication(viewer, jobId, out string errorMessage))
                    Console.WriteLine("Application submitted successfully!");
                else
                    Console.WriteLine($"Application failed: {errorMessage}");
            }
        }

        // REQ-006 / KAN-10: the service checks ownership before exposing profiles.
        static void HandleApplicants(JobSystem system, User employer)
        {
            Console.Write("Job id: ");
            if (!int.TryParse(Console.ReadLine(), out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }
            Application[] applicants = system.GetApplicantsForJob(employer, jobId,
                                                                  out int count, out string errorMessage);
            if (errorMessage != "")
            {
                Console.WriteLine(errorMessage);
                return;
            }
            if (count == 0) Console.WriteLine("No applicants for this job yet.");
            for (int i = 0; i < count; i++)
            {
                User candidate = applicants[i].GetCandidate();
                Console.WriteLine(applicants[i]);
                Console.WriteLine($"Candidate: {candidate.GetName()} | {candidate.GetEmail()}");
                Console.WriteLine(candidate.GetCandidateProfile());
            }
        }

        static void HandleApplicationStatus(JobSystem system, User employer)
        {
            Console.Write("Application id: ");
            if (!int.TryParse(Console.ReadLine(), out int applicationId))
            {
                Console.WriteLine("Invalid application id.");
                return;
            }
            Console.WriteLine("Status: 1 - NEW, 2 - UNDER_REVIEW, 3 - INTERVIEW, 4 - ACCEPTED, 5 - REJECTED");
            Console.Write("Choice: ");
            string status;
            switch (Console.ReadLine())
            {
                case "1": status = "NEW"; break;
                case "2": status = "UNDER_REVIEW"; break;
                case "3": status = "INTERVIEW"; break;
                case "4": status = "ACCEPTED"; break;
                case "5": status = "REJECTED"; break;
                default: status = ""; break;
            }
            if (system.UpdateApplicationStatus(employer, applicationId, status, out string errorMessage))
                Console.WriteLine("Application status updated.");
            else
                Console.WriteLine($"Update failed: {errorMessage}");
        }

        static void HandleMyApplications(JobSystem system, User candidate)
        {
            Application[] mine = system.GetMyApplications(candidate, out int count);
            if (count == 0) Console.WriteLine("You have no applications yet.");
            for (int i = 0; i < count; i++) Console.WriteLine(mine[i]);
        }

        // REQ-012 / KAN-16: list only this employer's jobs and navigate by id.
        static void HandleEmployerJobs(JobSystem system, User employer)
        {
            Job[] owned = system.GetEmployerJobs(employer, out int count);
            if (count == 0)
            {
                Console.WriteLine("You have no jobs yet.");
                return;
            }
            for (int i = 0; i < count; i++)
            {
                Job job = owned[i];
                int applications = system.CountApplicationsForEmployerJob(employer, job.GetId());
                Console.WriteLine($"Job #{job.GetId()} | {job.GetTitle()} | {job.GetStatus()} | Applications: {applications}");
            }

            Console.Write("Job id to manage (Enter to return): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return;
            if (!int.TryParse(input, out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                if (owned[i].GetId() == jobId) found = true;
            }
            if (!found)
            {
                Console.WriteLine("Job not found in your list.");
                return;
            }

            Console.Write("1 - View details, 2 - Edit, 3 - Close, Enter - Return: ");
            switch (Console.ReadLine())
            {
                case "1": HandleViewJobDetails(system, employer, jobId); break;
                case "2": HandleEditJob(system, employer, jobId); break;
                case "3": HandleCloseJob(system, employer, jobId); break;
            }
        }

        // REQ-005 / KAN-9: employer actions are routed through JobSystem.
        static void HandlePublishJob(JobSystem system, User employer)
        {
            Console.WriteLine("-- Publish a job --");
            ReadJobDetails(out string title, out string description, out string field,
                           out string location, out string jobType);
            if (system.PublishJob(employer, title, description, field, location,
                                  jobType, out int createdJobId, out string errorMessage))
                Console.WriteLine($"Job #{createdJobId} published successfully.");
            else
                Console.WriteLine($"Publishing failed: {errorMessage}");
        }

        static void HandleEditJob(JobSystem system, User employer)
        {
            Console.WriteLine("-- Edit a job --");
            Console.Write("Job id: ");
            if (!int.TryParse(Console.ReadLine(), out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }

            HandleEditJob(system, employer, jobId);
        }

        static void HandleEditJob(JobSystem system, User employer, int jobId)
        {

            ReadJobDetails(out string title, out string description, out string field,
                           out string location, out string jobType);
            if (system.EditJob(employer, jobId, title, description, field,
                               location, jobType, out string errorMessage))
                Console.WriteLine("Job updated successfully.");
            else
                Console.WriteLine($"Update failed: {errorMessage}");
        }

        static void HandleCloseJob(JobSystem system, User employer)
        {
            Console.WriteLine("-- Close a job --");
            Console.Write("Job id: ");
            if (!int.TryParse(Console.ReadLine(), out int jobId))
            {
                Console.WriteLine("Invalid job id.");
                return;
            }

            HandleCloseJob(system, employer, jobId);
        }

        static void HandleCloseJob(JobSystem system, User employer, int jobId)
        {

            if (system.CloseJob(employer, jobId, out string errorMessage))
                Console.WriteLine("Job closed successfully.");
            else
                Console.WriteLine($"Closing failed: {errorMessage}");
        }

        static void ReadJobDetails(out string title, out string description,
                                   out string field, out string location,
                                   out string jobType)
        {
            Console.Write("Title: ");
            title = Console.ReadLine();
            Console.Write("Description: ");
            description = Console.ReadLine();
            Console.Write("Field: ");
            field = Console.ReadLine();
            Console.Write("Location: ");
            location = Console.ReadLine();
            Console.WriteLine("Job type: 1 - Full-time, 2 - Part-time, 3 - Temporary, 4 - Internship");
            Console.Write("Choice: ");
            switch (Console.ReadLine())
            {
                case "1": jobType = "Full-time"; break;
                case "2": jobType = "Part-time"; break;
                case "3": jobType = "Temporary"; break;
                case "4": jobType = "Internship"; break;
                default: jobType = ""; break;
            }
        }

        // REQ-013 / KAN-17: end the logged-in session and return to the main menu.
        // Logic per design doc section 6.15:
        //   1. End the inner (logged-in) menu loop - handled by the caller via RunLoggedInSession.
        //   2. Reset the reference to the logged-in user within Program - loggedInUser simply goes
        //      out of scope when this method and RunLoggedInSession return, so no protected action
        //      is reachable afterward without logging in again.
        //   3. Control returns to the main menu loop in Main().
        static void HandleLogout(User loggedInUser)
        {
            Console.WriteLine($"Goodbye, {loggedInUser.GetName()}. You have been logged out.");
        }
    }
}
