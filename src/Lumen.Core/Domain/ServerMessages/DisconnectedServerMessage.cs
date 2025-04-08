using Lumen.Core.Domain.Enums;
using Lumen.Core.Domain.ServerMessages.Abstractions;

namespace Lumen.Core.Domain.ServerMessages;

public class DisconnectedServerMessage : IServerMessage
{
    public ServerMessageType Type => ServerMessageType.Disconnected;

    public string Event => "disconnect";

    public string Data => "The server is shutting down.";
}