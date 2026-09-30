using System;

namespace JobApp.Models
{
    // REQ-014: audit log record of an action performed by a system admin. Never edited after creation.
    public class AdminAction
    {
        private User admin;
        private string actionType; // SUSPEND / UNSUSPEND / REMOVE_JOB / HANDLE_REPORT
        private string target;
        private string reason;
        private string actionDate;

        public AdminAction(User admin, string actionType, string target, string reason)
        {
            this.admin = admin;
            this.actionType = actionType;
            this.target = target;
            this.reason = reason;
            this.actionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }

        public User GetAdmin() => admin;
        public string GetActionType() => actionType;
        public string GetTarget() => target;
        public string GetReason() => reason;
        public string GetActionDate() => actionDate;

        public override string ToString()
        {
            string adminName = admin != null ? admin.GetName() : "";
            return $"[{actionDate}] {adminName} | {actionType} | {target} | Reason: {reason}";
        }
    }
}
