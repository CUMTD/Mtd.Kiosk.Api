namespace Mtd.Kiosk.Core.Repositories
{
	public interface IKioskRepository : IRepository<Entities.Kiosk>
	{

		Task<Entities.Kiosk> GetByIdentityWithTicketsAsync(string identity, CancellationToken cancellationToken);
		Task<List<Entities.Kiosk>> GetAllAsync(CancellationToken cancellationToken);

	}
}
