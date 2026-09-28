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
                Console.WriteLine("3 - Search open jobs (not implemented yet)");
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
                        Console.WriteLine("Job search is not implemented yet (KAN-9 / KAN-14).");
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

        // Logged-in loop: only two actions available until role-specific menus exist.
        // loggedInUser is a local variable (not a field), so once this method returns
        // there is no way to reach a protected action without logging in again (T-13).
        static void RunLoggedInSession(JobSystem system, User loggedInUser)
        {
            bool isCandidate = loggedInUser.GetUserType() == "CANDIDATE";

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
                }
                Console.WriteLine("4 - Logout");
                Console.Write("Choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine(loggedInUser);
                        break;
                    case "2":
                        if (isCandidate)
                        {
                            HandleUpdateCandidateProfile(system, loggedInUser);
                        }
                        else
                        {
                            Console.WriteLine("Invalid choice, please try again.");
                        }
                        break;
                    case "3":
                        if (isCandidate)
                        {
                            HandleSubmitApplication(system, loggedInUser);
                        }
                        else
                        {
                            Console.WriteLine("Invalid choice, please try again.");
                        }
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
