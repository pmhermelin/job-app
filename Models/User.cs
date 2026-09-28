using System;

namespace JobApp.Models
{
    // REQ-001 / REQ-002: represents a user account, login details, permission type and suspension state.
    public class User
    {
        private int id;
        private string name;
        private string email;
        private string password;       // no getter - never exposed
        private string userType;       // CANDIDATE / EMPLOYER / ADMIN
        private bool suspended;
        private CandidateProfile candidateProfile; // job seekers only, otherwise null

        public User(int id, string name, string email, string password, string userType)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.password = password;
            this.userType = userType;
            this.suspended = false;
            this.candidateProfile = null;
        }

        public int GetId() => id;
        public string GetName() => name;
        public string GetEmail() => email;
        public string GetUserType() => userType;

        // Compares a password without ever exposing it (no getPassword).
        public bool CheckPassword(string candidatePassword)
        {
            return password == candidatePassword;
        }

        public bool IsSuspended() => suspended;
        public void SetSuspended(bool value) => suspended = value;

        public CandidateProfile GetCandidateProfile() => candidateProfile;
        public void SetCandidateProfile(CandidateProfile profile) => candidateProfile = profile;

        // Returns id, name, email, type and status - never the password.
        public override string ToString()
        {
            string status = suspended ? "Suspended" : "Active";
            return $"User #{id} | {name} | {email} | {userType} | {status}";
        }
    }
}
