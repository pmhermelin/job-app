using System;

namespace JobApp.Models
{
    // REQ-007 / REQ-008: an immutable audit-log entry for an action a system admin performed
    // (design doc section 5.7). Once created it cannot be edited - there is no setter for any field.
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
            return $"Admin Action | Type: {actionType} | Admin: {adminName} | Target: {target} | Reason: {reason} | Date: {actionDate}";
        }
    }
}
