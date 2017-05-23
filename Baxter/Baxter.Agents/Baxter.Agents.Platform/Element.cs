using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Agents.Platform
{
    public class Element : IEquatable<Element>, IEnumerable<Element>
    {
        public bool Equals(Element other)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Element> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            string temp = "1291020100019201";
            return temp.GetHashCode();
        }
    }
}