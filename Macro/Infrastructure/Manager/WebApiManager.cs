using Dignus.DependencyInjection.Attributes;
using Dignus.Log;
using Dignus.Utils;
using Dignus.Utils.Extensions;
using Macro.Models.Protocols;
using System;
using System.Threading.Tasks;
using Utils;

namespace Macro.Infrastructure.Manager
{
    [Injectable(Dignus.DependencyInjection.LifeScope.Transient)]
    internal class WebApiManager
    {
        private readonly HttpRequester _httpRequester = new HttpRequester();

        public VersionNote GetLatestVersion()
        {
            VersionNote versionNote = null;
            var requestUrl = ConstHelper.VersionUrl;
#if DEBUG
            requestUrl = "http://localhost:9100/macro/GetMacroLatestVersion";
#endif
            versionNote = Task.Run(async () =>
            {
                try
                {
                    var responseJson = await _httpRequester.PostByJsonAsync(requestUrl, "{}");
                    var response = JsonHelper.DeserializeObject<GetMacroLatestVersionResponse>(responseJson);

                    return response.VersionNote;
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                return null;

            }).GetResult();

            return versionNote;
        }
    }
}
