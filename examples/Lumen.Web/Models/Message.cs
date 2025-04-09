using Newtonsoft.Json;

namespace Lumen.Web.Models;

public record Message(
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("body")] string Body
);