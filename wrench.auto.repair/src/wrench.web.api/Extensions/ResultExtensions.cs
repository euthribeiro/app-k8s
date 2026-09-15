using Microsoft.AspNetCore.Mvc;
using wrench.auto.repair.core.Errors;

namespace wrench.web.api.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, string>? createdUri = null)
        {
            if (result.Sucesso)
            {
                return result.ResultadoStatus switch
                {
                    ResultadoStatusEnum.OK => new OkObjectResult(result.Valor),

                    ResultadoStatusEnum.CRIADO => createdUri is not null
                        ? new CreatedResult(createdUri(result.Valor!), result.Valor)
                        : new CreatedResult(string.Empty, result.Valor),

                    ResultadoStatusEnum.SEM_CONTEUDO => new NoContentResult(),

                    _ => new OkObjectResult(result.Valor)
                };
            }

            return result.TipoErro switch
            {
                TipoErroEnum.VALIDACAO => new BadRequestObjectResult(result.Erros),
                TipoErroEnum.NAO_ENCONTRADO => new NotFoundObjectResult(result.Erros),
                TipoErroEnum.CONFLITO => new ConflictObjectResult(result.Erros),
                TipoErroEnum.NAO_AUTORIZADO => new UnauthorizedObjectResult(result.Erros),
                TipoErroEnum.ENTIDADE_NAO_PROCESSAVEL => new UnprocessableEntityObjectResult(result.Erros),
                TipoErroEnum.INESPERADO => new ObjectResult(result.Erros) { StatusCode = 500 },
                TipoErroEnum.PROIBIDO => new ForbidResult(),
                _ => new ObjectResult(result.Erros) { StatusCode = 500 }
            };
        }

        public static IActionResult ToActionResult(this Result result, Func<string>? createdUri = null)
        {
            if (result.Sucesso)
            {
                return result.ResultadoStatus switch
                {
                    ResultadoStatusEnum.OK => new OkObjectResult(null),

                    ResultadoStatusEnum.CRIADO => createdUri is not null
                        ? new CreatedResult(createdUri(), null)
                        : new CreatedResult(string.Empty, null),

                    ResultadoStatusEnum.SEM_CONTEUDO => new NoContentResult(),

                    _ => new OkObjectResult(null)
                };
            }

            return result.TipoErro switch
            {
                TipoErroEnum.VALIDACAO => new BadRequestObjectResult(result.Erros),
                TipoErroEnum.NAO_ENCONTRADO => new NotFoundObjectResult(result.Erros),
                TipoErroEnum.CONFLITO => new ConflictObjectResult(result.Erros),
                TipoErroEnum.NAO_AUTORIZADO => new UnauthorizedObjectResult(result.Erros),
                TipoErroEnum.ENTIDADE_NAO_PROCESSAVEL => new UnprocessableEntityObjectResult(result.Erros),
                TipoErroEnum.INESPERADO => new ObjectResult(result.Erros) { StatusCode = 500 },
                TipoErroEnum.PROIBIDO => new ForbidResult(),
                _ => new ObjectResult(result.Erros) { StatusCode = 500 }
            };
        }
    }
}
