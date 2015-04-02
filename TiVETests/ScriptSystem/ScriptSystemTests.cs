using MoonSharp.Interpreter;
using NUnit.Framework;

namespace TiVETests.ScriptSystem
{
    [TestFixture]
    public class ScriptSystemTests
    {
        [Test]
        public void AddLuaTableForEnum()
        {
            Script lua = new Script();
            ProdigalSoftware.TiVE.ScriptSystem.ScriptSystem.AddLuaTableForEnum<TestEnum>(lua);

            Assert.That(lua.Globals.Get("TestEnum").Table["One"], Is.EqualTo((int)TestEnum.One));
            Assert.That(lua.Globals.Get("TestEnum").Table["Five"], Is.EqualTo((int)TestEnum.Five));
            Assert.That(lua.Globals.Get("TestEnum").Table["Two"], Is.EqualTo((int)TestEnum.Two));
            Assert.That(lua.Globals.Get("TestEnum").Table["Monkey"], Is.EqualTo((int)TestEnum.Monkey));

            Assert.That(lua.Globals.Get("TestEnum").Table["There"], Is.Null); // Lua scripts return null for an undefined value
        }

        private enum TestEnum
        {
            One, Five, Two, Monkey,
        }
    }
}
