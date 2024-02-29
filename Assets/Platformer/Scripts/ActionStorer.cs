using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class ActionStorer
{
    private List<ActionPoolItem> _actionPool = new List<ActionPoolItem>();
    private ActionTimer _actionTimer;
    private Character _character;


    public ActionStorer(Character character) { 
        _actionTimer = new ActionTimer();
        _character = character;

        _actionTimer.SetCurrentTime(0);
    }
    void Start()
    {
        Init();
    }

    private void Init()
    {
        _actionTimer = new ActionTimer();
    }

    public void Update()
    {
        _actionTimer.Count();
    }

    public void StoreAction(Actions action)
    {
        ActionPoolItem newActionPoolItem = new ActionPoolItem(action, _actionTimer.currentTime(), _character.transform.position);
        _actionPool.Add(newActionPoolItem);
    }

    public List<ActionPoolItem> actionPool()
    {
        return new List<ActionPoolItem>(_actionPool);
    }

    public void ResetActionPool()
    {
        _actionPool = new List<ActionPoolItem>();
        _actionTimer.SetCurrentTime(0);
    }
}

public class ActionPoolItem
{
    Actions _action;
    float _time;
    Vector3 _location;

    public ActionPoolItem(Actions action, float time, Vector3 location)
    {
        this._action = action;
        this._time = time;
        this._location = location;
    }
    public Actions action { get { return _action; } }
    public float time { get { return _time; } }
    public Vector3 location { get { return _location; } }
}


