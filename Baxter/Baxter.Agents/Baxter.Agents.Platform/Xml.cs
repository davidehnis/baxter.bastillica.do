using System;

namespace Baxter.Agents.Platform
{
    public class Xml
    {
        protected string _xml = string.Empty;

        public static Xml Create()
        {
            return new Platform.Xml();
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}