using NUnit.Framework;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVETests.Core
{
    /// <summary>
    /// Tests for the Scene class
    /// </summary>
    [TestFixture]
    public class SceneTests
    {
        private Scene scene;

        [SetUp]
        public void TestSetup()
        {
            scene = new Scene();
        }

        #region CreateNewEntity tests
        [Test]
        public void CreateNewEntity()
        {
            IEntity entity = scene.CreateNewEntity("1");
            VerifyEntity(entity);
        }

        [Test]
        public void CreateNewEntity_AfterDelete()
        {
            IEntity entity1 = scene.CreateNewEntity("1");
            IEntity entity2 = scene.CreateNewEntity("2");
            scene.DeleteEntity(entity1);

            IEntity entity3 = scene.CreateNewEntity("3");
            VerifyEntity(entity1);
            VerifyEntity(entity2);
            VerifyEntity(entity3);
        }
        #endregion

        #region AddComponent tests
        [Test]
        public void AddComponent()
        {
            IEntity entity = scene.CreateNewEntity("1");

            DummyComponent1 component1 = new DummyComponent1();
            entity.AddComponent(component1);

            VerifyEntity(entity, component1);
        }

        [Test]
        public void AddComponent_MultipleToSameEntity()
        {
            DummyComponent1 component1 = new DummyComponent1();
            DummyComponent1 component2 = new DummyComponent1();
            DummyComponent2 component3 = new DummyComponent2();

            IEntity entity = scene.CreateNewEntity("1");
            entity.AddComponent(component1);
            entity.AddComponent(component2);
            entity.AddComponent(component3);

            VerifyEntity(entity, component1, component2, component3);
        }

        [Test]
        public void AddComponent_MultipleToDifferentEntities()
        {
            DummyComponent1 component1 = new DummyComponent1();
            DummyComponent1 component2 = new DummyComponent1();
            DummyComponent2 component3 = new DummyComponent2();
            DummyComponent2 component4 = new DummyComponent2();

            IEntity entity1 = scene.CreateNewEntity("1");
            entity1.AddComponent(component1);
            entity1.AddComponent(component3);
            IEntity entity2 = scene.CreateNewEntity("2");
            entity2.AddComponent(component2);
            entity2.AddComponent(component4);

            VerifyEntity(entity1, component1, component3);
            VerifyEntity(entity2, component2, component4);
        }
        #endregion

        #region DeleteEntity tests
        [Test]
        public void DeleteEntity_Empty()
        {
            IEntity entity = scene.CreateNewEntity("1");
            scene.DeleteEntity(entity);

            VerifyEntity(entity);
        }

        [Test]
        public void DeleteEntity_WithComponents()
        {
            DummyComponent1 component1 = new DummyComponent1();
            DummyComponent2 component2 = new DummyComponent2();

            IEntity entity = scene.CreateNewEntity("1");
            entity.AddComponent(component1);
            entity.AddComponent(component2);

            scene.DeleteEntity(entity);

            VerifyEntity(entity);
        }
        #endregion

        private static void VerifyEntity(IEntity entity, params IComponent[] expectedComponents)
        {
            Assert.That(entity.Components, Is.EqualTo(expectedComponents));
        }

        private class DummyComponent1 : IComponent
        {
        }

        private class DummyComponent2 : IComponent
        {
        }
    }
}
