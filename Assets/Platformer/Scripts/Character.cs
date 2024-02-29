using facetedBuilder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    private ActionStorer _actionStorer;
    public Gravity _gravity;
    // Start is called before the first frame update
    void Start()
    {
        Test_Character character = CharacterBuilder.Create().OfType(test_characterType.boss).WithName("Meme Lord").Build();
        //Person person = CodeBuilder.Create().WithName("jerry").WithAge(20).Build();
        Code cb = new CodeBuilder("Person").AddField("Name", "string").AddField("Age", "int");
        Debug.Log($"Demo character builder: {character.type}, {character.name}");
        Debug.Log(cb.ToString());

        _gravity = GetComponent<Gravity>();
        _actionStorer = new ActionStorer(this);
        _actionStorer.StoreAction(Actions.idle);
    }

    // Update is called once per frame
    void Update()
    {
        if ( _actionStorer != null )
            _actionStorer.Update();
    }

    public void RecieveAction(Actions action, bool inverted = false)
    {
        _actionStorer.StoreAction(action);
        //this.Movement(action, inverted);
        this.GravityChanging(action, inverted);
    }

    private void Movement(Actions action, bool inverted = false)
    {
        int dir = 1;
        if (inverted)
            dir = -1;

        if (action == Actions.pressRight)
        {
            gameObject.transform.position = gameObject.transform.position + (Vector3.right * dir);
        }
        else if (action == Actions.pressLeft)
            gameObject.transform.position = gameObject.transform.position + (Vector3.left * dir);
        else if (action == Actions.pressUp)
            gameObject.transform.position = gameObject.transform.position + (Vector3.up * dir);
        else if (action == Actions.pressDown)
            gameObject.transform.position = gameObject.transform.position + (Vector3.down * dir);
    }

    private void GravityChanging(Actions action, bool inverted = false)
    {
        int multiplyer = 1;
        int dir = 1;
        if (inverted)
        {
            dir = -1;
            multiplyer = 10;
        }

        if (action == Actions.pressRight)
        {
            _gravity.SetDirection((Vector3.right * dir) + new Vector3(0,0,multiplyer));
        }
        else if (action == Actions.pressLeft)
            _gravity.SetDirection((Vector3.left * dir) + new Vector3(0, 0, multiplyer));
        else if (action == Actions.pressUp)
            _gravity.SetDirection((Vector3.up * dir) + new Vector3(0, 0, multiplyer));
        else if (action == Actions.pressDown)
            _gravity.SetDirection((Vector3.down * dir) + new Vector3(0, 0, multiplyer));
    }

    public ActionStorer actionStorer { get { return _actionStorer; } }

}
