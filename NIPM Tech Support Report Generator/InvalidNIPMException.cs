using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NIPM_Tech_Support_Report_Generator
{
    class InvalidNIPMException : Exception
    {
        //exception to be thrown when NIPM is not installed on the system
        public InvalidNIPMException()
        {
        }
    }
}
