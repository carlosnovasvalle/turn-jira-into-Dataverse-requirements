/**
 * Web resource de ejemplo: formulario principal de Oportunidad.
 * Registro en el formulario: onLoad -> Proyecto.Opportunity.onLoad
 *                            onSave -> Proyecto.Opportunity.onSave
 * Marcar "Pasar contexto de ejecución como primer parámetro".
 */
var Proyecto = window.Proyecto || {};

Proyecto.Opportunity = (function () {
    "use strict";

    // --- Constantes ---------------------------------------------------------
    const CAMPOS = {
        cliente: "customerid",
        descuento: "pfx_descuento",
        estado: "statecode"
    };
    const SECCIONES = {
        tab: "Summary",
        descuentos: "seccion_descuentos"
    };
    const ESTADO_ABIERTA = 0;
    const TIPO_CLIENTE_VIP = 100000001;
    const NOTIF_DESCUENTO = "notif_descuento";
    const TIPO_FORM_CREAR = 1;

    // --- Funciones privadas -------------------------------------------------
    function getValor(formContext, campo) {
        const attr = formContext.getAttribute(campo);
        return attr ? attr.getValue() : null;
    }

    // UX1: ocultar la sección de descuentos si la oportunidad no está abierta
    function actualizarVisibilidadDescuentos(formContext) {
        const tab = formContext.ui.tabs.get(SECCIONES.tab);
        const seccion = tab ? tab.sections.get(SECCIONES.descuentos) : null;
        if (!seccion) return;

        const estado = getValor(formContext, CAMPOS.estado);
        seccion.setVisible(estado === null || estado === ESTADO_ABIERTA);
    }

    // UX2: avisar si el cliente es VIP (el descuento real lo aplica el plugin, R1)
    async function avisarClienteVip(formContext) {
        formContext.ui.clearFormNotification(NOTIF_DESCUENTO);

        const cliente = getValor(formContext, CAMPOS.cliente);
        if (!cliente || cliente.length === 0 || cliente[0].entityType !== "account") return;

        try {
            const id = cliente[0].id.replace(/[{}]/g, "");
            const cuenta = await Xrm.WebApi.retrieveRecord("account", id, "?$select=customertypecode");
            if (cuenta.customertypecode === TIPO_CLIENTE_VIP) {
                formContext.ui.setFormNotification(
                    "Cliente VIP: al guardar se aplicará el descuento por defecto si no se informa otro.",
                    "INFO",
                    NOTIF_DESCUENTO
                );
            }
        } catch (error) {
            formContext.ui.setFormNotification(
                "No se ha podido comprobar el tipo de cliente: " + error.message,
                "WARNING",
                NOTIF_DESCUENTO
            );
        }
    }

    // --- Manejadores públicos -----------------------------------------------
    function onLoad(executionContext) {
        const formContext = executionContext.getFormContext();

        const attrCliente = formContext.getAttribute(CAMPOS.cliente);
        if (attrCliente) attrCliente.addOnChange(onChangeCliente);

        actualizarVisibilidadDescuentos(formContext);

        if (formContext.ui.getFormType() === TIPO_FORM_CREAR) {
            avisarClienteVip(formContext);
        }
    }

    function onChangeCliente(executionContext) {
        const formContext = executionContext.getFormContext();
        avisarClienteVip(formContext);
    }

    // UX3: el descuento no puede ser negativo ni superior a 50
    function onSave(executionContext) {
        const formContext = executionContext.getFormContext();
        const descuento = getValor(formContext, CAMPOS.descuento);

        if (descuento !== null && (descuento < 0 || descuento > 50)) {
            executionContext.getEventArgs().preventDefault();
            Xrm.Navigation.openAlertDialog({ text: "El descuento debe estar entre 0 y 50." });
        }
    }

    return {
        onLoad: onLoad,
        onChangeCliente: onChangeCliente,
        onSave: onSave
    };
})();
