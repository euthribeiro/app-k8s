namespace Wrench.Contracts;

/// <summary>
/// Contrato comum das mensagens da Saga: toda mensagem identifica a ordem de serviço
/// que coordena e carrega a correlação que liga os três microsserviços no mesmo trace.
/// </summary>
public interface ISagaMensagem
{
    Guid OrdemServicoId { get; }

    string CorrelationId { get; }
}
