using System.Collections.Generic;
using Actors;
using UnityEngine;

public class GameObjectPool<TView, TEntity> where TView : MonoBehaviour, IEntityView<TEntity>
{
    public GameObjectPool(int size)
    {
        Active = new TView[size];
    }
    
    private readonly Stack<TView> _pool = new();

    public TView[] Active { get; }

    public TView Get(TEntity entity)
    {
        var obj = _pool.Pop();
        obj.Init(entity);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Add(TView obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
    }
    
    public void Return(TView obj)
    {
        obj.Release();
        obj.gameObject.SetActive(false);
        _pool.Push(obj);
    }
}
