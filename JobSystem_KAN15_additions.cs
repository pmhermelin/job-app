// ============================================================
// KAN-15 (REQ-011) — דיווח על משרה
// קטע קוד למיזוג ידני לתוך JobSystem.cs הקיים.
// לפי מסמך העיצוב הטכני, סעיף 6.13.
// ============================================================
//
// חשוב: Story הזו יוצרת דיווחים (Report) בפועל. בלעדיה KAN-11
// (RemoveJob/HandleReport) לא ניתנת לבדיקה עם נתונים אמיתיים -
// רק עם נתוני seed ידניים. מומלץ למזג את KAN-15 בהקדם.
//
// הנחות (לוודא מול הקוד האמיתי):
// - JobSystem מכילה: Report[] reports, int reportCount, int nextReportId.
//   (אם עדיין לא נוספו - יש להוסיף אותם, ראו JobSystem_KAN11_additions.cs).
// - Job תומכת ב-GetStatus() עם הערך "REMOVED".

/// <summary>
/// REQ-011 — מחפש עבודה מדווח על משרה קיימת (שאינה REMOVED),
/// עם סיבה, ותוך בדיקת קיבולת. הדיווח נשמר בסטטוס NEW.
/// </summary>
public bool CreateReport(User candidate, int jobId, string reason)
{
    // 1. בדיקת הרשאה
    if (candidate == null || candidate.GetUserType() != "CANDIDATE")
    {
        Console.WriteLine("Error: only a candidate can report a job.");
        return false;
    }

    if (candidate.IsSuspended())
    {
        Console.WriteLine("Error: suspended users cannot report jobs.");
        return false;
    }

    // 2. איתור המשרה, ואי-אפשרות דיווח על משרה שהוסרה
    Job job = FindJobById(jobId);
    if (job == null)
    {
        Console.WriteLine("Error: job not found.");
        return false;
    }

    if (job.GetStatus() == "REMOVED")
    {
        Console.WriteLine("Error: this job has already been removed.");
        return false;
    }

    // 3. סיבה תקינה
    if (IsBlank(reason))
    {
        Console.WriteLine("Error: a reason is required.");
        return false;
    }

    // 4. בדיקת קיבולת
    if (reportCount >= reports.Length)
    {
        Console.WriteLine("Error: report capacity reached.");
        return false;
    }

    // 5. יצירת הדיווח
    Report report = new Report(nextReportId, candidate, job, reason);
    reports[reportCount] = report;
    reportCount++;
    nextReportId++;

    Console.WriteLine($"Report #{report.GetId()} submitted successfully.");
    return true;
}
