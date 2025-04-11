using Newtonsoft.Json;

namespace Lumen.MinimalApi.Models;

public record Message(
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("body")] string Body
);