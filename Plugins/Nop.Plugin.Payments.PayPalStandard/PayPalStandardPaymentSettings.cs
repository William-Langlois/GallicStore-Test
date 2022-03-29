using Nop.Core.Configuration;
using Nop.Services.Configuration.Caching;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.PayPalStandard
{
    /// <summary>
    /// Represents settings of the PayPal Standard payment plugin
    /// </summary>
    public class PayPalStandardPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }

        /// <summary>
        /// Gets or sets a business email
        /// </summary>
        public string BusinessEmail { get; set; }

        /// <summary>
        /// Gets or sets PDT identity token
        /// </summary>
        public string PdtToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to pass info about purchased items to PayPal
        /// </summary>
        public bool PassProductNamesAndTotals { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }

        /// <summary>
        /// Gets or sets a value for the store corresponding to this setting
        /// </summary>
        public int ActiveStoreScopeConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value for the vendor corresponding to this setting
        /// </summary>
        public int VendorIdScopeConfiguration { get; set; }

        public void LoadVendorScopedSettings(int storeId = 0 ,int vendorId = 0)
        {
            VendorIdScopeConfiguration = vendorId;
            ActiveStoreScopeConfiguration = storeId;
        }
    }
}
