using Lumen.Core.Domain.Enums;
using Lumen.Core.Domain.ServerMessages.Abstractions;

namespace Lumen.Core.Domain.ServerMessages;

public class ConnectedServerMessage : IServerMessage
{
    public ServerMessageType Type => ServerMessageType.Connected;

    public string Event => "open";

    public string Data => "The server is connected.";
}