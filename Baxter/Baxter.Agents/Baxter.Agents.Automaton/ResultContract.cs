using System;
using System.Runtime.Serialization;

namespace Baxter.Agents.Automaton
{
    [DataContract]
    public class ResultContract
    {
        [DataMember]
        public Int32 Added { get; protected set; }

        [DataMember]
        public bool Errors { get; protected set; }

        [DataMember]
        public Int32 Processed { get; protected set; }

        [DataMember]
        public Int32 Updated { get; protected set; }
    }
}