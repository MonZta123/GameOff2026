namespace Actors
{
    public interface IEntityView<in TEntity>
    {
        void Init(TEntity entity);

        void SetState(TEntity entity);

        void Release();
    }
}
