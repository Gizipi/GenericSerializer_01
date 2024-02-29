using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTimer 
{

    private float _currentTime = -1;


    public void SetCurrentTime(float currentTime) {
        _currentTime = currentTime; 
    }

    public void Count()
    {
        if (_currentTime < 0)
            return;

        _currentTime += Time.deltaTime;
    }

    public float currentTime()
    {
        return _currentTime;
    }
}
