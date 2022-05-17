using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivHelper {
    public class InvalidPropertyException : Exception {

        public InvalidPropertyException() : base() { }
        public InvalidPropertyException(string message) : base(message) { }
        public InvalidPropertyException(string message, Exception inner) : base(message, inner) { }
    }
}
