namespace Game.Engine.Controllers
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class RegistryController : APIControllerBase
    {
        private readonly GameConfiguration Config;
        
        private static Dictionary<string, RegistryReport> Reports = new Dictionary<string, RegistryReport>();
        private DateTime LastCleaning = DateTime.MinValue;
        private const int MAX_AGE = 10000;

        public RegistryController(
            ISecurityContext securityContext,
            GameConfiguration config
        ) : base(securityContext)
        {
            this.Config = config;
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
            HttpPost,
            Route("report")
        ]
        public Task<bool> PostReportAsync([FromBody]RegistryReport registryReport)
        {
            if (registryReport != null)
            {
                var url = registryReport.URL;
                lock (Reports)
                {
                    registryReport.Received = DateTime.Now;
                    if (Reports.ContainsKey(url))
                        Reports[url] = registryReport;
                    else
                        Reports.Add(url, registryReport);

                    return Task.FromResult(true);
                }
            }
            else
                return Task.FromResult(false);
        }

        [
            AllowAnonymous,
            HttpPost,
            Route("events")
        ]
        public bool PostEvents([FromBody]IEnumerable<object> events)
        {
            return true;
        }
    }
}