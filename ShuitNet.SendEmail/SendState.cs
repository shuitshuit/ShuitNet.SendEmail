using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuitNet.SendEmail
{
    public enum  SendState
    {
        Success,
        NonDelivery,
        Rejected,
        SizeExceeded,
        Timeout,
        NotAuthenticated,
        ConnectionError,
        ServerError,
        ClientError,
        RecipientError,
        AccessDenied,
        Error,
    }
}
