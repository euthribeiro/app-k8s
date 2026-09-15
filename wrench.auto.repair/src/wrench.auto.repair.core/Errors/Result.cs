namespace wrench.auto.repair.core.Errors
{
    public class Result
    {
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public bool Sucesso { get; private set; }
        public IEnumerable<string> Erros { get; private set; } = [];
        public TipoErroEnum? TipoErro { get; private set; }
        public ResultadoStatusEnum? ResultadoStatus { get; private set; }

        private Result(TipoErroEnum? tipoErro, IEnumerable<string> erros)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Sucesso = false;
            Erros = erros;
            TipoErro = tipoErro;
            ResultadoStatus = default;
        }

        private Result(ResultadoStatusEnum resultadoStatus)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Sucesso = true;
            Erros = [];
            TipoErro = default;
            ResultadoStatus = resultadoStatus;
        }

        public static Result Ok() =>
            new(ResultadoStatusEnum.OK);

        public static Result Created() =>
            new(ResultadoStatusEnum.CRIADO);

        public static Result NoContent() =>
            new(ResultadoStatusEnum.SEM_CONTEUDO);

        public static Result NotFound(params string[] errors) =>
            new(TipoErroEnum.NAO_ENCONTRADO, [.. errors]);

        public static Result Unauthorized(params string[] errors) =>
            new(TipoErroEnum.NAO_AUTORIZADO, [.. errors]);

        public static Result Unexpected(params string[] errors) =>
            new(TipoErroEnum.INESPERADO, [.. errors]);

        public static Result Conflicted(params string[] errors) =>
            new(TipoErroEnum.CONFLITO, [.. errors]);

        public static Result ValidationError(params string[] errors) =>
            new(TipoErroEnum.VALIDACAO, [.. errors]);

        public static Result UnprocessableEntity(params string[] errors) =>
            new(TipoErroEnum.ENTIDADE_NAO_PROCESSAVEL, [.. errors]);

        public static Result Forbidden(params string[] errors) =>
            new(TipoErroEnum.PROIBIDO, [.. errors]);

        public static Result NotFound(IEnumerable<string> errors) =>
            new(TipoErroEnum.NAO_ENCONTRADO, [.. errors]);

        public static Result Unauthorized(IEnumerable<string> errors) =>
            new(TipoErroEnum.NAO_AUTORIZADO, [.. errors]);

        public static Result Unexpected(IEnumerable<string> errors) =>
            new(TipoErroEnum.INESPERADO, [.. errors]);

        public static Result Conflicted(IEnumerable<string> errors) =>
            new(TipoErroEnum.CONFLITO, [.. errors]);

        public static Result ValidationError(IEnumerable<string> errors) =>
            new(TipoErroEnum.VALIDACAO, [.. errors]);

        public static Result UnprocessableEntity(IEnumerable<string> errors) =>
       new(TipoErroEnum.ENTIDADE_NAO_PROCESSAVEL, [.. errors]);

        public static Result Forbidden(IEnumerable<string> errors) =>
            new(TipoErroEnum.PROIBIDO, [.. errors]);
    }
}
