namespace Game.Registry.Controllers
{
    using Game.API.Client;
    using Game.API.Common.Models;
    using Game.API.Common.Models.Auditing;
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Nest;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class RegistryController : APIControllerBase
    {
        private readonly GameConfiguration Config;
        private readonly ElasticClient ElasticClient;
        private static Dictionary<string, RegistryReport> Reports = new Dictionary<string, RegistryReport>();
        private DateTime LastCleaning = DateTime.MinValue;
        private const int MAX_AGE = 10000;

        public RegistryController(
            ISecurityContext securityContext,
            GameConfiguration config,
            ElasticClient elasticClient
        ) : base(securityContext)
        {
            this.Config = config;
            this.ElasticClient = elasticClient;
        }

        [
            AllowAnonymous,
            HttpGet
        ]
        public List<RegistryReport> GetList()
        {
            lock (Reports)
            {
                if (DateTime.Now.Subtract(LastCleaning).TotalMilliseconds > 2000)
                {
                    LastCleaning = DateTime.Now;

                    var stale = new List<string>();

                    foreach (var key in Reports.Keys)
                    {
                        var report = Reports[key];
                        if (DateTime.Now.Subtract(report.Received).TotalMilliseconds > MAX_AGE)
                            stale.Add(key);
                    }

                    foreach (var key in stale)
                        Reports.Remove(key);
                }

                return Reports.Values
                    .ToList();
            }
        }

        [
            AllowAnonymous,
            HttpGet,
            Route("suggestion")
        ]
        public async Task<string> SuggestDomainsAsync(string configuredName = null)
        {
            if (configuredName != null)
                return configuredName;
            else
                return await RecommendHostNameAsync();
        }

        private async Task<string> RecommendHostNameAsync()
        {
            // I should probably support some kind of x-forwarded for headers etc.
            var ipAddress = ControllerContext.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            try
            {
                var entry = await Dns.GetHostEntryAsync(ipAddress);
                var apiClient = new APIClient(new Uri($"http://{ipAddress}"));

                var cts = new CancellationTokenSource();
                cts.CancelAfter(2000);
                var server = await apiClient.Server.ServerGetAsync(cts.Token);
                if (server == null)
                {
                    Console.WriteLine($"Suggesting localhost to {ipAddress}");
                    return "localhost";
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Suggesting localhost to {ipAddress}");
                return "localhost";
            }

            var address = $"daud-{ipAddress.Replace(".", "-")}.sslip.io";
            Console.WriteLine($"Suggesting {address} to {ipAddress}");

            return address;
        }

        [
            AllowAnonymous,
            HttpPost,
            Route("report")
        ]
        public async Task<bool> PostReportAsync([FromBody]RegistryReport registryReport)
        {
            if (registryReport != null)
            {
                if (registryReport.URL == null)
                    registryReport.URL = await RecommendHostNameAsync();

                var url = registryReport.URL;
                lock (Reports)
                {
                    registryReport.Received = DateTime.Now;
                    if (Reports.ContainsKey(url))
                        Reports[url] = registryReport;
                    else
                        Reports.Add(url, registryReport);

                    return true;
                }
            }
            else
                return false;
        }

        [
            AllowAnonymous,
            HttpPost,
            Route("events")
        ]
        public bool PostEvents([FromBody]IEnumerable<object> events)
        {
            var waitHandle = new CountdownEvent(1);

            var bulkAll = ElasticClient.BulkAll(events, e => e.Size(1000));

            bulkAll.Subscribe(new BulkAllObserver(
                onNext: (b) => { Console.Write("."); },
                onError: (e) => { throw e; },
                onCompleted: () => waitHandle.Signal()
            ));

            waitHandle.Wait();

            return true;
        }
    }
}