using Lumen.Core.Domain.Enums;
using Lumen.Core.Domain.ServerMessages.Abstractions;

namespace Lumen.Core.Domain.ServerMessages;

public class KeepAliveServerMessage : IServerMessage
{
    public ServerMessageType Type => ServerMessageType.KeepAlive;

    public string Event => "ping";

    public string Data => "keep_alive";
}