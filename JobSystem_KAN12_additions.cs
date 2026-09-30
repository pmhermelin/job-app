// ============================================================
// KAN-12 (REQ-008) — השעיה וביטול השעיית משתמש
// קטע קוד למיזוג ידני לתוך JobSystem.cs הקיים.
// לפי מסמך העיצוב הטכני, סעיף 6.10.
// ============================================================
//
// הנחות (לוודא מול הקוד האמיתי):
// - User תומכת ב-GetUserType(), IsSuspended(), SetSuspended(bool), GetId(), GetEmail(), GetName().
// - JobSystem כבר מכילה users[] / userCount ו-AddAdminAction() (נוספה ב-KAN-11).
// - אם AddAdminAction() עדיין לא קיימת (טרם מיזגת את KAN-11), יש להוסיף אותה קודם.

/// <summary>
/// REQ-008 — מנהל משעה משתמש (חוץ ממנהל מערכת), ומתעד AdminAction.
/// </summary>
public bool SuspendUser(User admin, int targetUserId, string reason)
{
    if (admin == null || admin.GetUserType() != "ADMIN")
    {
        Console.WriteLine("Error: only an admin can suspend a user.");
        return false;
    }

    User target = FindUserById(targetUserId);
    if (target == null)
    {
        Console.WriteLine("Error: user not found.");
        return false;
    }

    if (target.GetUserType() == "ADMIN")
    {
        Console.WriteLine("Error: an admin account cannot be suspended.");
        return false;
    }

    if (target.IsSuspended())
    {
        Console.WriteLine("Error: user is already suspended.");
        return false;
    }

    if (IsBlank(reason))
    {
        Console.WriteLine("Error: a reason is required.");
        return false;
    }

    target.SetSuspended(true);
    AddAdminAction(admin, "SUSPEND", $"User #{target.GetId()} - {target.GetName()}", reason);

    Console.WriteLine($"User #{target.GetId()} suspended successfully.");
    return true;
}

/// <summary>
/// REQ-008 — מנהל מבטל השעיה של משתמש, ומתעד AdminAction.
/// </summary>
public bool UnsuspendUser(User admin, int targetUserId, string reason)
{
    if (admin == null || admin.GetUserType() != "ADMIN")
    {
        Console.WriteLine("Error: only an admin can unsuspend a user.");
        return false;
    }

    User target = FindUserById(targetUserId);
    if (target == null)
    {
        Console.WriteLine("Error: user not found.");
        return false;
    }

    if (!target.IsSuspended())
    {
        Console.WriteLine("Error: user is not suspended.");
        return false;
    }

    if (IsBlank(reason))
    {
        Console.WriteLine("Error: a reason is required.");
        return false;
    }

    target.SetSuspended(false);
    AddAdminAction(admin, "UNSUSPEND", $"User #{target.GetId()} - {target.GetName()}", reason);

    Console.WriteLine($"User #{target.GetId()} unsuspended successfully.");
    return true;
}

// אם FindUserById() עדיין לא קיימת ב-JobSystem, יש להוסיף:
//
// private User FindUserById(int id)
// {
//     for (int i = 0; i < userCount; i++)
//     {
//         if (users[i].GetId() == id) return users[i];
//     }
//     return null;
// }
