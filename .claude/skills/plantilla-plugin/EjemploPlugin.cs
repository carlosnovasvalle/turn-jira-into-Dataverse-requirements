using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Proyecto.Plugins.Comun;

namespace Proyecto.Plugins.Oportunidad
{
    /// <summary>
    /// Ejemplo de referencia.
    /// Registro: Create y Update de opportunity, PreOperation, síncrono.
    /// Update filtrado por: customerid. PreImage "PreImage" con: customerid, pfx_descuento.
    /// </summary>
    public class OpportunityDefaultDiscountPlugin : PluginBase
    {
        private const string PreImageAlias = "PreImage";
        private const int TipoClienteVip = 100000001; // Valor de la opción en account.customertypecode
        private const decimal DescuentoVip = 10m;

        public OpportunityDefaultDiscountPlugin() : base(typeof(OpportunityDefaultDiscountPlugin)) { }

        protected override void ExecutePlugin(LocalPluginContext ctx)
        {
            // 1. Validar que el contexto aplica
            if (ctx.Context.PrimaryEntityName != "opportunity") return;
            if (ctx.Context.Stage != Etapas.PreOperation) return;
            if (ctx.Context.MessageName != "Create" && ctx.Context.MessageName != "Update") return;

            var target = ctx.GetTarget();
            if (target == null) return;

            var preImage = ctx.GetPreImage(PreImageAlias);

            // 2. Leer valores actuales (Target si viene, si no PreImage)
            var cliente = ctx.GetValorActual<EntityReference>(target, preImage, "customerid");
            if (cliente == null || cliente.LogicalName != "account")
            {
                ctx.Trace("Sin cuenta como cliente: no aplica R1.");
                return;
            }

            // Si el usuario ha informado el descuento en esta operación, se respeta.
            if (target.Contains("pfx_descuento"))
            {
                ctx.Trace("Descuento informado explícitamente: no se sobrescribe.");
                return;
            }

            // 3. Consultar solo lo necesario
            var cuenta = ctx.UserService.Retrieve("account", cliente.Id, new ColumnSet("customertypecode"));
            var tipo = cuenta.GetAttributeValue<OptionSetValue>("customertypecode");

            // R1: si el cliente es VIP, descuento por defecto del 10 %
            if (tipo?.Value == TipoClienteVip)
            {
                var descuentoActual = ctx.GetValorActual<decimal?>(target, preImage, "pfx_descuento");
                if (descuentoActual != DescuentoVip)
                {
                    target["pfx_descuento"] = DescuentoVip; // PreOperation: se modifica el Target, sin Update
                    ctx.Trace($"R1 aplicada: descuento {DescuentoVip} %.");
                }
            }
        }
    }
}
