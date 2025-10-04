using ControlSystem.DTOs;
using ControlSystem.Models;
using YourProject.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace ControlSystem.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _db;

        public ReportService(ApplicationDbContext db) => _db = db;

        // Экспорт дефектов в CSV по фильтру
        public async Task<MemoryStream> ExportDefectsToCsvAsync(DefectFilterDto filter)
        {
            var q = _db.Defects
                .Include(d => d.Project)
                .Include(d => d.AssignedTo)
                .AsQueryable();

            if (filter.ProjectId.HasValue) q = q.Where(d => d.ProjectId == filter.ProjectId.Value);
            if (filter.Status.HasValue) q = q.Where(d => d.Status == filter.Status.Value);
            if (filter.Priority.HasValue) q = q.Where(d => d.Priority == filter.Priority.Value);
            if (!string.IsNullOrEmpty(filter.AssignedToId)) q = q.Where(d => d.AssignedToId == filter.AssignedToId);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                q = q.Where(d => d.Title.ToLower().Contains(s) || d.Description.ToLower().Contains(s));
            }

            var items = await q.OrderByDescending(d => d.CreatedAt).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id;Title;Project;Status;Priority;AssignedTo;CreatedAt;DueDate;Description");
            foreach (var d in items)
            {
                var line = string.Join(';', new[]
                {
                    d.Id.ToString(),
                    EscapeCsv(d.Title),
                    EscapeCsv(d.Project?.Name ?? ""),
                    d.Status.ToString(),
                    d.Priority.ToString(),
                    d.AssignedTo?.Email ?? d.AssignedToId ?? "",
                    d.CreatedAt.ToString("o"),
                    d.DueDate?.ToString("o") ?? "",
                    EscapeCsv(d.Description ?? "")
                });
                sb.AppendLine(line);
            }

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            ms.Position = 0;
            return ms;
        }

        private static string EscapeCsv(string input)
        {
            if (input == null) return "";
            // простая CSV-экранировка для точек с запятой
            if (input.Contains(';') || input.Contains('"') || input.Contains('\n'))
            {
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            }
            return input;
        }

        // Аналитика: количество по статусам
        public async Task<Dictionary<string, int>> GetCountsByStatusAsync()
        {
            var groups = await _db.Defects.GroupBy(d => d.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            return groups.ToDictionary(g => g.Status.ToString(), g => g.Count);
        }

        // Аналитика: количество по приоритетам
        public async Task<Dictionary<string, int>> GetCountsByPriorityAsync()
        {
            var groups = await _db.Defects.GroupBy(d => d.Priority).Select(g => new { Priority = g.Key, Count = g.Count() }).ToListAsync();
            return groups.ToDictionary(g => g.Priority.ToString(), g => g.Count);
        }

        // Тренд: дефекты по дням за период
        public async Task<Dictionary<string, int>> GetCreatedTrendAsync(DateTime from, DateTime to)
        {
            var q = await _db.Defects
                .Where(d => d.CreatedAt >= from && d.CreatedAt <= to)
                .GroupBy(d => d.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return q.OrderBy(x => x.Date).ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);
        }
    }
}
