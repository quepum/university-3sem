// <copyright file="ReflectorTests.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyReflector.Tests;

[TestFixture]
public class ReflectorTests
{
    public class ClassA
    {
        public int FieldA;

        public string Shared() => "A";

        public void MethodA()
        {
        }
    }

    public class ClassB
    {
        public string FieldB;

        public int Shared() => 42;

        public void MethodB()
        {
        }
    }

    public class TestClass
    {
        public int PublicField;
        private static string privateStaticField = "test";

        public void PublicMethod(int x)
        {
        }

        public class Nested
        {
        }

        internal string GetString() => "hello";
    }

    [Test]
    public void GetMemberList_ReturnsCorrectMembers()
    {
        var reflector = new Reflector();
        var members = reflector.GetMemberList(typeof(TestClass));

        Assert.That(members, Has.Member("public Int32 PublicField"));
        Assert.That(members, Has.Member("private static String privateStaticField"));
        Assert.That(members, Has.Member("internal String GetString()"));
        Assert.That(members, Has.Member("public Void PublicMethod(Int32 x)"));
        Assert.That(members, Has.Member("nested class Nested"));
    }

    [Test]
    public void DiffClasses_ReturnsDifferences()
    {
        var reflector = new Reflector();
        var (onlyInA, onlyInB, different) = reflector.DiffClasses(typeof(ClassA), typeof(ClassB));
        Assert.Multiple(() =>
        {
            Assert.That(onlyInA, Has.Member("public Int32 FieldA"));
            Assert.That(onlyInA, Has.Member("public Void MethodA()"));
            Assert.That(onlyInB, Has.Member("public String FieldB"));
            Assert.That(onlyInB, Has.Member("public Void MethodB()"));

            Assert.That(different, Has.Some.StartsWith("A: public String Shared() | B: public Int32 Shared()"));
        });
    }

    [Test]
    public void PrintStructure_CreatesFile()
    {
        var reflector = new Reflector();
        const string fileName = "TestClass.cs";

        try
        {
            reflector.PrintStructure(typeof(TestClass));
            Assert.That(fileName, Does.Exist);

            var content = File.ReadAllText(fileName);
            Assert.That(content, Does.Contain("internal class TestClass"));
            Assert.That(content, Does.Contain("public Int32 PublicField;"));
            Assert.That(content, Does.Contain("public Void PublicMethod(Int32 x) { }"));
        }
        finally
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}