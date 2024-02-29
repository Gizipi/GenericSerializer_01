using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public enum test_characterType
{
    player,
    boss
}

public class Test_Character
{
    public test_characterType type;
    public string name;
}

public interface ISpecifyCharacterType
{
    ISpecifyCharacterName OfType(test_characterType type);
}

public interface ISpecifyCharacterName
{
    IBuildCharacter WithName(string name);
}

public interface IBuildCharacter
{
    public Test_Character Build();
}


public abstract class FunctionalBuilder<TSubject, TSelf> where TSelf : FunctionalBuilder<TSubject, TSelf> where TSubject : new ()
{
     private readonly List<Func<Test_Character, Test_Character>> actions = new List<Func<Test_Character, Test_Character>> ();

    //public TSelf Called(string name)
    //    => Do(p => p.name = name);

    public TSelf Do(Action<Test_Character> action)
        => AddAction(action);

    public Test_Character Build()
        => actions.Aggregate(new Test_Character(), (p, f) => f(p));

     private TSelf AddAction(Action<Test_Character> action)
    {
        actions.Add(p =>
        {
            action(p);
            return p;
        });
        return (TSelf)this;
    }
}

public sealed class funcCharacterBuilder : FunctionalBuilder<Test_Character, funcCharacterBuilder>
{
    public funcCharacterBuilder Called(string name)
    => Do(p => p.name = name);
}

public class CharacterBuilder
{
    private class Impl : ISpecifyCharacterType, ISpecifyCharacterName, IBuildCharacter
    {
        private Test_Character character = new Test_Character();
        
        public ISpecifyCharacterName OfType(test_characterType type)
        {
            character.type = type;
            return this;
        }

        public IBuildCharacter WithName(string name)
        {
            character.name = name;
            return this;
        }

        public Test_Character Build()
        {
            return character;
        }
    }
    public static ISpecifyCharacterType Create()
    {
        return new Impl();
    }
}

//public class Demo
//{
//    private void()
//    {
        
//    }
//}



//using System;

//namespace Coding.Exercise
//{

//    public class Person
//    {
//        public string Name;
//        public int Age;
//    }

//    public interface ISpecifyName
//    {
//        public ISpecifyAge WithName(string name);
//    }
//    public interface ISpecifyAge
//    {
//        public IBuildPerson WithAge(int age);
//    }
//    public interface IBuildPerson
//    {
//        public Person Build();
//    }

//    public class CodeBuilder
//    {
//        private class Impl : ISpecifyName, ISpecifyAge, IBuildPerson
//        {
//            private Person person = new Person();

//            public ISpecifyAge WithName(string name)
//            {
//                person.Name = name;
//                return this;
//            }

//            public IBuildPerson WithAge(int age)
//            {
//                person.Age = age;
//                return this;
//            }

//            public Person Build()
//            {
//                return person;
//            }
//        }

//        public static ISpecifyName Create()
//        {
//            return new Impl();
//        }
//    }
//}



namespace facetedBuilder
{

    public class Code
    {
        public string className;

        public List<(string name, string type)> fields = new List<(string name, string type)>();

        public override string ToString()
        {
            String content = "";
            foreach (var field in fields)
            {
                content += $"public {field.type} {field.name};\n"; 
            }
            char start = '{';
            char end = '}';
            return $"public class {className}\n{start}\n{content}{end}";
        }
    }

    public class CodeBuilder
    {
        protected Code code = new Code();
        private string className;
        public CodeBuilder(string className)
        {
            code.className = className;
        }

        public CodeBuilder AddField(string fieldName, string fieldType)
        {
            code.fields.Add((fieldName, fieldType));
            return this;
        }

        public static implicit operator Code(CodeBuilder cb)
        {
            return cb.code;
        }

        //public classContentBuilder AddField => new classContentBuilder(className,code);
    }


    public class classContentBuilder : CodeBuilder
    {
        public classContentBuilder(string className,Code code) : base(className)
        {
            this.code = code;
            this.code.className = className;
        }
        public classContentBuilder AddField(string fieldName, string fieldType)
        {
            code.fields.Add((fieldName, fieldType));
            return this;
        }
    }



}



//using System;

//namespace Coding.Exercise
//{

//    public class Person
//    {
//        public string Name;
//        public int Age;
//    }


//    public interface ISpecifyField
//    {
//        public IBuildPerson AddField(string varName, string varType);
//    }

//    public interface IBuildClass
//    {
//        public string Build();
//    }

//    public class CodeBuilder
//    {
//        private string output;

//        public CodeBuilder(string className)
//        {
//            output = "$public class {className}\n'{'";
//        }

//        private class Impl : ISpecifyField, IBuildClass
//        {
//            private Person person = new Person();

//            public ISpecifyAge WithName(string name)
//            {
//                person.Name = name;
//                return this;
//            }

//            public IBuildPerson WithAge(int age)
//            {
//                person.Age = age;
//                return this;
//            }

//            public Person Build()
//            {
//                return person;
//            }
//        }

//        public static ISpecifyName Create()
//        {
//            return new Impl();
//        }
//    }
//}
