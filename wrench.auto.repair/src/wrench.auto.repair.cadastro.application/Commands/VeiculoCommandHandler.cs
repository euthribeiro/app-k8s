using AutoMapper;
using MediatR;
using wrench.auto.repair.cadastro.domain.Data;
using wrench.auto.repair.cadastro.domain.Entities;
using wrench.auto.repair.core.Errors;

namespace wrench.auto.repair.cadastro.application.Commands
{
    public class VeiculoCommandHandler(
        IMapper _mapper,
        IClienteRepository _clienteRepository,
        IVeiculoRepository _veiculoRepository
    ) : IRequestHandler<CadastrarVeiculoCommand, Result<Guid>>,
        IRequestHandler<AtualizarVeiculoCommand, Result>
    {
        public async Task<Result<Guid>> Handle(CadastrarVeiculoCommand request, CancellationToken cancellationToken)
        {
            if (!request.EhValido())
                return Result<Guid>.ValidationError(request.ObterErros());

            var veiculo = await _veiculoRepository.ObterVeiculoPelaPlacaAsync(request.PlacaDoVeiculo, cancellationToken);

            if (veiculo != null)
                return Result<Guid>.Conflicted("A placa informada já está cadastrado para outro veículo.");

            var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);

            if (cliente == null) return Result<Guid>.NotFound("Cliente não encontrado.");

            var novoVeiculo = new Veiculo(cliente.Id, request.Marca, request.Modelo,
                                          request.Cor, request.AnoFabricacao, request.AnoModelo,
                                          request.PlacaDoVeiculo, request.Descricao, request.UltimaRevisao,
                                          request.QuilometragemAtual, DateTime.UtcNow);

            await _veiculoRepository.Adicionar(novoVeiculo, cancellationToken);

            var alteracoesRegistradas = await _veiculoRepository.UnitOfWork.CommitAsync();

            if (!alteracoesRegistradas)
                return Result<Guid>.Unexpected("Não foi possível cadastrar os dados do veículo. Por favor tente novamente");

            return Result<Guid>.Created(novoVeiculo.Id);
        }

        public async Task<Result> Handle(AtualizarVeiculoCommand request, CancellationToken cancellationToken)
        {
            var veiculo = await _veiculoRepository
                .ObterPorIdAsync(request.VeiculoId, cancellationToken);

            if (veiculo == null)
                return Result.NotFound("Veículo não encontrado");

            if (veiculo.ClienteId != request.ClienteId)
            {
                var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);

                if (cliente == null) return Result.NotFound("Cliente não encontrado");

                veiculo.AlterarCliente(cliente);
            }

            if (veiculo.PlacaDoVeiculo != request.PlacaDoVeiculo)
            {
                var veiculoExistente = await _veiculoRepository
                    .ObterVeiculoPelaPlacaAsync(request.PlacaDoVeiculo, cancellationToken);

                if (veiculoExistente != null)
                    return Result.Conflicted("A placa informada já está cadastrado para outro veículo.");

                veiculo.CorrigirPlacaVeiculo(request.PlacaDoVeiculo);
            }

            veiculo.AtualizarDescricao(request.Descricao);

            if (veiculo.Marca != request.Marca)
                veiculo.CorrigirMarca(request.Marca);

            if (veiculo.Modelo != request.Modelo)
                veiculo.CorrigirModelo(request.Modelo);

            if (veiculo.AnoFabricacao != request.AnoFabricacao)
                veiculo.CorrigirAnoFabricacao(request.AnoFabricacao);

            if (veiculo.AnoModelo != request.AnoModelo)
                veiculo.CorrigirAnoModelo(request.AnoModelo);

            if (veiculo.QuilometragemAtual != request.QuilometragemAtual)
            {
                if (veiculo.QuilometragemAtual > request.QuilometragemAtual)
                    veiculo.CorrigirQuilometragem(request.QuilometragemAtual);
                else
                    veiculo.AtualizarQuilometragem(request.QuilometragemAtual);
            }

            if (veiculo.UltimaRevisao != request.UltimaRevisao &&
                request.UltimaRevisao.HasValue)
            {
                if (veiculo.UltimaRevisao > request.UltimaRevisao.Value)
                    return Result.ValidationError("A data da última revisão é menor que a data da revisão anterior");

                veiculo.AtualizarUltimaRevisao(request.UltimaRevisao.Value);
            }

            await _veiculoRepository.Atualizar(veiculo);

            var alteracoesRegistradas = await _veiculoRepository.UnitOfWork.CommitAsync();

            if (!alteracoesRegistradas)
                return Result.Unexpected("Não foi possível atualizar os dados do veículo. Por favor tente novamente");

            return Result.NoContent();
        }
    }
}
