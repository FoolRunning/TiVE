using System;
using System.Diagnostics;

namespace ProdigalSoftware.TiVE.Core
{
    internal sealed class GameObjectManager
    {
        private ObjectEntry[] objects;

        public GameObjectManager(int initialCapacity)
        {
            objects = new ObjectEntry[initialCapacity];
            for (int i = 0; i < objects.Length; i++)
                objects[i] = new ObjectEntry(new Handle(0, 0, i));
        }

        public GameObject CreateNewGameObject()
        {
            int foundIndex = -1;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].Object == null)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex == -1)
            {
                foundIndex = objects.Length;
                EnlargeObjectList();
            }

            ObjectEntry entry = objects[foundIndex];
            entry.Handle = entry.Handle.IncrementCounter();
            GameObject newObj = new GameObject(entry.Handle);
            entry.Object = newObj;
            return newObj;
        }

        public GameObject Get(Handle handle)
        {
            Debug.Assert(handle.Index < objects.Length);

            ObjectEntry entry = objects[handle.Index];
            return (entry.Handle == handle) ? entry.Object : null;
        }

        public void Delete(GameObject obj)
        {
            Delete(obj.Handle);
        }

        public void Delete(Handle handle)
        {
            Debug.Assert(handle.Index < objects.Length);

            ObjectEntry entry = objects[handle.Index];
            if (entry.Handle == handle)
                entry.Object = null;

            // TODO: tell object it was deleted
        }

        private void EnlargeObjectList()
        {
            int prevSize = objects.Length;
            Array.Resize(ref objects, objects.Length + (objects.Length * 2 / 3) + 1);
            for (int i = prevSize; i < objects.Length; i++)
                objects[i] = new ObjectEntry(new Handle(0, 0, i));
        }

        private sealed class ObjectEntry
        {
            public Handle Handle;
            public GameObject Object;

            public ObjectEntry(Handle handle)
            {
                Handle = handle;
            }
        }
    }
}
