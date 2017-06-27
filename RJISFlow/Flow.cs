using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RJISFlow
{
    class Flow
    {
        public Flow(string line)
        {
            if (line[0] != 'R' && line[1] != 'F' && line.Length != 49)
            {
                throw new Exception("Bad flow line");
            }
            FlowKey = line.Substring(2, 8);
        }
        public string FlowKey { get; private set; }

        public string GetOrigin => FlowKey.Substring(0, 4);
        public string GetDestination => FlowKey.Substring(4, 4);

        public Flow GetReversedFlow() => new Flow("RF" + FlowKey.Substring(4, 4) + FlowKey.Substring(0, 4));
    }
}
