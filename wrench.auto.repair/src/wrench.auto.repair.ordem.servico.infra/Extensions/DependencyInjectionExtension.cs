using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using wrench.auto.repair.ordem.servico.domain.Saga;
using wrench.auto.repair.ordem.servico.domain.Saga.Mensageria;
using wrench.auto.repair.ordem.servico.domain.Data;
using wrench.auto.repair.ordem.servico.infra.Repositories;
using wrench.auto.repair.ordem.servico.infra.Saga;
using wrench.auto.repair.ordem.servico.infra.Saga.Mensageria;

namespace wrench.auto.repair.ordem.servico.infra.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddOrdemServicoInfra(this IServiceCollection services)
        {
            services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
            services.AddScoped<ISagaOrdemServicoRepository, SagaOrdemServicoRepository>();
            services.AddScoped<IOutboxSaga, OutboxSaga>();
            services.AddScoped<IInboxSaga, InboxSaga>();

            return services;
        }

        /// <summary>
        /// Transporte da Saga: clientes AWS, publicador, consumidor da fila, dispatcher do
        /// outbox e varredura de timeouts. Sem <c>Sqs:QueueUrl</c> o consumidor nao sobe, o que
        /// permite rodar a API localmente sem mensageria.
        /// </summary>
        public static IServiceCollection AddSagaMensageria(this IServiceCollection services, IConfiguration configuracao)
        {
            services.Configure<SnsOptions>(configuracao.GetSection(SnsOptions.Secao));
            services.Configure<SqsOptions>(configuracao.GetSection(SqsOptions.Secao));
            services.Configure<OutboxOptions>(configuracao.GetSection(OutboxOptions.Secao));
            services.Configure<SagaOptions>(configuracao.GetSection(SagaOptions.Secao));

            var sns = configuracao.GetSection(SnsOptions.Secao).Get<SnsOptions>() ?? new SnsOptions();
            var sqs = configuracao.GetSection(SqsOptions.Secao).Get<SqsOptions>() ?? new SqsOptions();
            var regiaoPadrao = configuracao["AWS:Region"];

            services.TryAddSingleton(TimeProvider.System);

            services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
            {
                var config = new AmazonSimpleNotificationServiceConfig();
                Configurar(config, sns.ServiceUrl, sns.Region ?? regiaoPadrao);

                return string.IsNullOrWhiteSpace(sns.ServiceUrl)
                    ? new AmazonSimpleNotificationServiceClient(config)
                    : new AmazonSimpleNotificationServiceClient(new BasicAWSCredentials("local", "local"), config);
            });

            services.AddSingleton<IAmazonSQS>(_ =>
            {
                var config = new AmazonSQSConfig();
                Configurar(config, sqs.ServiceUrl, sqs.Region ?? regiaoPadrao);

                return string.IsNullOrWhiteSpace(sqs.ServiceUrl)
                    ? new AmazonSQSClient(config)
                    : new AmazonSQSClient(new BasicAWSCredentials("local", "local"), config);
            });

            services.AddScoped<IPublicadorSaga, SnsPublicadorSaga>();
            services.AddScoped<PublicacaoOutbox>();

            services.AddHostedService<OutboxDispatcher>();
            services.AddHostedService<SqsSagaConsumer>();
            services.AddHostedService<SagaTimeoutWorker>();

            return services;
        }

        private static void Configurar(ClientConfig config, string? serviceUrl, string? regiao)
        {
            if (!string.IsNullOrWhiteSpace(serviceUrl)) config.ServiceURL = serviceUrl;

            if (!string.IsNullOrWhiteSpace(regiao)) config.RegionEndpoint = RegionEndpoint.GetBySystemName(regiao);
            else if (!string.IsNullOrWhiteSpace(serviceUrl)) config.AuthenticationRegion = "us-east-1";
        }
    }
}
