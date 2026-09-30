using System;

/// <summary>
/// Audit log record of an action performed by a system admin.
/// Design doc reference: section 5.7. Not editable after creation.
/// </summary>
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
        this.actionDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }

    public User GetAdmin() => admin;
    public string GetActionType() => actionType;
    public string GetTarget() => target;
    public string GetReason() => reason;
    public string GetActionDate() => actionDate;

    public override string ToString()
    {
        return $"Admin Action | Type: {actionType}\n" +
               $"Admin: {admin.GetName()} | Target: {target}\n" +
               $"Reason: {reason}\n" +
               $"Date: {actionDate}";
    }
}
