using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    ActionStorer _actionStorer;
    // Start is called before the first frame update
    void Start()
    {
        _actionStorer = new ActionStorer(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RecieveAction(Actions action)
    {
        _actionStorer.StoreAction(action);
        if (action == Actions.pressRight)
        {
            gameObject.transform.position = gameObject.transform.position + Vector3.right;
            Debug.Log("press right");
        }
        else if (action == Actions.pressLeft)
            gameObject.transform.position = gameObject.transform.position + Vector3.left;
        else if (action == Actions.pressUp)
            gameObject.transform.position = gameObject.transform.position + Vector3.up;
        else if (action == Actions.pressDown)
            gameObject.transform.position = gameObject.transform.position + Vector3.down;
    }

    public ActionStorer actionStorer { get { return _actionStorer; } }

}
