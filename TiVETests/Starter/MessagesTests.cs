using NUnit.Framework;
using ProdigalSoftware.TiVE.Starter;

namespace TiVETests.Starter
{
    [TestFixture]
    public class MessagesTests
    {
        [Test]
        public void AddMessage()
        {
            Messages.Print("This");
            Messages.Print("is");
            Messages.Println("a");
            Messages.Println("test.");

            Assert.AreEqual("Thisisa\ntest.\n", Messages.AllText);
        }
    }
}
