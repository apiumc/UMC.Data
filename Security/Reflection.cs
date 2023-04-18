using System;
using UMC.Data;
using UMC.Security;

namespace UMC.Security
{

    public class Reflection : UMC.Data.Reflection
    {
        protected override bool IsAuthorization(Identity identity, int site, string path)
        {
            return AuthManager.IsAuthorization(identity, site, path);
        }
        protected override ProviderConfiguration Configurate(string configKey)
        {
            if (ProviderConfiguration.Cache.ContainsKey(configKey))
            {
                return ProviderConfiguration.Cache[configKey];
            }
            var v = UMC.Data.DataFactory.Instance().Config($"Conf_{configKey}");
            if (v != null)
            {
                var p = UMC.Data.JSON.Deserialize<ProviderConfiguration>(v.ConfValue);
                if (p != null)
                {
                    ProviderConfiguration.Cache[configKey] = p;
                }
                return p;
            }
            return null;
        }
        protected override void Configurate(string configKey, ProviderConfiguration providerConfiguration)
        {
            if (ProviderConfiguration.Cache.ContainsKey(configKey) == false)
            {
                ProviderConfiguration.Cache[configKey] = providerConfiguration;
            }
            var config = new UMC.Data.Entities.Config() { ConfKey = $"Conf_{configKey}", ConfValue = UMC.Data.JSON.Serialize(providerConfiguration) };
            UMC.Data.DataFactory.Instance().Put(config);

        }
    }
}

