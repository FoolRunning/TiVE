using NUnit.Framework;
using ProdigalSoftware.TiVE.Core;

namespace TiVETests.Core
{
    /// <summary>
    /// Tests for the GameObjectManager class
    /// </summary>
    [TestFixture]
    public class GameObjectManagerTests
    {
        #region CreateNewGameObject tests
        [Test]
        public void CreateNewGameObject()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj = manager.CreateNewGameObject();
            VerifyGameObject(obj, new Handle(0, 1, 0));
        }

        [Test]
        public void CreateNewGameObject_Multiple()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            GameObject obj3 = manager.CreateNewGameObject();
            GameObject obj4 = manager.CreateNewGameObject();
            GameObject obj5 = manager.CreateNewGameObject();

            VerifyGameObject(obj1, new Handle(0, 1, 0));
            VerifyGameObject(obj2, new Handle(0, 1, 1));
            VerifyGameObject(obj3, new Handle(0, 1, 2));
            VerifyGameObject(obj4, new Handle(0, 1, 3));
            VerifyGameObject(obj5, new Handle(0, 1, 4));
        }

        [Test]
        public void CreateNewGameObject_AfterDelete()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            manager.Delete(obj1);

            GameObject obj3 = manager.CreateNewGameObject();
            VerifyGameObject(obj1, new Handle(0, 1, 0));
            VerifyGameObject(obj2, new Handle(0, 1, 1));
            VerifyGameObject(obj3, new Handle(0, 2, 0));
        }
        #endregion

        #region Get tests
        [Test]
        public void Get()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            GameObject obj3 = manager.CreateNewGameObject();

            Assert.That(manager.Get(new Handle(0, 1, 1)), Is.EqualTo(obj2));
            Assert.That(manager.Get(new Handle(0, 1, 0)), Is.EqualTo(obj1));
            Assert.That(manager.Get(new Handle(0, 1, 2)), Is.EqualTo(obj3));
        }

        [Test]
        public void Get_AfterDelete()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            GameObject obj3 = manager.CreateNewGameObject();

            manager.Delete(obj2);

            Assert.That(manager.Get(new Handle(0, 1, 0)), Is.EqualTo(obj1));
            Assert.That(manager.Get(new Handle(0, 1, 1)), Is.Null);
            Assert.That(manager.Get(new Handle(0, 1, 2)), Is.EqualTo(obj3));
        }

        [Test]
        public void Get_AfterDeleteAndAdd()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            GameObject obj3 = manager.CreateNewGameObject();

            manager.Delete(obj2);
            GameObject obj4 = manager.CreateNewGameObject();

            Assert.That(manager.Get(new Handle(0, 1, 0)), Is.EqualTo(obj1));
            Assert.That(manager.Get(new Handle(0, 1, 1)), Is.Null);
            Assert.That(manager.Get(new Handle(0, 1, 2)), Is.EqualTo(obj3));
            Assert.That(manager.Get(new Handle(0, 2, 1)), Is.EqualTo(obj4));
        }
        #endregion

        #region Delete tests
        [Test]
        public void Delete()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            manager.CreateNewGameObject();
            manager.CreateNewGameObject();

            manager.Delete(obj1);
            manager.Delete(new Handle(0, 1, 3));

            Assert.That(manager.Get(new Handle(0, 1, 0)), Is.Null);
            Assert.That(manager.Get(new Handle(0, 1, 3)), Is.Null);
        }

        [Test]
        public void Delete_SameObjMultipleTimes()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            GameObject obj3 = manager.CreateNewGameObject();

            manager.Delete(obj1);
            manager.Delete(obj1);
            manager.Delete(obj1);

            Assert.That(manager.Get(new Handle(0, 1, 0)), Is.Null);
            Assert.That(manager.Get(new Handle(0, 1, 1)), Is.EqualTo(obj2));
            Assert.That(manager.Get(new Handle(0, 1, 2)), Is.EqualTo(obj3));
        }

        [Test]
        public void Delete_UnknownHandle()
        {
            GameObjectManager manager = new GameObjectManager(5);
            GameObject obj1 = manager.CreateNewGameObject();
            GameObject obj2 = manager.CreateNewGameObject();
            GameObject obj3 = manager.CreateNewGameObject();

            manager.Delete(new Handle(0, 5, 0));
            manager.Delete(new Handle(1, 1, 0));
            manager.Delete(new Handle(0, 1, 4));

            Assert.That(manager.Get(new Handle(0, 1, 0)), Is.EqualTo(obj1));
            Assert.That(manager.Get(new Handle(0, 1, 1)), Is.EqualTo(obj2));
            Assert.That(manager.Get(new Handle(0, 1, 2)), Is.EqualTo(obj3));
        }
        #endregion

        #region Private helper methods
        private static void VerifyGameObject(GameObject obj, Handle expectedHandle)
        {
            Assert.That(obj.Handle, Is.EqualTo(expectedHandle));
        }
        #endregion
    }
}
