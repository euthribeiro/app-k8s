using System.Collections.Concurrent;
using System.Reflection;
using wrench.auto.repair.core.Services;

namespace wrench.auto.repair.infra.Services.Email
{
    /// <summary>
    /// Lê templates de e-mail embutidos no assembly (EmbeddedResource) e substitui placeholders {{Chave}}.
    /// O template bruto é cacheado; a substituição é feita sobre uma cópia a cada chamada.
    /// </summary>
    public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private static readonly Assembly _assembly = typeof(EmailTemplateRenderer).Assembly;
        private static readonly ConcurrentDictionary<string, string> _cache = new();

        public string Render(string templateName, IReadOnlyDictionary<string, string?> valores)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
            ArgumentNullException.ThrowIfNull(valores);

            var template = _cache.GetOrAdd(templateName, CarregarTemplate);

            foreach (var (chave, valor) in valores)
            {
                template = template.Replace($"{{{{{chave}}}}}", valor ?? string.Empty);
            }

            return template;
        }

        private static string CarregarTemplate(string templateName)
        {
            var sufixo = $".{templateName}.html";

            var resourceName = Array.Find(
                _assembly.GetManifestResourceNames(),
                nome => nome.EndsWith(sufixo, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"Template de e-mail '{templateName}' não encontrado nos recursos embutidos do assembly.");

            using var stream = _assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Não foi possível abrir o recurso embutido '{resourceName}'.");
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
