using System;

namespace Blish_HUD._Extensions {
    internal static class ExceptionExtensions {

        public static Exception GetBaseException(this Exception exception) {
            return exception.InnerException == null 
                       ? exception 
                       : exception.InnerException.GetBaseException();
        }

    }
}
