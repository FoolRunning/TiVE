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
            Engine engine = new Engine();
            
            engine.AddSystem(system1);
            engine.UpdateSystems(1.2f);
            VerifySystem(system1, 1, 1.2f);
            VerifySystem(system2, 0, 0.0f);
            VerifySystem(system3, 0, 0.0f);

            engine.AddSystem(system2);
            engine.UpdateSystems(1.2f);
            VerifySystem(system1, 2, 2.4f);
            VerifySystem(system2, 1, 1.2f);
            VerifySystem(system3, 0, 0.0f);

            engine.AddSystem(system3);
            engine.UpdateSystems(1.2f);
            VerifySystem(system1, 3, 3.6f);
            VerifySystem(system2, 2, 2.4f);
            VerifySystem(system3, 1, 1.2f);
        }

        private static void VerifySystem(TestSystem system, int expectedUpdateCount, float expectedTotalTime)
        {
            Assert.That(system.UpdateCount, Is.EqualTo(expectedUpdateCount));
            Assert.That(system.TotalTime, Is.InRange(expectedTotalTime - 0.00001f, expectedTotalTime + 0.00001f));
        }

        private sealed class TestSystem : EngineSystem
        {
            public int UpdateCount;
            public float TotalTime;

            public override string DebuggingName
            {
                get { return "TestSystem"; }
            }

            public override void Initialize()
            {
                
            }

            protected override void UpdateInternal(float timeDelta)
            {
                UpdateCount++;
                TotalTime += timeDelta;
            }
        }
    }
}
