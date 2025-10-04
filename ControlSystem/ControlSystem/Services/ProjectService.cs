using ControlSystem.Models;
using ControlSystem.DTOs;
using YourProject.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace ControlSystem.Services
{
    public class ProjectService
    {
        private readonly ApplicationDbContext _db;

        public ProjectService(ApplicationDbContext db) => _db = db;

        public async Task<Project[]> GetAllAsync() =>
            await _db.Projects.Include(p => p.Stages).ToArrayAsync();

        public async Task<Project> GetByIdAsync(Guid id) =>
            await _db.Projects.Include(p => p.Stages).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Project> CreateAsync(CreateProjectDto dto)
        {
            var p = new Project { Name = dto.Name, Description = dto.Description };
            _db.Projects.Add(p);
            await _db.SaveChangesAsync();
            return p;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto)
        {
            var p = await _db.Projects.FindAsync(id);
            if (p == null) return false;
            p.Name = dto.Name ?? p.Name;
            p.Description = dto.Description ?? p.Description;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var p = await _db.Projects.FindAsync(id);
            if (p == null) return false;
            _db.Projects.Remove(p);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectStage> AddStageAsync(CreateStageDto dto)
        {
            var project = await _db.Projects.FindAsync(dto.ProjectId);
            if (project == null) return null;
            var st = new ProjectStage { Name = dto.Name, ProjectId = dto.ProjectId };
            _db.ProjectStages.Add(st);
            await _db.SaveChangesAsync();
            return st;
        }
    }
}
