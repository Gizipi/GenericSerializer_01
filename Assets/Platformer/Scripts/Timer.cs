using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTimer : MonoBehaviour
{

    private float _currentTime = -1;


    public void SetCurrentTime(float currentTime) => _currentTime = currentTime;

    // Update is called once per frame
    void Update()
    {
        Count();
    }

    private void Count()
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
