using System.Collections.Generic;
using Notifications;

public class GameNotificationPool<TView, TNotificationEntity>
    where TView : NotificationView<TNotificationEntity>
{
    public GameNotificationPool(int size)
    {
        _active = new List<TView>(size);
    }

    private readonly Stack<TView> _pool = new();

    private readonly IList<TView> _active;

    public void Notify(TNotificationEntity entity)
    {
        if (!_pool.TryPop(out var obj))
            return;

        obj.gameObject.SetActive(true);
        _active.Add(obj); 
        obj.Notify(entity);
    }

    public void Add(TView obj)
    {
        obj.Init(() => Return(obj));
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
    }

    public void Return(TView obj)
    {
        if (!_active.Remove(obj))
            return;

        obj.OnReturn();
        obj.gameObject.SetActive(false); 
        _pool.Push(obj);
    }

    public void ReturnAll()
    {
        for (var i = _active.Count - 1; i >= 0; i--)
        {
            Return(_active[i]);
        }
    }
}
