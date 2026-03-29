using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using System.Net;

namespace doylib.Models;

public class ActiveTradeException(
    string message,
    HttpStatusCode statusCode,
    ExceptionSeverityLevel severity) : DoyVestmentException(message, statusCode, severity);
