using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test_SaveScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void Test_SaveScriptSimplePasses()
    {
        // Use the Assert class to test conditions

        Test_Dummy test_Dummy1 = new Test_Dummy();

        int testIntCache = test_Dummy1.ReadVal();
        test_Dummy1.ReadOBJValues();
        
        Test_GenericSaving.Serialize(test_Dummy1, new WriteStream(Application.dataPath + "/save.txt"));
        test_Dummy1.ChangeOBJValues();
        test_Dummy1.ReadOBJValues();

        test_Dummy1.WriteVal(3);

        test_Dummy1.ReadVal();
        Test_GenericSaving.Serialize(test_Dummy1, new ReadStream(Application.dataPath + "/save.txt"));
        test_Dummy1.ReadOBJValues();
        int comparedIntValue = test_Dummy1.ReadVal();
        //Assert.AreEqual(testIntCache, comparedIntValue);
    }
}
