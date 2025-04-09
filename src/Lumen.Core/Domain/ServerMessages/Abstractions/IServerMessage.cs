using Lumen.Core.Domain.Enums;

namespace Lumen.Core.Domain.ServerMessages.Abstractions;

public interface IServerMessage
{
    ServerMessageType Type { get; }

    string Event { get; }

    string Data { get; }
}