using System;
using System.IO;
using NUnit.Framework;
using ProdigalSoftware.TiVE.Core;
using ProdigalSoftware.TiVEPluginFramework;

namespace TiVETests.Core
{
    [TestFixture]
    public class TiVESerializerImplementationTests
    {
        private TiVESerializerImplementation serializer;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            serializer = new TiVESerializerImplementation();
            serializer.Initialize();
        }

        [Test]
        public void SerializeDeserializeClass_RoundTrip()
        {
            SerializedObj1 originalObj = new SerializedObj1(true, 863, "Is this a string?");
            byte[] serializedData;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                    serializer.Serialize(originalObj, writer);
                serializedData = stream.ToArray();
            }

            SerializedObj1 result;
            using (MemoryStream stream2 = new MemoryStream(serializedData))
            {
                using (BinaryReader reader = new BinaryReader(stream2))
                    result = serializer.Deserialize<SerializedObj1>(reader);
            }

            Assert.That(result.MyBool, Is.True);
            Assert.That(result.MyInt, Is.EqualTo(863));
            Assert.That(result.MyString, Is.EqualTo("Is this a string?"));
        }

        [Test]
        public void SerializeDeserializeStruct_RoundTrip()
        {
            SerializedStruct1 originalObj = new SerializedStruct1(true, 863, "Is this a string?");
            byte[] serializedData;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                    serializer.Serialize(originalObj, writer);
                serializedData = stream.ToArray();
            }

            SerializedStruct1 result;
            using (MemoryStream stream2 = new MemoryStream(serializedData))
            {
                using (BinaryReader reader = new BinaryReader(stream2))
                    result = serializer.Deserialize<SerializedStruct1>(reader);
            }

            Assert.That(result.MyBool, Is.True);
            Assert.That(result.MyInt, Is.EqualTo(863));
            Assert.That(result.MyString, Is.EqualTo("Is this a string?"));
        }

        #region SerializedObj1 class
        public sealed class SerializedObj1 : ITiVESerializable
        {
            public static readonly Guid ID = new Guid("37096930-CB73-4233-9DBD-AF18BE69796F");

            public readonly bool MyBool;
            public readonly int MyInt;
            public readonly string MyString;

            public SerializedObj1(BinaryReader reader)
            {
                MyBool = reader.ReadBoolean();
                MyInt = reader.ReadInt32();
                MyString = reader.ReadString();
            }

            public SerializedObj1(bool myBool, int myInt, string myString)
            {
                MyBool = myBool;
                MyInt = myInt;
                MyString = myString;
            }

            public void SaveTo(BinaryWriter writer)
            {
                writer.Write(MyBool);
                writer.Write(MyInt);
                writer.Write(MyString);
            }
        }
        #endregion

        #region SerializedStruct1 class
        public struct SerializedStruct1 : ITiVESerializable
        {
            public static readonly Guid ID = new Guid("65110A0A-8A81-47AB-AE7A-66F69B9ADA97");

            public readonly bool MyBool;
            public readonly int MyInt;
            public readonly string MyString;

            public SerializedStruct1(BinaryReader reader)
            {
                MyBool = reader.ReadBoolean();
                MyInt = reader.ReadInt32();
                MyString = reader.ReadString();
            }

            public SerializedStruct1(bool myBool, int myInt, string myString)
            {
                MyBool = myBool;
                MyInt = myInt;
                MyString = myString;
            }

            public void SaveTo(BinaryWriter writer)
            {
                writer.Write(MyBool);
                writer.Write(MyInt);
                writer.Write(MyString);
            }
        }
        #endregion
    }
}
