using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IBulkUploadService
{
    /// <summary>kind = blank | valid | errors</summary>
    Task<byte[]> BuildTemplateAsync(int actorUserId, bool isCandidate, string kind);

    Task<UploadResultDto> ImportAsync(int actorUserId, bool isCandidate, string fileName, Stream file);

    Task<List<UploadBatchDto>> HistoryAsync(int actorUserId, bool isCandidate);
}

public class BulkUploadService : IBulkUploadService
{
    private static readonly string[] Headers =
        { "Candidate email", "Date (yyyy-MM-dd)", "Log in (HH:mm)", "Log out (HH:mm)", "Break minutes", "Task", "Description" };

    private const long MaxBytes = 5 * 1024 * 1024;
    private const int MaxRows = 1000;

    private readonly TmsDbContext _db;
    private readonly ITimesheetCore _core;
    private readonly IScopeService _scope;
    private readonly IAuditService _audit;

    public BulkUploadService(TmsDbContext db, ITimesheetCore core, IScopeService scope, IAuditService audit)
    {
        _db = db;
        _core = core;
        _scope = scope;
        _audit = audit;
    }

    // ---------- Templates ----------

    public async Task<byte[]> BuildTemplateAsync(int actorUserId, bool isCandidate, string kind)
    {
        // Sample rows use a real candidate email so the sample file imports cleanly.
        var sampleEmail = "candidate@example.com";
        if (isCandidate)
        {
            sampleEmail = (await _core.GetCandidateByUserAsync(actorUserId)).Email ?? sampleEmail;
        }
        else
        {
            var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
            var q = _db.CandidateOnboardings.Where(c => c.RequestType == "Candidate" &&
                                                        c.Status == OnboardingStatus.Approved && c.IsActive);
            if (allowed is not null) q = q.Where(c => allowed.Contains(c.CandidateId));
            sampleEmail = await q.OrderBy(c => c.CandidateId).Select(c => c.Email).FirstOrDefaultAsync() ?? sampleEmail;
        }

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Timesheet");
        for (var i = 0; i < Headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = Headers[i];
            cell.Style.Font.Bold = true;
        }

        var day1 = LastWeekday(1);
        var day2 = LastWeekday(2);
        if (kind == "valid")
        {
            AddRow(ws, 2, sampleEmail, day2, "09:00", "17:00", 30, "Feature development", "Built and tested the reporting screen for the client.");
            AddRow(ws, 3, sampleEmail, day1, "09:30", "17:30", 45, "Code review", "Reviewed pull requests and fixed the review comments.");
        }
        else if (kind == "errors")
        {
            AddRow(ws, 2, sampleEmail, day2, "17:00", "09:00", 30, "Bad times", "Log out is before log in, so this row fails.");
            AddRow(ws, 3, "not-an-email", day1, "09:00", "17:00", 30, "Bad email", "The email above is not a real candidate address.");
            AddRow(ws, 4, sampleEmail, "2999-01-01", "09:00", "17:00", 30, "Future date", "Future dates are never accepted for a timesheet.");
            AddRow(ws, 5, sampleEmail, day1, "09:00", "17:00", 30, "", "Task is missing on this row so validation fails.");
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string LastWeekday(int back)
    {
        var d = DateOnly.FromDateTime(DateTime.UtcNow);
        var found = 0;
        while (found < back)
        {
            d = d.AddDays(-1);
            if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) found++;
        }

        return d.ToString("yyyy-MM-dd");
    }

    private static void AddRow(IXLWorksheet ws, int row, string email, string date, string login, string logout, int brk, string task, string desc)
    {
        ws.Cell(row, 1).Value = email;
        ws.Cell(row, 2).SetValue(date);
        ws.Cell(row, 3).SetValue(login);
        ws.Cell(row, 4).SetValue(logout);
        ws.Cell(row, 5).Value = brk;
        ws.Cell(row, 6).Value = task;
        ws.Cell(row, 7).Value = desc;
    }

    // ---------- Import ----------

    private sealed record ParsedRow(int Number, string Email, string DateText, string In, string Out, string Break, string Task, string Desc);

    public async Task<UploadResultDto> ImportAsync(int actorUserId, bool isCandidate, string fileName, Stream file)
    {
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Please upload an Excel (.xlsx) file. Download a template to start.");

        var rows = ReadRows(file);
        if (rows.Count == 0) throw new BusinessRuleException("The file has no data rows.");
        if (rows.Count > MaxRows) throw new BusinessRuleException($"A file can hold at most {MaxRows} rows.");

        CandidateOnboarding? self = null;
        HashSet<int>? allowed = null;
        if (isCandidate) self = await _core.GetCandidateByUserAsync(actorUserId);
        else allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);

        var batch = new TimesheetUploadBatch
        {
            CandidateId = self?.CandidateId,
            UploadedByAppUserId = actorUserId,
            FileName = fileName.Length > 200 ? fileName[..200] : fileName,
            RowCountTotal = rows.Count,
            Status = "Processing",
            UploadedAt = DateTime.UtcNow,
        };
        _db.TimesheetUploadBatches.Add(batch);
        await _db.SaveChangesAsync();

        var result = new UploadResultDto { BatchId = batch.UploadBatchId, FileName = batch.FileName, Total = rows.Count };
        var approverId = isCandidate ? (int?)null : await _core.EnsureApproverAsync(actorUserId);
        _ = approverId;

        foreach (var r in rows)
        {
            var (candidate, error, dayId) = await ProcessRowAsync(r, actorUserId, self, allowed, batch.UploadBatchId);
            var uploadRow = new TimesheetUploadRow
            {
                UploadBatchId = batch.UploadBatchId,
                RowNumber = r.Number,
                CandidateEmail = Trunc(r.Email, 200),
                WorkDate = DateOnly.TryParse(r.DateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var wd) ? wd : null,
                LogInText = Trunc(r.In, 20),
                LogOutText = Trunc(r.Out, 20),
                BreakMinutes = int.TryParse(r.Break, out var bm) ? bm : null,
                Task = Trunc(r.Task, 80),
                Description = Trunc(r.Desc, 500),
                ValidationStatus = error is null ? "Imported" : "Error",
                ErrorMessage = error is null ? null : Trunc(error, 300),
                TimesheetDayId = dayId,
            };
            _db.TimesheetUploadRows.Add(uploadRow);

            result.Rows.Add(new UploadRowResultDto
            {
                Row = r.Number,
                Candidate = candidate is null ? r.Email : $"{candidate.FirstName} {candidate.LastName}",
                Date = r.DateText,
                Status = error is null ? "Valid" : "Error",
                Message = error,
            });
            if (error is null) result.Imported++; else result.Errors++;
        }

        batch.RowCountImported = result.Imported;
        batch.RowCountError = result.Errors;
        batch.Status = "Completed";
        result.Status = batch.Status;
        await _db.SaveChangesAsync();

        var actor = await _db.AppUsers.Where(u => u.AppUserId == actorUserId).Select(u => u.FullName).FirstAsync();
        var logId = await _audit.LogAsync(new AuditEntry(actorUserId, LogCategory.Submissions,
            result.Errors > 0 ? Severity.Warning : Severity.Success, "BulkUpload", "UploadBatch", batch.UploadBatchId, null,
            $"{actor} uploaded {batch.FileName}: {result.Imported} imported, {result.Errors} with errors",
            System.Text.Json.JsonSerializer.Serialize(new { summary = $"{result.Total} rows read" })));
        if (result.Imported > 0)
        {
            await _audit.NotifyPermissionHoldersAsync(PermissionCodes.ApproveTimesheets, self?.CandidateId,
                "Timesheets uploaded", $"{result.Imported} day(s) are waiting for approval.", "/timesheet-approvals");
        }

        _ = logId;
        return result;
    }

    private async Task<(CandidateOnboarding? Candidate, string? Error, long? DayId)> ProcessRowAsync(
        ParsedRow r, int actorUserId, CandidateOnboarding? self, HashSet<int>? allowed, int batchId)
    {
        CandidateOnboarding? candidate;
        if (self is not null)
        {
            if (!string.IsNullOrWhiteSpace(r.Email) &&
                !string.Equals(r.Email, self.Email, StringComparison.OrdinalIgnoreCase))
                return (self, "You can only upload your own hours; the email does not match your account.", null);
            candidate = self;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(r.Email)) return (null, "Candidate email is required.", null);
            candidate = await _db.CandidateOnboardings.Include(c => c.CandidateWorkSetting)
                .FirstOrDefaultAsync(c => c.Email == r.Email && c.RequestType == "Candidate" &&
                                          c.Status == OnboardingStatus.Approved && c.IsActive);
            if (candidate is null) return (null, "No active candidate found with this email.", null);
            if (allowed is not null && !allowed.Contains(candidate.CandidateId))
                return (candidate, "You do not have access to this candidate.", null);
        }

        if (!DateOnly.TryParse(r.DateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return (candidate, "Date is not valid. Use yyyy-MM-dd.", null);
        if (!TimeHelper.TryParseTime(r.In, out var tin)) return (candidate, "Log in must look like 09:00.", null);
        if (!TimeHelper.TryParseTime(r.Out, out var tout)) return (candidate, "Log out must look like 17:00.", null);
        if (!int.TryParse(string.IsNullOrWhiteSpace(r.Break) ? "0" : r.Break, out var brk) || brk is < 0 or > 600)
            return (candidate, "Break minutes must be a number from 0 to 600.", null);
        if (tout <= tin) return (candidate, "Log out must be after log in.", null);
        if (string.IsNullOrWhiteSpace(r.Task) || r.Task.Trim().Length > 80)
            return (candidate, "Task is required (up to 80 characters).", null);
        var dl = r.Desc.Trim().Length;
        if (dl is < 10 or > 500) return (candidate, "Description needs 10 to 500 characters.", null);

        var setting = await _core.GetSettingAsync(candidate.CandidateId);
        var zone = TimeHelper.GetZone(setting.TimeZoneId);
        try
        {
            await _core.ValidateWorkDateAsync(date, zone);
        }
        catch (BusinessRuleException ex)
        {
            return (candidate, ex.Message, null);
        }

        var day = await _db.TimesheetDays.Include(d => d.TimesheetStatusHistories)
            .FirstOrDefaultAsync(d => d.CandidateId == candidate.CandidateId && d.WorkDate == date);
        if (day is not null && day.Status is DayStatus.Pending or DayStatus.Approved)
            return (candidate, $"This day is already {day!.Status.ToLowerInvariant()} and cannot be replaced.", null);

        var inUtc = TimeHelper.ToUtc(date, tin, zone);
        var outUtc = TimeHelper.ToUtc(date, tout, zone);
        var minutes = Math.Max(0, (int)(outUtc - inUtc).TotalMinutes - brk);
        try
        {
            await _core.EnsureWeekCapAsync(candidate.CandidateId, TimeHelper.WeekStart(date), day?.TimesheetDayId ?? 0, minutes, setting);
        }
        catch (BusinessRuleException ex)
        {
            return (candidate, ex.Message, null);
        }

        if (day is null)
        {
            day = new TimesheetDay
            {
                CandidateId = candidate.CandidateId,
                WorkDate = date,
                WeekStartDate = TimeHelper.WeekStart(date),
                EntrySource = "Upload",
                Status = string.Empty,
                CreatedAt = DateTime.UtcNow,
            };
            _db.TimesheetDays.Add(day);
        }

        day.EntrySource = "Upload";
        day.UploadBatchId = batchId;
        day.Task = r.Task.Trim();
        day.Description = r.Desc.Trim();
        day.RejectionReason = null;
        _core.ApplyTimes(day, inUtc, outUtc, brk);
        await _core.EvaluateFlagsAsync(day, setting);
        day.SubmittedAt = DateTime.UtcNow;
        _core.ChangeStatus(day, DayStatus.Pending, self?.CandidateId, actorUserId, "Uploaded from Excel");
        await _db.SaveChangesAsync();
        return (candidate, null, day.TimesheetDayId);
    }

    private static List<ParsedRow> ReadRows(Stream file)
    {
        if (file.Length > MaxBytes) throw new BusinessRuleException("The file is larger than 5 MB.");

        try
        {
            using var wb = new XLWorkbook(file);
            var ws = wb.Worksheets.First();
            var used = ws.LastRowUsed()?.RowNumber() ?? 1;
            var list = new List<ParsedRow>();
            for (var row = 2; row <= used; row++)
            {
                string Text(int col) => ws.Cell(row, col).GetFormattedString().Trim();
                var parsed = new ParsedRow(row, Text(1), DateText(ws.Cell(row, 2)), TimeText(ws.Cell(row, 3)),
                    TimeText(ws.Cell(row, 4)), Text(5), Text(6), Text(7));
                if (string.IsNullOrWhiteSpace(parsed.Email + parsed.DateText + parsed.In + parsed.Out + parsed.Task + parsed.Desc)) continue;
                list.Add(parsed);
            }

            return list;
        }
        catch (Exception ex) when (ex is not BusinessRuleException)
        {
            throw new BusinessRuleException("That file could not be read. Please use one of the templates.");
        }
    }

    private static string DateText(IXLCell c) =>
        c.DataType == XLDataType.DateTime ? c.GetDateTime().ToString("yyyy-MM-dd") : c.GetString().Trim();

    private static string TimeText(IXLCell c)
    {
        if (c.DataType == XLDataType.TimeSpan) return c.GetTimeSpan().ToString(@"hh\:mm");
        if (c.DataType == XLDataType.DateTime) return c.GetDateTime().ToString("HH:mm");
        if (c.DataType == XLDataType.Number && c.TryGetValue<double>(out var d) && d is >= 0 and < 1)
            return TimeSpan.FromDays(d).ToString(@"hh\:mm");
        return c.GetString().Trim();
    }

    private static string? Trunc(string? v, int max) => v is null ? null : v.Length <= max ? v : v[..max];

    // ---------- History ----------

    public async Task<List<UploadBatchDto>> HistoryAsync(int actorUserId, bool isCandidate)
    {
        var q = _db.TimesheetUploadBatches.AsQueryable();
        q = isCandidate
            ? q.Where(b => b.UploadedByAppUserId == actorUserId)
            : q.Where(b => b.UploadedByAppUserId != null);

        var batches = await q.OrderByDescending(b => b.UploadedAt).Take(30).ToListAsync();
        var userIds = batches.Select(b => b.UploadedByAppUserId).Distinct().ToList();
        var names = await _db.AppUsers.Where(u => userIds.Contains(u.AppUserId))
            .ToDictionaryAsync(u => u.AppUserId, u => u.FullName);

        return batches.Select(b => new UploadBatchDto
        {
            BatchId = b.UploadBatchId,
            FileName = b.FileName,
            UploadedBy = b.UploadedByAppUserId is int id && names.TryGetValue(id, out var n) ? n : string.Empty,
            UploadedAt = DateTime.SpecifyKind(b.UploadedAt, DateTimeKind.Utc).ToString("o"),
            Total = b.RowCountTotal,
            Imported = b.RowCountImported,
            Errors = b.RowCountError,
            Status = b.Status,
        }).ToList();
    }
}
