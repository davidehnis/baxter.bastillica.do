using System;
using System.Runtime.Serialization;

namespace Baxter.Vector.Machine
{
    public class Node : ISerializable
    {
        public Node()
        {
            Index = 0;
            Value = 0;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public int Index { get; }

        public double Value { get; }
    }
}
