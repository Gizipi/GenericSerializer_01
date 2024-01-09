using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPerformer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    dodge = 9,
    block = 10,
    special1 = 11,
    special2 = 12,
    special3 = 13,
    special4 = 14
}