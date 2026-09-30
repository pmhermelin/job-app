// ============================================================
// KAN-11 (REQ-007) — הסרת משרה וטיפול בדיווחים
// קטע קוד למיזוג ידני לתוך JobSystem.cs הקיים.
// לפי מסמך העיצוב הטכני, סעיפים 5.8, 6.0, 6.9, 6.17, 7.
// ============================================================
//
// הנחות שנעשו (יש לוודא/להתאים מול הקוד הקיים בפועל):
// 1. קיימים כבר בתוך JobSystem השדות: Job[] jobs, int jobCount.
// 2. קיים nextJobId, nextReportId, nextApplicationId וכו' לפי סעיף 5.8.
// 3. יש להוסיף (אם עוד לא קיימים, כי KAN-15/יצירת דיווח לא מומשה עדיין):
//      private Report[] reports = new Report[100];
//      private int reportCount = 0;
//      private int nextReportId = 1;
//      private AdminAction[] adminActions = new AdminAction[300];
//      private int adminActionCount = 0;
// 4. Job כבר תומכת ב-GetStatus()/SetStatus(string) עם ערכים OPEN/CLOSED/REMOVED.
// 5. User כבר תומכת ב-GetUserType() המחזירה "ADMIN"/"EMPLOYER"/"CANDIDATE".
//
// אם שמות המתודות/שדות הקיימים שונים (למשל GetStatus() לעומת getStatus()),
// יש להתאים את הקוד הזה לפני שמדביקים אותו.

// ---------- הוספה לתוך המחלקה JobSystem ----------

/// <summary>
/// REQ-007 — מנהל מסיר משרה (מחיקה לוגית), מתעד AdminAction,
/// ומסמן כ-HANDLED את כל הדיווחים הפתוחים על אותה משרה.
/// </summary>
public bool RemoveJob(User admin, int jobId, string reason)
{
    // 1. בדיקת הרשאה
    if (admin == null || admin.GetUserType() != "ADMIN")
    {
        Console.WriteLine("Error: only an admin can remove a job.");
        return false;
    }

    // 2. איתור המשרה
    Job job = FindJobById(jobId);
    if (job == null)
    {
        Console.WriteLine("Error: job not found.");
        return false;
    }

    if (job.GetStatus() == "REMOVED")
    {
        Console.WriteLine("Error: job is already removed.");
        return false;
    }

    // 3. הצגת הדיווחים המקושרים למשרה, לפני ביצוע הפעולה
    PrintReportsForJob(job);

    // 4. קליטת סיבה תקינה
    if (IsBlank(reason))
    {
        Console.WriteLine("Error: a reason is required to remove a job.");
        return false;
    }

    // 5. שינוי סטטוס - מחיקה לוגית
    job.SetStatus("REMOVED");

    // 6. תיעוד AdminAction
    AddAdminAction(admin, "REMOVE_JOB", $"Job #{job.GetId()} - {job.GetTitle()}", reason);

    // 7. סימון כל הדיווחים הפתוחים (NEW) על המשרה הזו כ-HANDLED
    for (int i = 0; i < reportCount; i++)
    {
        if (reports[i].GetJob() == job && reports[i].GetStatus() == "NEW")
        {
            reports[i].MarkHandled();
        }
    }

    Console.WriteLine($"Job #{job.GetId()} removed successfully.");
    return true;
}

/// <summary>
/// REQ-007 + REQ-011 — מנהל מטפל בדיווח NEW קיים: מציג את פרטי הדיווח
/// והמשרה, ובוחר בין סימון "טופל" בלבד לבין הסרת המשרה.
/// </summary>
public bool HandleReport(User admin, int reportId, bool removeJobToo, string reason)
{
    // 1. בדיקת הרשאה
    if (admin == null || admin.GetUserType() != "ADMIN")
    {
        Console.WriteLine("Error: only an admin can handle reports.");
        return false;
    }

    // 2. איתור הדיווח
    Report report = FindReportById(reportId);
    if (report == null)
    {
        Console.WriteLine("Error: report not found.");
        return false;
    }

    if (report.GetStatus() != "NEW")
    {
        Console.WriteLine("Error: this report was already handled.");
        return false;
    }

    // 3. הצגת פרטי הדיווח והמשרה
    Console.WriteLine(report.ToString());
    Console.WriteLine(report.GetJob().ToString());

    // 4. סימון הדיווח כטופל
    report.MarkHandled();

    // 5. תיעוד AdminAction עבור טיפול בדיווח
    AddAdminAction(admin, "HANDLE_REPORT",
        $"Report #{report.GetId()} on Job #{report.GetJob().GetId()}", reason);

    // 6. אם המנהל בחר גם להסיר את המשרה - קריאה ל-RemoveJob
    //    (יוצרת AdminAction נוסף מסוג REMOVE_JOB, כנדרש בסעיף 6.17)
    if (removeJobToo && report.GetJob().GetStatus() != "REMOVED")
    {
        RemoveJob(admin, report.GetJob().GetId(), reason);
    }

    Console.WriteLine($"Report #{report.GetId()} marked as handled.");
    return true;
}

// ---------- מתודות עזר נדרשות (סעיף 7 במסמך) ----------

private Report FindReportById(int id)
{
    for (int i = 0; i < reportCount; i++)
    {
        if (reports[i].GetId() == id)
        {
            return reports[i];
        }
    }
    return null;
}

private void PrintReportsForJob(Job job)
{
    bool found = false;
    for (int i = 0; i < reportCount; i++)
    {
        if (reports[i].GetJob() == job)
        {
            Console.WriteLine(reports[i].ToString());
            found = true;
        }
    }
    if (!found)
    {
        Console.WriteLine("No reports linked to this job.");
    }
}

private void AddAdminAction(User admin, string actionType, string target, string reason)
{
    if (adminActionCount >= adminActions.Length)
    {
        Console.WriteLine("Warning: admin action log is full; action was not logged.");
        return;
    }
    adminActions[adminActionCount] = new AdminAction(admin, actionType, target, reason);
    adminActionCount++;
}

private bool IsBlank(string value)
{
    return string.IsNullOrWhiteSpace(value);
}

// הערה: FindJobById() כבר אמור להיות קיים ב-JobSystem (נעשה בו שימוש
// ב-KAN-8/submitApplication). אם הוא לא קיים, יש להוסיף:
//
// private Job FindJobById(int id)
// {
//     for (int i = 0; i < jobCount; i++)
//     {
//         if (jobs[i].GetId() == id) return jobs[i];
//     }
//     return null;
// }
