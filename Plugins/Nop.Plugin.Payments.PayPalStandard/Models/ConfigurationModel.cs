﻿using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Payments.PayPalStandard.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.VendorIdScopeConfiguration")]
        public int VendorIdScopeConfiguration { get; set; }
        public bool VendorIdScopeConfiguration_OverrideForStore { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.BusinessEmail")]
        public string BusinessEmail { get; set; }
        public bool BusinessEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTToken")]
        public string PdtToken { get; set; }
        public bool PdtToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals")]
        public bool PassProductNamesAndTotals { get; set; }
        public bool PassProductNamesAndTotals_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}