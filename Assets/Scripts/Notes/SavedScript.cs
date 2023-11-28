using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedScript : MonoBehaviour
{

    private int count = 3;
    private int otherCount = 2;
    private char shortName = 'l';   


    void Start()
    {
        Debug.Log($"Start Save Script: {Application.dataPath + "/value.txt"}");
        //WriteStream stream = new WriteStream(Application.dataPath + "save.txt");
        //stream.SaveBytes();
        GenericSaving.Serialize(this, new WriteStream(Application.dataPath + "/save.txt"));
        //GenericSaving.Serialize(this, new ReadStream(Application.dataPath + "/save.txt"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
