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

            Assert.AreEqual("Thisisa\ntest.\n\n", Messages.AllText); // Extra newline is from empty line ready to be filled at end of view
        }
    }
}
