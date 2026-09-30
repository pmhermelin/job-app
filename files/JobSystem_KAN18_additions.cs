// ============================================================
// KAN-18 (REQ-014) — צפייה ביומן פעולות מנהל
// קטע קוד למיזוג ידני לתוך JobSystem.cs הקיים.
// לפי מסמך העיצוב הטכני, סעיף 6.16.
// ============================================================
//
// הנחות (לוודא מול הקוד האמיתי):
// - JobSystem מכילה: AdminAction[] adminActions, int adminActionCount
//   (נוספו כבר ב-KAN-11 / KAN-12, אם אחת מהן כבר מוזגה).

/// <summary>
/// REQ-014 — מנהל צופה ביומן פעולות המנהל: מציג מנהל, סוג פעולה,
/// יעד, סיבה ותאריך לכל רשומה. אין מתודה למחיקת רשומות.
/// </summary>
public void PrintAdminLog(User admin)
{
    if (admin == null || admin.GetUserType() != "ADMIN")
    {
        Console.WriteLine("Error: only an admin can view the admin log.");
        return;
    }

    if (adminActionCount == 0)
    {
        Console.WriteLine("The admin log is empty.");
        return;
    }

    for (int i = 0; i < adminActionCount; i++)
    {
        Console.WriteLine(adminActions[i].ToString());
        Console.WriteLine("--------------------");
    }
}
