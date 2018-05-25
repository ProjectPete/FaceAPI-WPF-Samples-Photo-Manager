using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
    using System;
    using System.ServiceModel;
    using System.Threading;

    public class Retry
    {
        static Retry _singleton;

        private Retry(){}

        public static Retry Current
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new Retry();
                }
                return _singleton;
            }
        }

        public TResult OperationWithBasicRetryAsync<TResult>(Func<TResult> func, Type[] transientExceptionTypes, int retryDelayMilliseconds = 1000, int maxRetries = 60)
        {
            int retryCount = 0;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                    when (IsTransientError(ex, transientExceptionTypes))
                {
                    if (++retryCount >= maxRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(retryDelayMilliseconds);
                }
            }
        }

        private static bool IsTransientError(Exception ex, Type[] transientExceptionTypes)
        {
            return transientExceptionTypes.Contains(ex.GetType());
        }
    }
}
