using System.Diagnostics;

namespace ProdigalSoftware.TiVE.Core
{
    /// <summary>
    /// Represents a handle to a game object
    /// </summary>
    internal struct Handle
    {
        #region Constants/Member variable
        private const int TypeBitMask = 0x3FF; // 10 bits = 1024 values
        private const int CounterBitMask = 0x1F; // 5 bits = 32 values
        private const int IndexBitMask = 0xFFFF; // 16 bits = 65536 values

        private const int TypeBitOffset = 21;
        private const int CounterBitOffset = 16;
        private const int IndexBitOffset = 0;

        private readonly int data;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new handle from the specified integer value
        /// </summary>
        private Handle(int data)
        {
            this.data = data;
        }

        /// <summary>
        /// Creates a new handle for the specified values
        /// </summary>
        public Handle(int type, int counter, int index)
        {
            Debug.Assert(type <= TypeBitMask);
            Debug.Assert(counter <= CounterBitMask);
            Debug.Assert(index <= IndexBitMask);

            data = type << TypeBitOffset | counter << CounterBitOffset | index << IndexBitOffset;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type from the handle
        /// </summary>
        public int Type
        {
            get { return (data >> TypeBitOffset) & TypeBitMask; }
        }

        /// <summary>
        /// Gets the counter from the handle
        /// </summary>
        public int Counter
        {
            get { return (data >> CounterBitOffset) & CounterBitMask; }
        }

        /// <summary>
        /// Gets the index from the handle
        /// </summary>
        public int Index
        {
            get { return (data >> IndexBitOffset) & IndexBitMask; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns a new handle with the counter of this handle incremented by one. 
        /// This also handles the counter wrapping back to zero when passing it's max value.
        /// </summary>
        public Handle IncrementCounter()
        {
            int nextCounter = (Counter + 1) % (CounterBitMask + 1); // Let counter wrap when over max value
            int newData = data;
            newData &= ~(CounterBitMask << CounterBitOffset);  // Clear current counter from data
            newData |= nextCounter << CounterBitOffset; // Put new counter in data
            return new Handle(newData);
        }
        #endregion

        #region Overrides of object
        public override bool Equals(object obj)
        {
            return obj is Handle && ((Handle)obj).data == data;
        }

        public override int GetHashCode()
        {
            return data;
        }

        public override string ToString()
        {
            return string.Format("Handle T:{0} C:{1} I:{2}", Type, Counter, Index);
        }
        #endregion

        #region Operator overloads
        /// <summary>
        /// Allows a handle to be created from an integer without casting
        /// </summary>
        public static implicit operator Handle(int val)
        {
            return new Handle(val);
        }

        /// <summary>
        /// Allows a handle to be turned into an integer without casting
        /// </summary>
        public static implicit operator int(Handle handle)
        {
            return handle.data;
        }

        public static bool operator ==(Handle h1, Handle h2)
        {
            return h1.data == h2.data;
        }

        public static bool operator !=(Handle h1, Handle h2)
        {
            return h1.data != h2.data;
        }
        #endregion
    }
}
