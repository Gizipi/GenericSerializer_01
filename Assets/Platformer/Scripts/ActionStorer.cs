using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActionStorer : MonoBehaviour
{
    private List<ActionPoolItem> _actionPool = new List<ActionPoolItem>();
    private ActionTimer _actionTimer;


    void Start()
    {
        Init();
    }

    private void Init()
    {
        _actionTimer = new ActionTimer();
    }

    public void StoreAction(Actions action)
    {
        ActionPoolItem newActionPoolItem = new ActionPoolItem(action, _actionTimer.currentTime(), transform.position);
        _actionPool.Add(newActionPoolItem);
    }

    public List<ActionPoolItem> actionPool()
    {
        return _actionPool;
    }
}

public class ActionPoolItem
{
    Actions action;
    float time;
    Vector3 location;

    public ActionPoolItem(Actions action, float time, Vector3 location)
    {
        this.action = action;
        this.time = time;
        this.location = location;
    }
}


