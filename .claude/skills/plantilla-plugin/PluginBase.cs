using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace Proyecto.Plugins.Comun
{
    /// <summary>
    /// Clase base para todos los plugins del proyecto.
    /// Resuelve los servicios, centraliza el tracing y convierte errores inesperados
    /// en InvalidPluginExecutionException con información útil en el log.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        protected string NombrePlugin { get; }

        protected PluginBase(Type tipoPlugin)
        {
            NombrePlugin = tipoPlugin.FullName;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new InvalidPluginExecutionException(nameof(serviceProvider));

            var ctx = new LocalPluginContext(serviceProvider);
            ctx.Trace($"Inicio {NombrePlugin} | Mensaje: {ctx.Context.MessageName} | Entidad: {ctx.Context.PrimaryEntityName} | Etapa: {ctx.Context.Stage} | Profundidad: {ctx.Context.Depth}");

            try
            {
                ExecutePlugin(ctx);
            }
            catch (InvalidPluginExecutionException)
            {
                // Errores de negocio ya preparados para el usuario.
                throw;
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                ctx.Trace($"Error de Dataverse: {ex.Detail?.Message ?? ex.Message}");
                throw new InvalidPluginExecutionException($"Error en {NombrePlugin}: {ex.Detail?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                ctx.Trace($"Error inesperado: {ex}");
                throw new InvalidPluginExecutionException($"Error inesperado en {NombrePlugin}. Consulte el registro de seguimiento.", ex);
            }
            finally
            {
                ctx.Trace($"Fin {NombrePlugin}");
            }
        }

        protected abstract void ExecutePlugin(LocalPluginContext ctx);
    }

    public class LocalPluginContext
    {
        public IPluginExecutionContext Context { get; }
        public IOrganizationService UserService { get; }
        public IOrganizationService SystemService { get; }
        public ITracingService TracingService { get; }

        public LocalPluginContext(IServiceProvider serviceProvider)
        {
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            UserService = factory.CreateOrganizationService(Context.UserId);
            SystemService = factory.CreateOrganizationService(null);
        }

        public void Trace(string mensaje) => TracingService?.Trace(mensaje);

        /// <summary>Target como Entity, o null si no existe o no es una Entity.</summary>
        public Entity GetTarget()
        {
            return Context.InputParameters.TryGetValue("Target", out var t) ? t as Entity : null;
        }

        public Entity GetPreImage(string alias)
        {
            return Context.PreEntityImages.TryGetValue(alias, out var img) ? img : null;
        }

        public Entity GetPostImage(string alias)
        {
            return Context.PostEntityImages.TryGetValue(alias, out var img) ? img : null;
        }

        /// <summary>Valor del Target si viene en la operación; si no, de la imagen indicada.</summary>
        public T GetValorActual<T>(Entity target, Entity imagen, string atributo)
        {
            if (target != null && target.Contains(atributo))
                return target.GetAttributeValue<T>(atributo);
            return imagen != null ? imagen.GetAttributeValue<T>(atributo) : default(T);
        }
    }

    public static class Etapas
    {
        public const int PreValidation = 10;
        public const int PreOperation = 20;
        public const int PostOperation = 40;
    }
}
