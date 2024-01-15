using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ActionPerformer : MonoBehaviour
{

    float _count = 0;
    int _index = 0;
    List<ActionPoolItem> _currentActionPool;
    Character _CurrentCharacter;


    // Update is called once per frame
    void Update()
    {
        PerformActionPool();
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

        Debug.Log($"Index: {_index}, time on this acton: {_currentActionPool[_index].time}");
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

        _CurrentCharacter = character;
        _currentActionPool = new List<ActionPoolItem>(actionpool);
        _index = 0;
        _count = time;
        // _currentActionPool = new List<ActionPoolItem>(actionpool.Count);
        //actionpool.ForEach(item =>
        //{
        //    _currentActionPool.Add(new ActionPoolItem(item));
        //});
    }
}

public enum Actions
{
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
    releaseSpecial = 16
}