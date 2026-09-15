namespace wrench.auto.repair.core.Services
{
    /// <summary>
    /// Renderiza um template de e-mail (HTML) substituindo os placeholders no formato {{Chave}}.
    /// A origem física do template (recurso embutido, arquivo, etc.) é um detalhe da implementação na infraestrutura.
    /// </summary>
    public interface IEmailTemplateRenderer
    {
        string Render(string templateName, IReadOnlyDictionary<string, string?> valores);
    }
}
