namespace wrench.auto.repair.core.Errors
{
    public class Result<T>
    {
        public Guid Id { get; private set; }
        public DateTime Timestamp { get; private set; }
        public bool Sucesso { get; private set; }
        public T? Valor { get; private set; }
        public IEnumerable<string> Erros { get; private set; } = [];
        public TipoErroEnum? TipoErro { get; private set; }
        public ResultadoStatusEnum? ResultadoStatus { get; private set; }

        private Result(TipoErroEnum? tipoErro, IEnumerable<string> erros)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Sucesso = false;
            Valor = default;
            Erros = erros;
            TipoErro = tipoErro;
            ResultadoStatus = default;
        }

        private Result(ResultadoStatusEnum resultadoStatus, T? valor)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Sucesso = true;
            Valor = valor;
            Erros = [];
            TipoErro = default;
            ResultadoStatus = resultadoStatus;
        }

        public static Result<T> Ok(T valor) =>
            new(ResultadoStatusEnum.OK, valor);

        public static Result<T> Created(T valor) =>
            new(ResultadoStatusEnum.CRIADO, valor);

        public static Result<T> NoContent() =>
            new(ResultadoStatusEnum.SEM_CONTEUDO, default);

        public static Result<T> NotFound(params string[] errors) =>
            new(TipoErroEnum.NAO_ENCONTRADO, [.. errors]);

        public static Result<T> Unauthorized(params string[] errors) =>
            new(TipoErroEnum.NAO_AUTORIZADO, [.. errors]);

        public static Result<T> Unexpected(params string[] errors) =>
            new(TipoErroEnum.INESPERADO, [.. errors]);

        public static Result<T> Conflicted(params string[] errors) =>
            new(TipoErroEnum.CONFLITO, [.. errors]);

        public static Result<T> ValidationError(params string[] errors) =>
            new(TipoErroEnum.VALIDACAO, [.. errors]);

        public static Result<T> UnprocessableEntity(params string[] errors) =>
            new(TipoErroEnum.ENTIDADE_NAO_PROCESSAVEL, [.. errors]);

        public static Result<T> Forbidden(params string[] errors) =>
            new(TipoErroEnum.PROIBIDO, [.. errors]);

        public static Result<T> Custom(TipoErroEnum tipoErro, params string[] errors) =>
            new(tipoErro, [.. errors]);

        public static Result<T> NotFound(IEnumerable<string> errors) =>
            new(TipoErroEnum.NAO_ENCONTRADO, [.. errors]);

        public static Result<T> Unauthorized(IEnumerable<string> errors) =>
            new(TipoErroEnum.NAO_AUTORIZADO, [.. errors]);

        public static Result<T> Unexpected(IEnumerable<string> errors) =>
            new(TipoErroEnum.INESPERADO, [.. errors]);

        public static Result<T> Conflicted(IEnumerable<string> errors) =>
            new(TipoErroEnum.CONFLITO, [.. errors]);

        public static Result<T> ValidationError(IEnumerable<string> errors) =>
            new(TipoErroEnum.VALIDACAO, [.. errors]);

        public static Result<T> UnprocessableEntity(IEnumerable<string> errors) =>
            new(TipoErroEnum.ENTIDADE_NAO_PROCESSAVEL, [.. errors]);

        public static Result<T> Forbidden(IEnumerable<string> errors) =>
            new(TipoErroEnum.PROIBIDO, [.. errors]);

        public static Result<T> Custom(TipoErroEnum tipoErro, IEnumerable<string> errors) =>
            new(tipoErro, [.. errors]);
    }
}
