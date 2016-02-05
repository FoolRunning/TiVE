using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ProdigalSoftware.TiVE.Starter;
using ProdigalSoftware.TiVEPluginFramework;
using ProdigalSoftware.Utils;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class TiVESerializerImplementation : ITiVESerializerImpl
    {
        #region Constants/member variables
        private static readonly Type serializableType = typeof(ITiVESerializable);
        private static readonly Type[] constructorParams = { typeof(BinaryReader) };
        private static readonly byte[] msClrToken = { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };
        private static readonly byte[] msFxToken = { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a };

        private readonly Dictionary<Guid, Delegate> objectConstructors = new Dictionary<Guid, Delegate>();
        private readonly Dictionary<Type, Guid> typeIds = new Dictionary<Type, Guid>();
        private readonly byte[] guidBytesStorage = new byte[16];
        #endregion

        #region Implementation of ITiVESerializerImpl
        public void Serialize(ITiVESerializable obj, BinaryWriter writer)
        {
            Guid guid = typeIds[obj.GetType()];
            writer.Write(guid.ToByteArray());
            obj.SaveTo(writer);
        }

        public T Deserialize<T>(BinaryReader reader) where T : ITiVESerializable
        {
            reader.Read(guidBytesStorage, 0, guidBytesStorage.Length);
            return ((Func<BinaryReader, T>)objectConstructors[new Guid(guidBytesStorage)])(reader);
        }
        #endregion

        #region Other public methods
        public bool Initialize()
        {
            Messages.Print("Initializing object serializer...");
            List<string> errors = new List<string>();
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsFrameworkAssembly(a)))
            {
                foreach (Type type in asm.ExportedTypes.Where(t => !t.IsAbstract))
                {
                    if (serializableType.IsAssignableFrom(type))
                    {
                        FieldInfo idFieldInfo = type.GetField("ID");
                        if (idFieldInfo == null)
                        {
                            errors.Add("Serializable type " + type.Name + " does not have an ID field of type Guid");
                            continue;
                        }

                        Delegate constructor = CreateConstructorDelegate(type);
                        if (constructor == null)
                        {
                            errors.Add("Serializable type " + type.Name + " does not have a constructor that take a BinaryReader");
                            continue;
                        }

                        Guid guid = (Guid)idFieldInfo.GetValue(type);
                        objectConstructors.Add(guid, constructor);
                        typeIds.Add(type, guid);
                    }
                }
            }

            if (errors.Count == 0)
                Messages.AddDoneText();
            else
            {
                Messages.AddFailText();
                foreach (string error in errors)
                    Messages.AddError(error);
                return false;
            }
            return true;
        }
        #endregion

        #region Private helper methods
        /// <summary>
        /// Determines if the specified assembly is a .Net framework assembly.
        /// </summary>
        /// <remarks>.Net assembly public key tokens taken from http://blogs.msdn.com/b/shawnfa/archive/2004/06/07/150378.aspx</remarks>
        private static bool IsFrameworkAssembly(Assembly asm)
        {
            byte[] asmToken = asm.GetName().GetPublicKeyToken();
            return ArrayUtils.AreEqual(asmToken, msClrToken) || ArrayUtils.AreEqual(asmToken, msFxToken);
        }

        /// <summary>
        /// Most of this brilliant code is taken from http://stackoverflow.com/questions/1600712/a-constructor-as-a-delegate-is-it-possible-in-c
        /// <para>Changes are to make it work with structs and to handle error conditions</para>
        /// </summary>
        private static Delegate CreateConstructorDelegate(Type type)
        {
            ConstructorInfo ctor = type.GetConstructor(constructorParams);
            if (ctor == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(BinaryReader), "val");
            LambdaExpression lambda = Expression.Lambda(Expression.New(ctor, param), param);
            return lambda.Compile();
        }
        #endregion
    }
}
