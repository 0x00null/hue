using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroNull.Hue.Tests
{
    public class TestLogger
    {
        public static ILogger Instance
        {
            get;
            private set;
        }

        static TestLogger()
        {
            Instance = new LoggerConfiguration()
                .WriteTo.Debug()
                .CreateLogger();
        }
    
    }
}
