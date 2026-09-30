<div dir="rtl"> <p align="center"> <img src="https://img.shields.io/badge/C%23-.NET-512BD4?style=for-the-badge&logo=csharp" alt=".NET" /> <img src="https://img.shields.io/badge/Type-Console%20App-2E7D32?style=for-the-badge" alt="Console App" /> <img src="https://img.shields.io/badge/Status-In%20Development-orange?style=for-the-badge" alt="In Development" /> </p> <h1 align="center">📌 JobSystem</h1> <p align="center"><i>לוח דרושים דיגיטלי — ממועמד ראשון ועד קבלה לעבודה</i></p>
למה הפרויקט הזה קיים?

מועמדים מחפשים עבודה, מעסיקים מחפשים עובדים — ובאמצע יש הרבה בלגן של קורות חיים ואקסלים. JobSystem היא אפליקציית קונסולה ב-C# שמדמה תהליך גיוס שלם: מועמד נרשם, בונה פרופיל, מחפש משרה ושולח מועמדות; מעסיק מפרסם משרות, סוקר מועמדים ומעדכן סטטוס; מנהל מערכת משגיח על הכול.

הפרויקט נבנה כעבודת צוות עם ניהול משימות ב-Jira, ותהליך Git מסודר של branch לכל משימה ו-Pull Request לפני כל מיזוג.

🗂️ ניווט מהיר
קישור	תוכן
🧑‍🤝‍🧑 מי בצוות	חברי הצוות והאחריות שלהם
🧩 מודל הנתונים	הישויות והקשרים ביניהן
⚙️ מה כבר עובד	טבלת יכולות לפי סוג משתמש
🔀 זרימת פיתוח	מ-Jira ועד master
📐 מוסכמות	שם branch, ניסוח commit, סגנון קוד
🔗 קישורים	Jira / Confluence
▶️ הרצה	הפעלת הפרויקט מקומית
🛣️ מה הלאה	רעיונות להמשך
🧑‍🤝‍🧑 מי בצוות
חבר צוות	תפקיד בפרויקט
Shai	הרשמה, התחברות, הגשת מועמדות, עדכון פרופיל, התנתקות
Zohar Glick	ניהול משרות, ניהול מועמדים, צפייה במשרה, משרות המעסיק
הוסיפו כאן חברי צוות נוספים ותחומי אחריות בפועל	

✏️ טבלה זו מבוססת על היסטוריית ה-Commits בפועל. כדאי לעדכן אותה ידנית אם מישהו הצטרף או התפקידים השתנו.

🧩 מודל הנתונים

חמש מחלקות מרכזיות תחת Models/, וביניהן הקשרים הבאים:

יש למועמד
מעסיק מנהל
מפרסמת
מגיש
מתקבלות עבורה
1
1
1
1
1
0..1
0..*
0..*
0..*
0..*
User
+int Id
+string Name
+string Email
-string Password
+string UserType «CANDIDATE / EMPLOYER / ADMIN»
+bool Suspended
CandidateProfile
+string Education
+string Experience
+string Skills
+string Summary
Company
+int Id
+string Name
+string Description
Job
+int Id
+string Title
+string Field
+string Location
+string JobType
+string Status «OPEN / CLOSED / REMOVED»
Application
+int Id
+string ApplicationDate
+string Status «NEW → UNDER_REVIEW → INTERVIEW → ACCEPTED/REJECTED»

החלטת עיצוב מרכזית: Program.cs הוא שכבת תצוגה בלבד (מדפיס תפריטים וקורא קלט) — כל הלוגיקה, ההרשאות והשמירה על עקביות הנתונים מרוכזות ב-JobSystem.cs. שכבת ה-Models לא יודעת כלום על תהליכים עסקיים, רק מחזיקה נתונים.

⚙️ מה כבר עובד
#	יכולת	REQ	KAN	מועמד	מעסיק	מנהל
1	הרשמה למערכת	REQ-001	KAN-5	✅	✅	—
2	התחברות	REQ-002	KAN-6	✅	✅	✅
3	חיפוש משרות פתוחות	REQ-003	KAN-7	✅	—	—
4	הגשת מועמדות	REQ-004	KAN-8	✅	—	—
5	פרסום / עריכה / סגירת משרה	REQ-005	KAN-9	—	✅	—
6	ניהול וסינון מועמדים למשרה	REQ-006	KAN-10	—	✅	—
7	עדכון פרופיל מועמד (השכלה, ניסיון, כישורים)	REQ-009	KAN-13	✅	—	—
8	צפייה בפרטי משרה מלאים	REQ-010	KAN-14	✅	✅	—
9	צפייה במשרות שפרסם המעסיק	REQ-012	KAN-16	—	✅	—
10	התנתקות מהמערכת	REQ-013	KAN-17	✅	✅	✅

קצב התקדמות: ██████████ 10/10 דרישות הליבה ממומשות בענף master נכון לכתיבת שורות אלה — הטבלה נשארת פה כתיעוד חי, יש לעדכן כשנוספות דרישות.

🔀 זרימת פיתוח
לא
כן
Story ב-JiraKAN-N
git switch mastergit pull
git switch -cKAN-N-shem-katzar
כתיבת הקודהתאמה ל-Story
git commit -m'KAN-N ...'
git push
Pull Requestל-master
Code Reviewעבר?
Merge ל-master
Jira → Done

שם ה-branch תמיד KAN-<מספר>-<תיאור-קצר-באנגלית> (למשל KAN-8-submit-application) — כך שם הענף לבדו מספיק כדי לדעת על איזה Story עובדים, בלי לחפש מיפוי בקובץ נפרד.

📐 מוסכמות קוד וקומיטים
הודעת Commit נפתחת תמיד במפתח ה-Jira: KAN-8 Implement submitApplication validation.
קובץ משותף — JobSystem.cs: כמה Stories נוגעים באותו קובץ מרכזי. לפני שמוסיפים שדה/מתודה, כדאי לבדוק אם מישהו אחר עובד שם באותו הרגע כדי למנוע Merge Conflict מיותר.
בלי לוגיקה ב-Program.cs: קובץ זה רק קורא קלט, מציג פלט וקורא למתודות של JobSystem. ולידציה ושינוי נתונים שייכים אך ורק ל-JobSystem.
בדיקה ידנית לפני PR: מריצים את התרחיש שממומש (הצלחה + כשל אחד לפחות) ומוודאים שאין קריסה על קלט לא תקין.
.gitignore: לוודא שתיקיות bin/ ו-obj/ לא עולות ל-Repository.
🔗 קישורים חיצוניים
משאב	קישור	שימוש
📋 Jira Board	להשלים	מעקב אחרי כל ה-Stories וה-Subtasks
📚 Confluence	להשלים	מסמך דרישות (REQ-001 עד REQ-013+), עיצוב טכני והחלטות צוות
🐙 GitHub Repo	pmhermelin/job-app	קוד המקור וה-Pull Requests

📌 מומלץ להדביק כאן את קישורי ה-Jira וה-Confluence האמיתיים של הצוות ברגע שהם קיימים, כדי שה-README ישמש נקודת כניסה יחידה לכל חומרי הפרויקט.

▶️ איך מריצים
bash
git clone https://github.com/pmhermelin/job-app.git
cd job-app
dotnet run --project JobApp.csproj

דרוש .NET SDK מותקן מקומית. בהפעלה נטענים אוטומטית חשבון מנהל ומשרות דמו, כדי שאפשר יהיה לבדוק מיד הגשת מועמדות בלי להגדיר נתונים ידנית.

🛣️ מה הלאה

רעיונות שלא חלק מהדרישות הנוכחיות אך יכולים לשדרג את הפרויקט בהמשך:

שמירת נתונים לקובץ/DB במקום זיכרון בלבד (כרגע הכול נמחק בסגירת התוכנית).
בדיקות יחידה (Unit Tests) למתודות הוולידציה ב-JobSystem.
ממשק גרפי (Web/Desktop) מעל אותה שכבת לוגיקה קיימת.
<p align="center"><sub>נבנה כפרויקט לימודי — עדכנו קטע זה בהתאם למוסד/קורס שלכם.</sub></p> </div>
