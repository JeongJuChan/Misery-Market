using System.Collections.Generic;
using UnityEngine;

// 상속해서 원하는 객체를 풀링할 수 있도록 하는 오브젝트 풀러
// 풀링할 객체는 IPoolable을 붙여서 구현할 것
public class ObjectPooler<T> where T : MonoBehaviour, IPoolable<T>
{
    private T _prefab;
    private Queue<T> _pool = new Queue<T>();
    private List<T> _activeObjects = new List<T>();
    private Transform _parent;
    private int _initCount;
    private int _totalCount;
    private int _maxCount;

    public ObjectPooler(T prefab, Transform parent, int initCount, int maxCount)
    {
        _prefab = prefab;
        _parent = parent;
        _initCount = initCount;
        _maxCount = maxCount;

        for (int i = 0; i < initCount && _totalCount <= maxCount; i++)
        {
            InstantitateNewObject();
        }
    }

    #region Pool Methods
    public T Pool()
    {
        T t;

        if (_totalCount < _maxCount && _pool.Count <= _totalCount / 2)
        {
            for (int i = 0; i < _initCount && _totalCount <= _maxCount; i++)
            {
                InstantitateNewObject();
            }
        }

        if (_pool.Count > 0)
        {
            t = _pool.Dequeue();
        }
        else
        {
            t = _activeObjects[0];
            _activeObjects.RemoveAt(0);
        }

        _activeObjects.Add(t);
        t.gameObject.SetActive(true);

        return t;
    }

    public T Pool(Transform parent)
    {
        T t = Pool();
        t.transform.SetParent(parent);

        return t;
    }

    private void ReturnToPooler(T t)
    {
        _activeObjects.Remove(t);
        _pool.Enqueue(t);
        t.transform.SetParent(_parent);
        t.gameObject.SetActive(false);
    }

    private void InstantitateNewObject()
    {
        T t = Object.Instantiate(_prefab);
        t.transform.SetParent(_parent);
        t.gameObject.SetActive(false);
        t.returnAction += ReturnToPooler;
        _pool.Enqueue(t);
        _totalCount++;
    }
    #endregion
}