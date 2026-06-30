using Enterprise.Domain.Entities;

namespace Enterprise.Application.Common.Interfaces;

public interface IWorkspaceRepository
{
    Task<List<Workspace>> GetAllAsync();

    Task<Workspace?> GetByIdAsync(Guid id);

    Task AddAsync(Workspace workspace);

    Task DeleteAsync(Guid id);
}