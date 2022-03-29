using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayPalStandard.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
//Ajout pour récupérer les vendeurs disponibles
using Nop.Services.Vendors;
using Nop.Core.Domain.Vendors;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Payments.PayPalStandard.Controllers
{
    public class PaymentPayPalStandardController : BasePaymentController
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public PaymentPayPalStandardController(IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            ShoppingCartSettings shoppingCartSettings,
            IVendorService vendorService)
        {
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _vendorService = vendorService; 
        }

        #endregion

        #region Methods

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("VendorScope")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> ChangeVendorScope(ConfigurationModel model) //Permet de changer le scope vendeur lors de la configuration du plugin de paiement
        {
            return await Configure(model.VendorIdScopeConfiguration);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(int vendorId = 0)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            PayPalStandardPaymentSettings payPalStandardPaymentSettings = await _settingService.LoadSettingAsync<PayPalStandardPaymentSettings>(storeScope,vendorId:vendorId);
            vendorId = payPalStandardPaymentSettings.VendorIdScopeConfiguration > 0 ? payPalStandardPaymentSettings.VendorIdScopeConfiguration : vendorId;

            IList<Vendor> availableVendors = await _vendorService.GetAllVendorsListAsync();
            IList<SelectListItem> vendorsOptions = new List<SelectListItem>();
            foreach (Vendor vendor in availableVendors)
            {
                vendorsOptions.Add(new SelectListItem(vendor.Name,vendor.Id.ToString(),vendorId == vendor.Id));
            }
            //Ajout d'une option pour le vendorScope 0
            vendorsOptions.Add(new SelectListItem("Select vendor", "0",vendorId == 0));


            var model = new ConfigurationModel
            {
                UseSandbox = payPalStandardPaymentSettings.UseSandbox,
                BusinessEmail = payPalStandardPaymentSettings.BusinessEmail,
                PdtToken = payPalStandardPaymentSettings.PdtToken,
                PassProductNamesAndTotals = payPalStandardPaymentSettings.PassProductNamesAndTotals,
                AdditionalFee = payPalStandardPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = payPalStandardPaymentSettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope,
                VendorIdScopeConfiguration = vendorId,
                AvailableVendors = vendorsOptions
            };

            if (storeScope <= 0 && vendorId <= 0)
                return View("~/Plugins/Payments.PayPalStandard/Views/Configure.cshtml", model);

            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.UseSandbox, storeScope,vendorId);
            model.BusinessEmail_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.BusinessEmail, storeScope, vendorId);
            model.PdtToken_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.PdtToken, storeScope, vendorId);
            model.PassProductNamesAndTotals_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, storeScope, vendorId);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.AdditionalFee, storeScope, vendorId);
            model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, storeScope, vendorId);
            model.VendorIdScopeConfiguration_OverrideForStore = await _settingService.SettingExistsAsync(payPalStandardPaymentSettings, x => x.VendorIdScopeConfiguration, storeScope, vendorId);

            return View("~/Plugins/Payments.PayPalStandard/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("save")]

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model, int vendorId = 0)
     {

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (vendorId <= 0)
                vendorId = model.VendorIdScopeConfiguration;

            if (!ModelState.IsValid)
                return await Configure(vendorId);

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var payPalStandardPaymentSettings = await _settingService.LoadSettingAsync<PayPalStandardPaymentSettings>(storeScope,vendorId);

            //save settings
            payPalStandardPaymentSettings.UseSandbox = model.UseSandbox;
            payPalStandardPaymentSettings.BusinessEmail = model.BusinessEmail;
            payPalStandardPaymentSettings.PdtToken = model.PdtToken;
            payPalStandardPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            payPalStandardPaymentSettings.AdditionalFee = model.AdditionalFee;
            payPalStandardPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            payPalStandardPaymentSettings.VendorIdScopeConfiguration = vendorId;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, vendorId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.BusinessEmail, model.BusinessEmail_OverrideForStore, storeScope, vendorId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.PdtToken, model.PdtToken_OverrideForStore, storeScope, vendorId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, vendorId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, vendorId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, vendorId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(payPalStandardPaymentSettings, x => x.VendorIdScopeConfiguration, model.VendorIdScopeConfiguration_OverrideForStore, storeScope, vendorId, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure(vendorId);
        }

        //action displaying notification (warning) to a store owner about inaccurate PayPal rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> RoundingWarning(bool passProductNamesAndTotals)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = await _localizationService.GetResourceAsync("Plugins.Payments.PayPalStandard.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> PDTHandler()
        {
            var tx = _webHelper.QueryString<string>("tx");

            if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.PayPalStandard") is not PayPalStandardPaymentProcessor processor || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("PayPal Standard module cannot be loaded");

            var (result, values, response) = await processor.GetPdtDetailsAsync(tx);

            if (result)
            {
                values.TryGetValue("custom", out var orderNumber);
                var orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch
                {
                    // ignored
                }

                var order = await _orderService.GetOrderByGuidAsync(orderNumberGuid);

                if (order == null)
                    return RedirectToAction("Index", "Home", new { area = string.Empty });

                var mcGross = decimal.Zero;

                try
                {
                    mcGross = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync("PayPal PDT. Error getting mc_gross", exc);
                }

                values.TryGetValue("payer_status", out var payerStatus);
                values.TryGetValue("payment_status", out var paymentStatus);
                values.TryGetValue("pending_reason", out var pendingReason);
                values.TryGetValue("mc_currency", out var mcCurrency);
                values.TryGetValue("txn_id", out var txnId);
                values.TryGetValue("payment_type", out var paymentType);
                values.TryGetValue("payer_id", out var payerId);
                values.TryGetValue("receiver_id", out var receiverId);
                values.TryGetValue("invoice", out var invoice);
                values.TryGetValue("mc_fee", out var mcFee);

                var sb = new StringBuilder();
                sb.AppendLine("PayPal PDT:");
                sb.AppendLine("mc_gross: " + mcGross);
                sb.AppendLine("Payer status: " + payerStatus);
                sb.AppendLine("Payment status: " + paymentStatus);
                sb.AppendLine("Pending reason: " + pendingReason);
                sb.AppendLine("mc_currency: " + mcCurrency);
                sb.AppendLine("txn_id: " + txnId);
                sb.AppendLine("payment_type: " + paymentType);
                sb.AppendLine("payer_id: " + payerId);
                sb.AppendLine("receiver_id: " + receiverId);
                sb.AppendLine("invoice: " + invoice);
                sb.AppendLine("mc_fee: " + mcFee);

                var newPaymentStatus = PayPalHelper.GetPaymentStatus(paymentStatus, string.Empty);
                sb.AppendLine("New payment status: " + newPaymentStatus);

                //order note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = sb.ToString(),
                    DisplayToCustomer = true,
                    CreatedOnUtc = DateTime.UtcNow
                });

                //validate order total
                var orderTotalSentToPayPal = await _genericAttributeService.GetAttributeAsync<decimal?>(order, PayPalHelper.OrderTotalSentToPayPal);
                if (orderTotalSentToPayPal.HasValue && mcGross != orderTotalSentToPayPal.Value)
                {
                    var errorStr = $"PayPal PDT. Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
                    //log
                    await _logger.ErrorAsync(errorStr);
                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = errorStr,
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }

                //clear attribute
                if (orderTotalSentToPayPal.HasValue)
                    await _genericAttributeService.SaveAttributeAsync<decimal?>(order, PayPalHelper.OrderTotalSentToPayPal, null);

                if (newPaymentStatus != PaymentStatus.Paid)
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

                if (!_orderProcessingService.CanMarkOrderAsPaid(order))
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

                //mark order as paid
                order.AuthorizationTransactionId = txnId;
                await _orderService.UpdateOrderAsync(order);
                await _orderProcessingService.MarkOrderAsPaidAsync(order);

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
            {
                if (!values.TryGetValue("custom", out var orderNumber))
                    orderNumber = _webHelper.QueryString<string>("cm");

                var orderNumberGuid = Guid.Empty;

                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch
                {
                    // ignored
                }

                var order = await _orderService.GetOrderByGuidAsync(orderNumberGuid);
                if (order == null)
                    return RedirectToAction("Index", "Home", new { area = string.Empty });

                //order note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "PayPal PDT failed. " + response,
                    DisplayToCustomer = true,
                    CreatedOnUtc = DateTime.UtcNow
                });

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> CancelOrder()
        {
            var order = (await _orderService.SearchOrdersAsync((await _storeContext.GetCurrentStoreAsync()).Id,
                customerId: (await _workContext.GetCurrentCustomerAsync()).Id, pageSize: 1)).FirstOrDefault();

            if (order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("Homepage");
        }

        #endregion
    }
}