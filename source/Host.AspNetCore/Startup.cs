using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Builder;
using Owin;
using Microsoft.AspNetCore.DataProtection;

namespace Host.AspNetCore
{
    using DataProtectionProviderDelegate = Func<string[], Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>>;
    using DataProtectionTuple = Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(pipeline =>
            {
                var builder = new AppBuilder();

                var provider = app.ApplicationServices.GetRequiredService<IDataProtectionProvider>();

                builder.Properties["security.DataProtectionProvider"] = new DataProtectionProviderDelegate(purposes =>
                {
                    var dataProtection = provider.CreateProtector(String.Join(",", purposes));
                    return new DataProtectionTuple(dataProtection.Protect, dataProtection.Unprotect);
                });

                builder.UseIdentityServer();

                var owinPipeline = builder.Build();
                pipeline(next => owinPipeline);
            });
        }
    }
}
