using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class values
{
    public const float MAX_TIME = 9;
}

public class ActionPerformer : MonoBehaviour
{

    float _count = 0;
    int _index = 0;
    List<ActionPoolItem> _currentActionPool;
    Character _CurrentCharacter;

    bool _preformingBackwards = false;


    // Update is called once per frame
    void Update()
    {
        if (_preformingBackwards)
        {
            BackwardsPerformActionPool();
        }
        else
        {
            PerformActionPool();
        }
    }

    private void BackwardsPerformActionPool()
    {
        if(_count <= 0 || _index <= 0)
        {
            _preformingBackwards = false;
            _count = 0;
            _index = 0;
            _CurrentCharacter._gravity.SetDirection(new Vector3(0, -1, 1));
            _CurrentCharacter.actionStorer.ResetActionPool();
            _CurrentCharacter.transform.position = _currentActionPool[0].location;
            return;
        }

        _count = _count - 10f;

        if (_count > _currentActionPool[_index].time)
        {
            return;
        }

        Vector3 characterLocation = _CurrentCharacter.gameObject.transform.position;
        Vector3 actionLocation = _currentActionPool[_index].location;

        if (characterLocation.x != actionLocation.x || characterLocation.y != actionLocation.y || characterLocation.z != actionLocation.z)
            _CurrentCharacter.gameObject.transform.position = actionLocation;

        _CurrentCharacter.RecieveAction(_currentActionPool[_index - 1].action, true);

        _index--;
    }

    private void PerformActionPool()
    {
        if (_currentActionPool == null)
            return;
        if (_index >= _currentActionPool.Count)
            return;
        _count++;
        if (_count < _currentActionPool[_index].time)
            return;

        Vector3 characterLocation = _CurrentCharacter.gameObject.transform.position;
        Vector3 actionLocation = _currentActionPool[_index].location;

        if (characterLocation.x != actionLocation.x || characterLocation.y != actionLocation.y || characterLocation.z != actionLocation.z)
            _CurrentCharacter.gameObject.transform.position = actionLocation;

        _CurrentCharacter.RecieveAction(_currentActionPool[_index].action);

        _index++;
        if (_index >= _currentActionPool.Count)
        {
            _currentActionPool = new List<ActionPoolItem>(0);
            _index = 0;
            Debug.Log($"Count: {_count}, index: {_index}, pool length: {_currentActionPool.Count}");
        }
    }

    public void RecieveActionPool(List<ActionPoolItem> actionpool, Character character, float time)
    {
        Debug.Log($"Recieve action pool, lenght: {actionpool.Count}");


        _preformingBackwards = true;
        _CurrentCharacter = character;
        _currentActionPool = new List<ActionPoolItem>();
            
        float currentLargestTime = actionpool[actionpool.Count -1].time;
        //if(currentLargestTime >  values.MAX_TIME)
        //{
            foreach(ActionPoolItem item in actionpool)
            {
            Debug.Log($"Time of action: {item.time}"); //pull backwards snipping out of here and try to put it into reverse preforming, also need to corectly store full action pull for repreforming actions
                if(item.time < 3) { 
                    _currentActionPool.Add(item);
                }

                if (item.time > (3 + (values.MAX_TIME / 3) / 2) && item.time <= (6 + (values.MAX_TIME / 3) / 2))
                {
                    _currentActionPool.Add(item);
                }
                if(item.time > currentLargestTime - 3)
                {
                    _currentActionPool.Add(item);
                }
            }
       // }

        _index = _currentActionPool.Count - 1;
        _count = _currentActionPool[_index].time;
        // _currentActionPool = new List<ActionPoolItem>(actionpool.Count);
        //actionpool.ForEach(item =>
        //{
        //    _currentActionPool.Add(new ActionPoolItem(item));
        //});
    }
}

public enum Actions
{
    idle = -1,
    jump = 0,
    pressLeft = 1,
    pressRight = 2,
    pressUp = 3,
    pressDown = 4,
    releaseLeft = 5,
    releaseRight = 6,
    releaseUp = 7,
    releaseDown = 8,
    pressAttack = 9,
    releaseAttack = 10,
    pressDodge = 11,
    releaseDodge = 12,
    pressBlock = 13,
    releaseBlock = 14,
    pressSpecial = 15,
    releaseSpecial = 16,
    timeTravel = 17
}