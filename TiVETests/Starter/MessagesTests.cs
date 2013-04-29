using NUnit.Framework;
using ProdigalSoftware.TiVE.Starter;

namespace ProdigalSoftware.TiVETests.Starter
{
    [TestFixture]
    public class MessagesTests
    {
        [Test]
        public void AddMessage()
        {
            Messages.Print("This");
            Messages.Print("is");
            Messages.Print("a");
            Messages.Println("test.");

            Assert.AreEqual("Thisisatest.\n", Messages.AllText);
        }
    }
}
