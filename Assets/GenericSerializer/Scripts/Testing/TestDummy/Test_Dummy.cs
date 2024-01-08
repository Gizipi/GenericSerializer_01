using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Test_Dummy
{
    [SerializableVariable]
    private int val = 5;
    [SerializableVariable]
    private Test_Dummy_v2 obj1 = new Test_Dummy_v2();
    [SerializableVariable]
    private Test_Dummy_v2 obj2 = new Test_Dummy_v2(2);


    public int ReadVal()
    {
        Debug.Log($"[Test_Dummy]Val: {val}");
        return val;
    }
    public void WriteVal(int newVal)
    {
        val = newVal;
    }
    public void ReadOBJValues()
    {
        obj1.ReadVal();
        obj2.ReadVal();
    }
    public void ChangeOBJValues()
    {
        obj1.changeVal();
        obj2.changeChar();
    }
}
public class Test_Dummy_v2
{

    [SerializableVariable]
    private char charValue = 'l';

    [SerializableVariable]
    private int value = 4;


    public Test_Dummy_v2(int val = 4)
    {
        value = val;
    }

    public void changeVal()
    {
        value += 1;
    }

    public void changeChar()
    {
        charValue = 'a';
    }

    public void ReadVal()
    {
        Debug.Log($"[Test_Dummy_V2]Value: {value}, charValue: {charValue}");
    }
}
