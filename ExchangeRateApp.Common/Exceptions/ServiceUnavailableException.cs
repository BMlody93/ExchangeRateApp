using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateApp.Common.Exceptions
{
    public class ServiceUnavailableException : GeneralException
    {
        public ServiceUnavailableException(string serviceName, string? details = null, Exception? inner = null)
            : base($"Service '{serviceName}' is unavailable. {details}", inner) { }
    }
}
