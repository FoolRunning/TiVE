using NUnit.Framework;
using ProdigalSoftware.TiVE.ScriptSystem;

namespace TiVETests.Scripts
{
    [TestFixture]
    public class LuaScriptsTests
    {
        [Test]
        public void AddLuaTableForEnum()
        {
            dynamic lua = new DynamicLua.DynamicLua();
            ScriptSystem.AddLuaTableForEnum<TestEnum>(lua);

            Assert.That(lua.TestEnum.One, Is.EqualTo(TestEnum.One));
            Assert.That(lua.TestEnum.Five, Is.EqualTo(TestEnum.Five));
            Assert.That(lua.TestEnum.Two, Is.EqualTo(TestEnum.Two));
            Assert.That(lua.TestEnum.Monkey, Is.EqualTo(TestEnum.Monkey));

            Assert.That(lua.TestEnum.There, Is.EqualTo(null)); // Lua scripts return null for an undefined value
        }

        private enum TestEnum
        {
            One, Five, Two, Monkey,
        }
    }
}
