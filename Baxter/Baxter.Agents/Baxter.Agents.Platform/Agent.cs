using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Agents.Platform
{
    public class Agent : Element
    {
        public DateTime DateTime { get; protected set; }

        public string Description { get; protected set; }

        public string Error { get; protected set; }

        public string File { get; protected set; }

        public IEnumerable<Language> Languages { get; protected set; }

        public DateTime Lease { get; protected set; }

        public string Name { get; protected set; }

        public IEnumerable<Node> Nodes { get; protected set; }

        public IEnumerable<Ontology> Ontologies { get; protected set; }

        public IEnumerable<Protocol> Protocols { get; protected set; }

        public void Add(Language lan)
        {
        }

        public void Add(Protocol protocol)
        {
        }

        public void Add(Service service)
        {
        }

        public void ChangeLease(DateTime dt)
        {
        }

        public Xml Parse()
        {
            return Xml.Create();
        }
    }
}