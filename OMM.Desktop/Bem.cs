using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop
{
    public class Bem
    {
        private string baseName;
        public Bem(string baseName)
        {
            this.baseName = baseName;
        }

        public string Base(string flag = null)
        {
            if (flag is null)
                return baseName;

            return baseName + " " + baseName + "--" + flag;
        }

        public string Element(string element, string flag = null)
        {
            if (string.IsNullOrWhiteSpace(element))
                return "";

            if (flag is null)
                return baseName + "__" + element;

            return baseName + "__" + element + " " + baseName + "__" + element + "--" + flag;
        }
    }
}
