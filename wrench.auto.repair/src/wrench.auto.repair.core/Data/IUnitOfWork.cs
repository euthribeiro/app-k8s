namespace wrench.auto.repair.core.Data
{
    public interface IUnitOfWork
    {
        Task<bool> CommitAsync();
    }
}
