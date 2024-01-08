using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

public class Notes : MonoBehaviour
{

    //collects information loaded in the assembly
    //creates type instances during runtime
    //loading each assembly into the correct domain and controlling the memory layout in the type hierachy 

    //assemblies contain modules
    //modules contain types
    // types contain members
    //reflections allow you to encapsulate these
    //you can dynamically create an instance of a type with reflections, bind the type to an existing object,
    //or get the type from an existing object. you can then invoke the type methods or access its fields and properties

    //typically used for:
    //<Assembly> defining and loading assemblies
    //use of <module> to locate information such as the assembly that contains the module
    //<ContructorInfo> discovers information like the name, parameters, access modifiers (private, Public), and implementation details (abstract, virtual)
    //<MethodInfo> dicover stuff like the name, return type, params, access modifiers,  and implementation details. use GetMethod/s
    //<FieldInfo>
    //<EventInfo>
    //<propertyInfo>
    //<ParameterInfo>
    //<CustomAttributeData
    //
    //can also create type browsers


    //viewing type information
    //<System.Type> is central to reflection
    //use <Assembly.GetType/s>
    //syntax for getting the assembly object and module
    Assembly a = typeof(UnityEngine.Object).Module.Assembly;

    private void Start()
    {
        //personal test
        Method();
    }

    private void Method()
    {
        //loading
        Assembly b = GetAssemblyOfType<Frog>();
        //get type name from assembly
        Type[] types = b.GetTypes();
        foreach (Type type in types)
        {
            //Debug.Log( t.FullName );
            if (type == typeof(Frog))
            {
                Debug.Log(type.FullName);
                FieldInfo[] fields = type.GetFields();

                foreach (FieldInfo field in fields)
                {
                    Debug.Log($"Field: {field.Module}, {field.FieldType.Name}, {field.Name}");
                }

                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    //Debug.Log($"Field: {property.Module}, {property.PropertyType}, {property.Name}");
                }

                MethodInfo[] methodInfos = type.GetMethods();
                foreach (MethodInfo method in methodInfos)
                {
                    //Debug.Log($"Method: {GetAccessModifierName(method)} {GetImplementationDetails(method)} {method.ReturnType} {method.Name}");

                    //Debug.Log(method.Invoke());
                }
            }
        }
    }

    private string GetAccessModifierName(MethodInfo info)
    {
        if (info.IsPublic)
            return "public";
        else if (info.IsPrivate)
            return "private";
        else
            return "";
    }
    private string GetImplementationDetails(MethodInfo info)
    {

        if (info.IsStatic)
            return "static";
        else if (info.IsVirtual)
            return "virtual";
        else
            return "";
    }

    Assembly GetAssemblyOfType<T>() => typeof(T).Assembly;

    
}
