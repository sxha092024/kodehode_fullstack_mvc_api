using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BloggoApi.Contexts.Converters;

public class DateTimeAsUtcValueConverter()
    : ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
    );
