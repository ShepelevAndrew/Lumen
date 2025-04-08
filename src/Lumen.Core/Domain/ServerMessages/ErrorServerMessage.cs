using Lumen.Core.Domain.Enums;
using Lumen.Core.Domain.ServerMessages.Abstractions;

namespace Lumen.Core.Domain.ServerMessages;

public class ErrorServerMessage(string errorMessage) : IServerMessage
{
    public ServerMessageType Type => ServerMessageType.Error;

    public string Event => "error";

    public string Data => errorMessage;
}