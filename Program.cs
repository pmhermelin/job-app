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

            // Role-specific menus (candidate/employer/admin) are implemented in later stories
            // (KAN-8, KAN-13 and teammates' stories). For now, a minimal logged-in loop
            // just confirms the session; logging out is handled by HandleLogout below (REQ-013 / KAN-17).
            RunLoggedInSession(loggedInUser);
        }

        // Logged-in loop: only two actions available until role-specific menus exist.
        // loggedInUser is a local variable (not a field), so once this method returns
        // there is no way to reach a protected action without logging in again (T-13).
        static void RunLoggedInSession(User loggedInUser)
        {
            bool loggedIn = true;
            while (loggedIn)
            {
                Console.WriteLine();
                Console.WriteLine($"==== Logged in as {loggedInUser.GetName()} ({loggedInUser.GetUserType()}) ====");
                Console.WriteLine("1 - View my account info");
                Console.WriteLine("2 - Logout");
                Console.Write("Choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine(loggedInUser);
                        break;
                    case "2":
                        loggedIn = false;
                        HandleLogout(loggedInUser);
                        break;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
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
