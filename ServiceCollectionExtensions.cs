using PayWall.NetCore.Configuration;
using PayWall.NetCore.Implementations;
using PayWall.NetCore.Models.Common;
using PayWall.NetCore.Services;

namespace PayWallDemo
{
    public static class ServiceCollectionExtensions
    {
        #region Public Properties
        public static string PaymentClientName => "PaymentApiClient";
        public static string PaymentPrivateClientName => "PaymentPrivateApiClient";
        public static string CardWallClientName => "CardWallApiClient";
        public static string MemberClientName => "MemberApiClient";
        #endregion

        #region Private Properties
        // Global
        private static Uri ProdPaymentApiUrlGCP => new("https://payment-api.itspaywall.com/api/paywall/");
        private static Uri TestPaymentApiUrlGCP => new("https://dev-payment-api.itspaywall.com/api/paywall/");
        private static Uri ProdPaymentPrivateApiUrlGCP => new("https://payment-private-api.itspaywall.com/api/paywall/");
        private static Uri TestPaymentPrivateApiUrlGCP => new("https://dev-payment-private-api.itspaywall.com/api/paywall/");
        private static Uri ProdCardWallApiUrlGCP => new("https://card-api.itspaywall.com/paywall/");
        private static Uri TestCardWallApiUrlGCP => new("https://dev-card-api.itspaywall.com/paywall/");
        private static Uri ProdMemberApiUrlGCP => new("https://member-api.itspaywall.com/api/paywall/");
        private static Uri TestMemberApiUrlGCP => new("https://dev-member-api.itspaywall.com/api/paywall/");


        // Turkey
        private static Uri ProdPaymentApiUrlHuawei => new("https://payment-api.paywall.com.tr/api/paywall/");
        private static Uri ProdPaymentPrivateApiUrlHuawei => new("https://payment-private-api.paywall.com.tr/api/paywall/");
        private static Uri ProdCardWallApiUrlHuawei => new("https://card-api.paywall.com.tr/paywall/");
        private static Uri ProdMemberApiUrlHuawei => new("https://member-api.paywall.com.tr/api/paywall/");
        #endregion

        #region Public Methods
        public static void AddPaywallService(this IServiceCollection services, IConfiguration config,
            params Func<IServiceProvider, DelegatingHandler>[] handlerFactories)
        {
            var payWallOptions = new PayWallOptions();

            config.GetSection("PayWall").Bind(payWallOptions);

            services
                .AddPaymentApiClient(payWallOptions, handlerFactories)
                .AddPaymentPrivateApiClient(payWallOptions, handlerFactories)
                .AddCardWallApiClient(payWallOptions, handlerFactories)
                .AddMemberApiClient(payWallOptions, handlerFactories)
                .AddTransient<PayWallService>();
        }
        #endregion

        #region Private Methods
        private static IServiceCollection AddPaymentApiClient(this IServiceCollection services,
            PayWallOptions payWallOptions, params Func<IServiceProvider, DelegatingHandler>[] handlerFactories)
        {
            if (payWallOptions == null) throw new ArgumentNullException(nameof(payWallOptions));

            var baseAddress = payWallOptions.DataCenter switch
            {
                DataCenter.Global => payWallOptions.Prod ? ProdPaymentApiUrlGCP : TestPaymentApiUrlGCP,
                DataCenter.Turkey => payWallOptions.Prod ? ProdPaymentApiUrlHuawei : throw new NotSupportedException("Huawei does not support test."),
                _ => throw new NotSupportedException($"'{payWallOptions.DataCenter}' is not supported.")
            };

            services.AddHttpClient(PaymentClientName, httpClient =>
            {
                httpClient.BaseAddress = baseAddress;
                httpClient.DefaultRequestHeaders.Add("apikeypublic", payWallOptions.PublicKey);
                httpClient.DefaultRequestHeaders.Add("apiclientpublic", payWallOptions.PublicClient);
                httpClient.DefaultRequestHeaders.Add("apikeyprivate", payWallOptions.PrivateKey);
                httpClient.DefaultRequestHeaders.Add("apiclientprivate", payWallOptions.PrivateClient);
            }).AddMultipleHttpMessageHandlers(handlerFactories);

            services.AddTransient<PaymentApiClient>();

            return services;
        }

        private static IServiceCollection AddPaymentPrivateApiClient(this IServiceCollection services,
            PayWallOptions payWallOptions, params Func<IServiceProvider, DelegatingHandler>[] handlerFactories)
        {
            if (payWallOptions == null) throw new ArgumentNullException(nameof(payWallOptions));

            var baseAddress = payWallOptions.DataCenter switch
            {
                DataCenter.Global => payWallOptions.Prod ? ProdPaymentPrivateApiUrlGCP : TestPaymentPrivateApiUrlGCP,
                DataCenter.Turkey => payWallOptions.Prod ? ProdPaymentPrivateApiUrlHuawei : throw new NotSupportedException("Huawei does not support test."),
                _ => throw new NotSupportedException($"'{payWallOptions.DataCenter}' is not supported.")
            };

            services.AddHttpClient(PaymentPrivateClientName, httpClient =>
            {
                httpClient.BaseAddress = baseAddress;
                httpClient.DefaultRequestHeaders.Add("apikeyprivate", payWallOptions.PrivateKey);
                httpClient.DefaultRequestHeaders.Add("apiclientprivate", payWallOptions.PrivateClient);
            }).AddMultipleHttpMessageHandlers(handlerFactories);

            services.AddTransient<PaymentPrivateApiClient>();

            return services;
        }

        private static IServiceCollection AddCardWallApiClient(this IServiceCollection services,
            PayWallOptions payWallOptions, params Func<IServiceProvider, DelegatingHandler>[] handlerFactories)
        {
            if (payWallOptions == null) throw new ArgumentNullException(nameof(payWallOptions));

            var baseAddress = payWallOptions.DataCenter switch
            {
                DataCenter.Global => payWallOptions.Prod ? ProdCardWallApiUrlGCP : TestCardWallApiUrlGCP,
                DataCenter.Turkey => payWallOptions.Prod ? ProdCardWallApiUrlHuawei : throw new NotSupportedException("Huawei does not support test."),
                _ => throw new NotSupportedException($"'{payWallOptions.DataCenter}' is not supported.")
            };

            services.AddHttpClient(CardWallClientName, httpClient =>
            {
                httpClient.BaseAddress = baseAddress;
                httpClient.DefaultRequestHeaders.Add("apikeyprivate", payWallOptions.PrivateKey);
                httpClient.DefaultRequestHeaders.Add("apiclientprivate", payWallOptions.PrivateClient);
            }).AddMultipleHttpMessageHandlers(handlerFactories);

            services.AddTransient<CardWallApiClient>();

            return services;
        }

        private static IServiceCollection AddMemberApiClient(this IServiceCollection services,
            PayWallOptions payWallOptions, params Func<IServiceProvider, DelegatingHandler>[] handlerFactories)
        {
            if (payWallOptions == null) throw new ArgumentNullException(nameof(payWallOptions));

            var baseAddress = payWallOptions.DataCenter switch
            {
                DataCenter.Global => payWallOptions.Prod ? ProdMemberApiUrlGCP : TestMemberApiUrlGCP,
                DataCenter.Turkey => payWallOptions.Prod ? ProdMemberApiUrlHuawei : throw new NotSupportedException("Huawei does not support test."),
                _ => throw new NotSupportedException($"'{payWallOptions.DataCenter}' is not supported.")
            };

            services.AddHttpClient(MemberClientName, httpClient =>
            {
                httpClient.BaseAddress = baseAddress;
                httpClient.DefaultRequestHeaders.Add("apikeypublic", payWallOptions.PublicKey);
                httpClient.DefaultRequestHeaders.Add("apiclientpublic", payWallOptions.PublicClient);
            })
                .AddMultipleHttpMessageHandlers(handlerFactories);

            services.AddTransient<MemberApiClient>();

            return services;
        }

        private static void AddMultipleHttpMessageHandlers(this IHttpClientBuilder httpClientBuilder,
            params Func<IServiceProvider, DelegatingHandler>[] handlerFactories)
        {
            if (handlerFactories == null)
            {
                return;
            }

            foreach (var handlerFactory in handlerFactories)
            {
                if (handlerFactory == null)
                {
                    continue;
                }

                httpClientBuilder.AddHttpMessageHandler(handlerFactory);
            }
        }
        #endregion
    }
}
