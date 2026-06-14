using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Infrastructure.Persistence.ValueConverters;

public class DocumentCollectionConverter : ValueConverter<List<DocumentReference>, string>
{
    public DocumentCollectionConverter()
        : base(
            v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
            v => JsonSerializer.Deserialize<List<DocumentReference>>(v, JsonSerializerOptions.Default) ?? new List<DocumentReference>())
    {
    }
}
