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

        private User[] users;
        private int userCount;

        private Company[] companies;
        private int companyCount;

        private int nextUserId;
        private int nextCompanyId;

        public JobSystem()
        {
            users = new User[MaxUsers];
            userCount = 0;

            companies = new Company[MaxCompanies];
            companyCount = 0;

            nextUserId = 1;
            nextCompanyId = 1;

            SeedAdmin();
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
    }
}
