using Dignus.DependencyInjection.Attributes;
using Dignus.Utils;

namespace Macro.Infrastructure.Manager
{
    [Injectable(Dignus.DependencyInjection.LifeScope.Singleton)]
    internal class AdManager
    {
        private readonly string[] AdUrls = new string[]
        {
            "https://diademata.tistory.com/259",
            "https://www.itdah.com/MacroVersion",
        };
        private readonly RandomGenerator _randomGenerator = new RandomGenerator();
        public string GetRandomAdUrl()
        {
            return AdUrls[_randomGenerator.Next(AdUrls.Length)];
        }
    }
}
