using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroNull.Hue.Verbs
{
    public class VerbInfo
    {
        public string VerbName { get; set; }
        public Type OptionsType { get; set; }
        public Type HandlerType { get; set; }
    }
}
