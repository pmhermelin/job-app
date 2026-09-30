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
        // Design doc section 16 ("פורמט תאריך"): "DateTime פנימי" - kept as an actual DateTime here
        // (not a pre-formatted string like Application/Report use), so GetActionDate() below can
        // format it to match section 9.3's display example ("Date: 04/08/2026 17:30") exactly.
        private DateTime actionDate;

        public AdminAction(User admin, string actionType, string target, string reason)
        {
            this.admin = admin;
            this.actionType = actionType;
            this.target = target;
            this.reason = reason;
            this.actionDate = DateTime.Now;
        }

        public User GetAdmin() => admin;
        public string GetActionType() => actionType;
        public string GetTarget() => target;
        public string GetReason() => reason;

        // Design doc section 9.3 display format: dd/MM/yyyy HH:mm.
        public string GetActionDate() => actionDate.ToString("dd/MM/yyyy HH:mm");

        public override string ToString()
        {
            string adminName = admin != null ? admin.GetName() : "";
            return $"Admin Action | Type: {actionType} | Admin: {adminName} | Target: {target} | Reason: {reason} | Date: {GetActionDate()}";
        }
    }
}
