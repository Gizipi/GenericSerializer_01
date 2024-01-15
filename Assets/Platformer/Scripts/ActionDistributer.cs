using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDistributer : MonoBehaviour
{

    [SerializeField] Character _player;
    [SerializeField] ActionPerformer _performer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayerInput();
    }

    void GetPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ActionStorer storer = _player.actionStorer;
            List<ActionPoolItem> list = storer.actionPool();
            storer.ResetActionPool();

            _performer.RecieveActionPool(list, _player, 0);
        }

        //if (Input.GetKeyDown("attack"))
        //{
        //    _player.RecieveAction(Actions.pressAttack);
        //}
        //else if (Input.GetKeyUp("attack"))
        //{
        //    _player.RecieveAction(Actions.releaseAttack);
        //}
        //if (Input.GetKeyDown("dodge"))
        //{
        //    _player.RecieveAction(Actions.pressDodge);
        //}
        //else if (Input.GetKeyUp("dodge"))
        //{
        //    _player.RecieveAction(Actions.releaseDodge);
        //}

        if (Input.GetKeyDown("left"))
        {
            _player.RecieveAction(Actions.pressLeft);
        }
        else if (Input.GetKeyUp("left"))
        {
            _player.RecieveAction(Actions.releaseLeft);
        }
        if (Input.GetKeyDown("right"))
        {
            _player.RecieveAction(Actions.pressRight);
        }
        else if (Input.GetKeyUp("right"))
        {
            _player.RecieveAction(Actions.releaseRight);
        }
        if (Input.GetKeyDown("up"))
        {
            _player.RecieveAction(Actions.pressUp);
        }
        else if (Input.GetKeyUp("up"))
        {
            _player.RecieveAction(Actions.releaseUp);
        }
        if (Input.GetKeyDown("down"))
        {
            _player.RecieveAction(Actions.pressDown);
        }
        else if (Input.GetKeyUp("down"))
        {
            _player.RecieveAction(Actions.releaseDown);
        }
    }
}
