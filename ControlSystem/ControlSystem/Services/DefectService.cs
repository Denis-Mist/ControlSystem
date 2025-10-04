using ControlSystem.DTOs;
using ControlSystem.Models;
using YourProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using YourProject.Models;

namespace ControlSystem.Services
{
    public class DefectService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DefectService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<Defect> CreateAsync(CreateDefectDto dto, string createdById)
        {
            var d = new Defect
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                ProjectId = dto.ProjectId,
                StageId = dto.StageId,
                AssignedToId = dto.AssignedToId,
                DueDate = dto.DueDate
            };

            _db.Defects.Add(d);
            await _db.SaveChangesAsync();

            await LogHistoryAsync(d.Id, createdById, "Created", null, $"Title:{d.Title}");
            return d;
        }

        public async Task<Defect> GetByIdAsync(Guid id)
        {
            return await _db.Defects
                .Include(x => x.Attachments)
                .Include(x => x.Comments).ThenInclude(c => c.Author)
                .Include(x => x.History).ThenInclude(h => h.ChangedBy)
                .Include(x => x.Project)
                .Include(x => x.Stage)
                .Include(x => x.AssignedTo)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<(IEnumerable<Defect> Items, int Total)> QueryAsync(DefectFilterDto filter)
        {
            var q = _db.Defects
                .Include(d => d.AssignedTo)
                .Include(d => d.Project)
                .Include(d => d.Stage)
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

            // Sorting (basic)
            bool desc = filter.SortDir?.ToLower() == "desc";
            q = filter.SortBy?.ToLower() switch
            {
                "priority" => desc ? q.OrderByDescending(d => d.Priority) : q.OrderBy(d => d.Priority),
                "duedate" => desc ? q.OrderByDescending(d => d.DueDate) : q.OrderBy(d => d.DueDate),
                "status" => desc ? q.OrderByDescending(d => d.Status) : q.OrderBy(d => d.Status),
                _ => desc ? q.OrderByDescending(d => d.CreatedAt) : q.OrderBy(d => d.CreatedAt)
            };

            var total = await q.CountAsync();
            var items = await q.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToArrayAsync();
            return (items, total);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateDefectDto dto, string userId)
        {
            var d = await _db.Defects.FindAsync(id);
            if (d == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != d.Title)
            {
                await LogHistoryAsync(d.Id, userId, "Title", d.Title, dto.Title);
                d.Title = dto.Title;
            }

            if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description != d.Description)
            {
                await LogHistoryAsync(d.Id, userId, "Description", d.Description, dto.Description);
                d.Description = dto.Description;
            }

            if (dto.Priority.HasValue && dto.Priority.Value != d.Priority)
            {
                await LogHistoryAsync(d.Id, userId, "Priority", d.Priority.ToString(), dto.Priority.Value.ToString());
                d.Priority = dto.Priority.Value;
            }

            if (dto.StageId.HasValue && dto.StageId != d.StageId)
            {
                await LogHistoryAsync(d.Id, userId, "StageId", d.StageId?.ToString(), dto.StageId.ToString());
                d.StageId = dto.StageId;
            }

            if (!string.IsNullOrWhiteSpace(dto.AssignedToId) && dto.AssignedToId != d.AssignedToId)
            {
                await LogHistoryAsync(d.Id, userId, "AssignedTo", d.AssignedToId, dto.AssignedToId);
                d.AssignedToId = dto.AssignedToId;
            }

            if (dto.DueDate != d.DueDate)
            {
                await LogHistoryAsync(d.Id, userId, "DueDate", d.DueDate?.ToString("o"), dto.DueDate?.ToString("o"));
                d.DueDate = dto.DueDate;
            }

            await _db.SaveChangesAsync();
            return true;
        }

        private static readonly Dictionary<DefectStatus, DefectStatus[]> AllowedTransitions = new()
        {
            { DefectStatus.New, new[]{ DefectStatus.InProgress, DefectStatus.Cancelled } },
            { DefectStatus.InProgress, new[]{ DefectStatus.UnderReview, DefectStatus.Cancelled } },
            { DefectStatus.UnderReview, new[]{ DefectStatus.Closed, DefectStatus.InProgress, DefectStatus.Cancelled } },
            { DefectStatus.Closed, Array.Empty<DefectStatus>() },
            { DefectStatus.Cancelled, Array.Empty<DefectStatus>() }
        };

        public async Task<(bool Ok, string Error)> ChangeStatusAsync(Guid id, DefectStatus newStatus, string userId)
        {
            var d = await _db.Defects.FindAsync(id);
            if (d == null) return (false, "Defect not found");

            if (d.Status == newStatus) return (true, null);

            if (!AllowedTransitions.TryGetValue(d.Status, out var allowed)
                || !allowed.Contains(newStatus))
            {
                return (false, $"Transition from {d.Status} to {newStatus} is not allowed.");
            }

            var old = d.Status;
            d.Status = newStatus;
            await LogHistoryAsync(d.Id, userId, "Status", old.ToString(), newStatus.ToString());
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<DefectComment> AddCommentAsync(Guid defectId, string text, string authorId)
        {
            var d = await _db.Defects.FindAsync(defectId);
            if (d == null) return null;
            var c = new DefectComment { DefectId = defectId, Text = text, AuthorId = authorId };
            _db.DefectComments.Add(c);
            await _db.SaveChangesAsync();
            await LogHistoryAsync(defectId, authorId, "Comment", null, text);
            return c;
        }

        public async Task<DefectAttachment> AttachFileAsync(Guid defectId, IFormFile file, string uploaderId)
        {
            var d = await _db.Defects.FindAsync(defectId);
            if (d == null) return null;

            using var ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms);
            var att = new DefectAttachment
            {
                DefectId = defectId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = ms.ToArray(),
                UploadedById = uploaderId
            };
            _db.DefectAttachments.Add(att);
            await _db.SaveChangesAsync();
            await LogHistoryAsync(defectId, uploaderId, "Attachment", null, att.FileName);
            return att;
        }

        private async Task LogHistoryAsync(Guid defectId, string userId, string field, string oldValue, string newValue)
        {
            var h = new DefectHistory
            {
                DefectId = defectId,
                ChangedById = userId,
                Field = field,
                OldValue = oldValue,
                NewValue = newValue
            };
            _db.DefectHistories.Add(h);
            await _db.SaveChangesAsync();
        }
    }
}
