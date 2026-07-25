using MaisonAeternum.Domain.Entities;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IModuleRepository : IRepository<Module>
{
    Task<Module?> GetWithFormationAsync(int moduleId, CancellationToken cancellationToken = default);

    Task<List<Module>> GetByFormationIdAsync(int formationId, CancellationToken cancellationToken = default);
}
