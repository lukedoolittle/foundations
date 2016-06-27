using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Foundations.Extensions;
using Foundations.Reflection;
using Foundations.Test.Mocks;
using Xunit;

namespace Foundations.Test
{
    public class ReflectionTests
    {
        [Fact]
        public void GetAllConcreteImplementorsReturnsOnlyConcreteItems()
        {
            var type = typeof (BaseClass);
            var expected = new List<Type> {typeof (ConcreteDerivedClass), typeof (ConcreteDoubleDerivedClass), typeof(DoubleDerivedClass)};

            var actual = Reflection.Reflection.GetAllConcreteImplementors(type, GetType().Assembly);

            Assert.Equal(expected.Count, actual.Count());

            foreach (var actualType in actual)
            {
                Assert.True(expected.Contains(actualType));
            }
        }

        [Fact]
        public void CreateGenericTypeMakesTheCorrectType()
        {
            var genericType = typeof (DummyGenericClass<>);
            var type = typeof (DummyClass);

            var generic = Reflection.Reflection.CreateGenericType(genericType, type);

            Assert.NotNull(generic);
        }

        [Fact]
        public void CreateGenericTypeWithDoubleGenericMakesAValidType()
        {
            var genericType = typeof(Event3<,>);
            var type = typeof(EventConstraintBase2);
            var subType = typeof (EventConstraintBase1);

            var generic = Reflection.Reflection.CreateGenericType(genericType, new Type[] {subType, type});

            Assert.NotNull(generic);
        }

        [Fact]
        public void InvokeGenericMethodWillInvokeAPrivateGenericMethodOnAnInstance()
        {
            var instance = new DummyGenericClass<DummyClass>(1);
            
            Reflection.Reflection.InvokeGenericMethod(instance, "GenericMethod", typeof(DummyClass));

            Assert.Equal(1, instance.CallCount);
            Assert.Equal(typeof(DummyClass).FullName, instance.LastCallType.FullName);
        }

        [Fact]
        public void CreateGenericInstanceGreatesValidInstance()
        {
            var genericType = typeof(DummyGenericClass<>);
            var genericTypeParameter = typeof(DummyClass);

            var instance =
                new InstanceCreator(genericType)
                    .AddGenericParameter(genericTypeParameter)
                    .AddConstructorParameter(1)
                    .Create<object>();

            Assert.NotNull(instance);
        }

        [Fact]
        public void CreateGenericInstanceWithNoConstructorParametersGreatesValidInstance()
        {
            var genericType = typeof(DummyGenericClass<>);
            var genericTypeParameter = typeof(DummyClass);

            var instance =
                new InstanceCreator(genericType)
                    .AddGenericParameter(genericTypeParameter)
                    .Create<object>();

            Assert.NotNull(instance);
        }

        [Fact]
        public void CreateInstanceCreatesInstanceOfProperType()
        {
            var instance = new InstanceCreator(typeof(DummyClass))
                .AddConstructorParameter("string")
                .Create<object>();

            Assert.NotNull(instance);
        }

        public void GettingBaseNullInstanceThrowsException()
        {
            Type type = null;
            Assert.Throws<ArgumentNullException>(() => type.HasBase(null));
        }

        [Fact]
        public void GettingBaseNoBaseClassReturnsCorrectly()
        {
            var type = typeof(AnotherClass);
            var @base = typeof(BaseClass);

            var actual = type.HasBase(@base);

            Assert.False(actual);
        }

        [Fact]
        public void GettingBaseShallowClassReturnsCorrectly()
        {
            var type = typeof(DerivedClass);
            var @base = typeof(BaseClass);

            var actual = type.HasBase(@base);

            Assert.True(actual);
        }

        [Fact]
        public void GettingBaseDeepClassReturnsCorrectly()
        {
            var type = typeof(DoubleDerivedClass);
            var @base = typeof(BaseClass);

            var actual = type.HasBase(@base);

            Assert.True(actual);
        }

        [Fact]
        public void GettingCustomAttributeNullInstanceThrowsException()
        {
            Type type = null;
            Assert.Throws<ArgumentNullException>(() => type.GetCustomAttributes<SomeAttribute>());
        }

        [Fact]
        public void GettingCustomAttributeWithTypeThatDoesNotHaveAttributeReturnsNull()
        {
            Type type = typeof(AnotherClass);
            var result = type.GetCustomAttributes<SomeAttribute>();

            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void GettingCustomAttributeForATypeReturnsThatAttribute()
        {
            var type = typeof(SomeClass);

            var actual = type.GetCustomAttributes<SomeAttribute>();

            Assert.NotNull(actual);
            Assert.Equal("somestring", actual.First().SomeValue);
        }

        [Fact]
        public void FactNoArgument()
        {
            var instance = new MyClassOfMethods();
            instance.MethodB();

            Test(instance, null);
        }

        [Fact]
        public void FactNoGenericsButDerived()
        {
            var instance = new MyClassOfMethods();
            var argument = new ConstraintB();
            instance.MethodC(argument);

            Test(instance, argument);
        }

        [Fact]
        public void Fact()
        {
            var instance = new MyClassOfMethods();
            var argument = new ConstraintA();
            instance.MethodD(argument);


            Test(instance, argument);
        }

        [Fact]
        public void TestDerived()
        {
            var instance = new MyClassOfMethods();
            var argument = new ConstraintB();
            instance.MethodD(argument);

            Test(instance, argument);
        }

        [Fact]
        public void FactEmbedded()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyClass<ConstraintA>();
            instance.MethodE(argument);

            Test(instance, argument);
        }

        [Fact]
        public void FactEmbeddedAndDerived()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyClass<ConstraintB>();
            instance.MethodE(argument);

            Test(instance, argument);
        }

        [Fact]
        public void FactInherited()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyThirdClass();
            instance.MethodE(argument);

            Test(instance, argument);
        }

        [Fact]
        public void InvokeMethodAllInOne()
        {
            var instance = new MyClassOfMethods();
            var argument = new MyThirdClass();

            instance.InvokeMethodMatchingParameters(argument);
        }

        private void Test(object instance, object argument)
        {
            IEnumerable<MethodInfo> methods = instance.GetMethodInfosMatchingParameterSignature(argument);
            Assert.True(methods.Count() == 1);

            var method = methods.First();

            if (argument != null)
            {
                var constructedMethod = method.MakeGenericMethodFromArguments(argument);

                constructedMethod.Invoke(instance, new object[] { argument });
            }
            else
            {
                method.Invoke(instance, null);
            }
        }
    }

    public class DoubleDerivedClass : DerivedClass { }


    public class AnotherClass
    { }

    [Some("somestring")]
    public class SomeClass
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SomeAttribute : Attribute
    {
        public string SomeValue { get; set; }

        public SomeAttribute(string someValue)
        {
            SomeValue = someValue;
        }
    }

    public class DummyClass
    {
        public DummyClass(string s) { }
    }

    public class DummyGenericClass<T>
    {
        public int CallCount { get; set; }

        public Type LastCallType { get; set; }

        public DummyGenericClass() { }

        public DummyGenericClass(int i) { } 

        private void GenericMethod<K>()
        {
            CallCount++;
            LastCallType = typeof (K);
        }
    }

    public abstract class BaseClass { }
    public abstract class DerivedClass : BaseClass { }
    public class ConcreteDerivedClass : BaseClass { }
    public class ConcreteDoubleDerivedClass : DerivedClass { }


    public class ConstraintA { }
    public class ConstraintB : ConstraintA { }
    public class MyClass<T> where T : ConstraintA { }
    public class MySecondClass<T> where T : ConstraintA { }
    public class MyThirdClass : MySecondClass<ConstraintB> { }
    public class MyClassOfMethods
    {
        public void MethodB() { }
        public void MethodC(ConstraintA someParameter) { }
        public void MethodD<T>(T someParameter) where T : ConstraintA { }
        public void MethodE<T>(MyClass<T> someParameter) where T : ConstraintA { }
        public void MethodE<T>(MySecondClass<T> someParameter) where T : ConstraintB { }
    }

}
