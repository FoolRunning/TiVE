using NUnit.Framework;
using ProdigalSoftware.TiVE.Core;

namespace TiVETests.Core
{
    [TestFixture]
    public class EngineTests
    {
        [Test]
        public void Update()
        {
            TestSystem system1 = new TestSystem();
            TestSystem system2 = new TestSystem();
            TestSystem system3 = new TestSystem();
            Engine engine = new Engine(5);
            
            engine.AddSystem(system1);
            engine.UpdateSystems(12);
            VerifySystem(system1, 1, 12);
            VerifySystem(system2, 0, 0);
            VerifySystem(system3, 0, 0);

            engine.AddSystem(system2);
            engine.UpdateSystems(12);
            VerifySystem(system1, 2, 24);
            VerifySystem(system2, 1, 12);
            VerifySystem(system3, 0, 00);

            engine.AddSystem(system3);
            engine.UpdateSystems(12);
            VerifySystem(system1, 3, 36);
            VerifySystem(system2, 2, 24);
            VerifySystem(system3, 1, 12);
        }

        private static void VerifySystem(TestSystem system, int expectedUpdateCount, int expectedTotalTicks)
        {
            Assert.That(system.UpdateCount, Is.EqualTo(expectedUpdateCount));
            Assert.That(system.TotalTicks, Is.EqualTo(expectedTotalTicks));
        }

        private sealed class TestSystem : EngineSystem
        {
            public int UpdateCount;
            public int TotalTicks;

            public TestSystem() : base("TestSystem")
            {
            }

            public override void Dispose()
            {   
            }

            public override bool Initialize()
            {
                return true;
            }

            public override void ChangeScene(Scene newScene)
            {
            }

            protected override bool UpdateInternal(int ticksSinceLastUpdate, float timeBlendFactor, Scene currentScene)
            {
                UpdateCount++;
                TotalTicks += ticksSinceLastUpdate;

                return true;
            }
        }
    }
}
