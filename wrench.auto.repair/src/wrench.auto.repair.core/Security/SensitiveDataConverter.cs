using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace wrench.auto.repair.core.Security
{
    public class SensitiveDataConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == typeof(string))
                return false;

            if (!typeToConvert.IsClass)
                return false;

            return typeToConvert
                .GetProperties()
                .Any(p => Attribute.IsDefined(p, typeof(SensitiveDataAttribute)));
        }

        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            writer.WriteStartObject();

            foreach (var prop in type.GetProperties())
            {
                var propValue = prop.GetValue(value);
                var attr = prop.GetCustomAttribute<SensitiveDataAttribute>();

                writer.WritePropertyName(prop.Name);

                if (attr != null && propValue is string str)
                {
                    writer.WriteStringValue(Mask(str, attr.Type));
                }
                else
                {
                    JsonSerializer.Serialize(writer, propValue, options);
                }
            }

            writer.WriteEndObject();
        }

        private string Mask(string value, SensitiveDataType type)
        {
            return type switch
            {
                SensitiveDataType.CpfCnpj => MaskCpfCnpj(value),
                SensitiveDataType.PlacaVeiculo => MaskPlacaVeiculo(value),
                _ => value
            };
        }

        public static string MaskCpfCnpj(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var digits = new string(value.Where(char.IsDigit).ToArray());

            if (digits.Length == 11)
            {
                return $"{digits[..3]}.***.***-{digits[^2..]}";
            }

            if (digits.Length == 14)
            {
                return $"{digits[..2]}.***.***/****-{digits[^2..]}";
            }

            return value;
        }

        public static string MaskPlacaVeiculo(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa))
                return placa;

            var clean = placa.Replace("-", "").Replace(" ", "").ToUpper();

            if (clean.Length != 7)
                return placa;

            return $"{clean[..3]}***{clean.Last()}";
        }
    }
}
