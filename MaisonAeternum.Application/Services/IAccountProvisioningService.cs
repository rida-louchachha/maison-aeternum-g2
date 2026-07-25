namespace MaisonAeternum.Application.Services;

public interface IAccountProvisioningService
{
    Task CreateLearnerProfileAsync(string userId, CancellationToken cancellationToken = default);
}
