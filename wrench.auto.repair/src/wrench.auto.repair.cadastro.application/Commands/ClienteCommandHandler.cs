using AutoMapper;
using MediatR;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.cadastro.domain.ValueObjects;
using wrench.auto.repair.core.Errors;
using wrench.auto.repair.core.Mediator;
using wrench.auto.repair.core.Messages.CommonMessages.IntegrationEvents;
using wrench.auto.repair.core.ValueObjects;

namespace wrench.auto.repair.cadastro.application.Commands
{
    public class ClienteCommandHandler(
        IMapper _mapper,
        IMediatorHandler _mediatorHandler,
        IClienteRepository _clienteRepository
    ) : IRequestHandler<CadastrarClienteCommand, Result<Guid>>,
        IRequestHandler<AtualizarClienteCommand, Result>
    {
        public async Task<Result<Guid>> Handle(CadastrarClienteCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<Guid>.ValidationError(request.ObterErros());

            var cliente = await _clienteRepository.ObterPorDocumentAsync(request.Documento, cancellationToken);

            if (cliente != null)
                return Result<Guid>.Conflicted("Cliente já cadastrado.");

            var email = new Email(request.Email);

            var clienteEmail = await _clienteRepository.ObterPorEmailAsync(email, cancellationToken);

            if (clienteEmail != null)
                return Result<Guid>.Conflicted("E-mail já cadastrado para outro cliente.");

            var endereco = _mapper.Map<Endereco>(request.Endereco);

            var document = new CpfCnpj(request.Documento);
            var nomeCompleto = new NomeRazaoSocial(request.Nome);
            var telefone = new Telefone(request.Telefone);

            var novoCliente = new Cliente(document, nomeCompleto, telefone, email, endereco, DateTime.UtcNow);

            await _clienteRepository.Adicionar(novoCliente, cancellationToken);

            var alteracoesRegistradas = await _clienteRepository.UnitOfWork.CommitAsync();

            if (!alteracoesRegistradas)
                return Result<Guid>.Unexpected("Não foi possível cadastrar o cliente. Por favor tente novamente");

            await _mediatorHandler.PublicarEvento(new ClienteCadastradoEvent(novoCliente.Id, email.Endereco));

            return Result<Guid>.Created(novoCliente.Id);
        }

        public async Task<Result> Handle(AtualizarClienteCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result.ValidationError(request.ObterErros());

            var cliente = await _clienteRepository
                .ObterPorIdAsync(request.ClienteId, cancellationToken);

            if (cliente == null)
                return Result.NotFound("Cliente não encontrado");

            var nomeCliente = new NomeRazaoSocial(request.Nome);

            if (cliente.Nome != nomeCliente)
                cliente.AtualizarNome(nomeCliente);

            var telefoneCliente = new Telefone(request.Telefone);

            if (cliente.Telefone != telefoneCliente)
                cliente.AtualizarTelefone(telefoneCliente);

            var emailCliente = new Email(request.Email);

            if (cliente.Email != emailCliente)
                cliente.AtualizarEmail(emailCliente);

            var endereco = _mapper.Map<Endereco>(request.Endereco);

            if (endereco != cliente.Endereco)
                cliente.AtualizarEndereco(endereco);

            await _clienteRepository.Atualizar(cliente);

            var alteracoesSalvas = await _clienteRepository.UnitOfWork.CommitAsync();

            if (!alteracoesSalvas) return Result.Unexpected("Não foi possível atualizar o cliente.");

            return Result.NoContent();
        }
    }
}
