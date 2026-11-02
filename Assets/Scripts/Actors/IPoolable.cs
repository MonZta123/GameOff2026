namespace Actors
{
    public interface IPoolable<in T>
    {
        short Id { get; }
        void Init(T entity);
        void SetState(T entity);
        void Release();
    }
}
