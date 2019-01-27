using System;
using Autofac;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;

namespace IXIComponents.Simple
{
    public static class SetupExtension
    {
        public static IXICore SimpleSetup(this IXICore core, string coinName, IXISettings settings)
        {

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new CoinName(coinName));
            builder.RegisterInstance(settings);
            builder.RegisterType<SimpleTangleAccessor>().As<ITangleAccessor>().InstancePerLifetimeScope();
            builder.RegisterType<SimpleDataAccessor>().As<IDataAccessor>().InstancePerLifetimeScope();
            builder.RegisterType<SimpleConsensus>().As<IConsensus>().InstancePerLifetimeScope();

            builder.RegisterType<IXICore>().AsSelf().InstancePerLifetimeScope();

            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                return scope.Resolve<IXICore>();
            }

            throw new ArgumentException("oop");

        }
    }
}
