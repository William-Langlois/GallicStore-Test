using FluentMigrator;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Seo;

namespace Nop.Web.Framework.Migrations.UpgradeTo440
{
    [NopMigration("2020-06-10 00:00:00", "4.40.0", UpdateMigrationType.Settings)]
    [SkipMigrationOnInstall]
    public class SettingMigration : MigrationBase
    {
        /// <summary>Collect the UP migration expressions</summary>
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //do not use DI, because it produces exception on the installation process
            var settingRepository = EngineContext.Current.Resolve<IRepository<Setting>>();
            var settingService = EngineContext.Current.Resolve<ISettingService>();

            //#4904 External authentication errors logging
            var externalAuthenticationSettings = settingService.LoadSettingAsync<ExternalAuthenticationSettings>(0).Result;
            if (!settingService.SettingExistsAsync(externalAuthenticationSettings, settings => settings.LogErrors,0).Result)
            {
                externalAuthenticationSettings.LogErrors = false;
                settingService.SaveSettingAsync(externalAuthenticationSettings,0).Wait();
            }

            var multiFactorAuthenticationSettings = settingService.LoadSettingAsync<MultiFactorAuthenticationSettings>(0).Result;
            if (!settingService.SettingExistsAsync(multiFactorAuthenticationSettings, settings => settings.ForceMultifactorAuthentication,0).Result)
            {
                multiFactorAuthenticationSettings.ForceMultifactorAuthentication = false;

                settingService.SaveSettingAsync(multiFactorAuthenticationSettings,0).Wait();
            }

            //#5102 Delete Full-text settings
            settingRepository
                .DeleteAsync(setting => setting.Name == "commonsettings.usefulltextsearch" || setting.Name == "commonsettings.fulltextmode")
                .Wait();

            //#4196
            settingRepository
                .DeleteAsync(setting => setting.Name == "commonsettings.scheduletaskruntimeout" ||
                    setting.Name == "commonsettings.staticfilescachecontrol" ||
                    setting.Name == "commonsettings.supportpreviousnopcommerceversions" ||
                    setting.Name == "securitysettings.pluginstaticfileextensionsBlacklist")
                .Wait();

            //#5384
            var seoSettings = settingService.LoadSettingAsync<SeoSettings>(0).Result;
            foreach (var slug in NopSeoDefaults.ReservedUrlRecordSlugs)
            {
                if (!seoSettings.ReservedUrlRecordSlugs.Contains(slug))
                    seoSettings.ReservedUrlRecordSlugs.Add(slug);
            }
            settingService.SaveSettingAsync(seoSettings,0).Wait();

            //#3015
            if (!settingService.SettingExistsAsync(seoSettings, settings => settings.HomepageTitle,0).Result)
            {
                seoSettings.HomepageTitle = seoSettings.DefaultTitle;
                settingService.SaveSettingAsync(seoSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(seoSettings, settings => settings.HomepageDescription,0).Result)
            {
                seoSettings.HomepageDescription = "Your home page description";
                settingService.SaveSettingAsync(seoSettings,0).Wait();
            }

            //#5210
            var adminAreaSettings = settingService.LoadSettingAsync<AdminAreaSettings>(0).Result;
            if (!settingService.SettingExistsAsync(adminAreaSettings, settings => settings.ShowDocumentationReferenceLinks,0).Result)
            {
                adminAreaSettings.ShowDocumentationReferenceLinks = true;
                settingService.SaveSettingAsync(adminAreaSettings,0).Wait();
            }

            //#4944
            var shippingSettings = settingService.LoadSettingAsync<ShippingSettings>(0).Result;
            if (!settingService.SettingExistsAsync(shippingSettings, settings => settings.RequestDelay,0).Result)
            {
                shippingSettings.RequestDelay = 300;
                settingService.SaveSettingAsync(shippingSettings,0).Wait();
            }

            //#276 AJAX filters
            var catalogSettings = settingService.LoadSettingAsync<CatalogSettings>(0).Result;
            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.UseAjaxCatalogProductsLoading,0).Result)
            {
                catalogSettings.UseAjaxCatalogProductsLoading = true;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.EnableManufacturerFiltering,0).Result)
            {
                catalogSettings.EnableManufacturerFiltering = true;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.EnablePriceRangeFiltering,0).Result)
            {
                catalogSettings.EnablePriceRangeFiltering = true;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.SearchPagePriceRangeFiltering,0).Result)
            {
                catalogSettings.SearchPagePriceRangeFiltering = true;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.SearchPagePriceFrom,0).Result)
            {
                catalogSettings.SearchPagePriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.SearchPagePriceTo,0).Result)
            {
                catalogSettings.SearchPagePriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.SearchPageManuallyPriceRange,0).Result)
            {
                catalogSettings.SearchPageManuallyPriceRange = false;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.ProductsByTagPriceRangeFiltering,0).Result)
            {
                catalogSettings.ProductsByTagPriceRangeFiltering = true;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.ProductsByTagPriceFrom,0).Result)
            {
                catalogSettings.ProductsByTagPriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.ProductsByTagPriceTo,0).Result)
            {
                catalogSettings.ProductsByTagPriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.ProductsByTagManuallyPriceRange,0).Result)
            {
                catalogSettings.ProductsByTagManuallyPriceRange = false;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            //#4303
            var orderSettings = settingService.LoadSettingAsync<OrderSettings>(0).Result;
            if (!settingService.SettingExistsAsync(orderSettings, settings => settings.DisplayCustomerCurrencyOnOrders,0).Result)
            {
                orderSettings.DisplayCustomerCurrencyOnOrders = false;
                settingService.SaveSettingAsync(orderSettings,0).Wait();
            }

            //#16 #2909
            if (!settingService.SettingExistsAsync(catalogSettings, settings => settings.AttributeValueOutOfStockDisplayType,0).Result)
            {
                catalogSettings.AttributeValueOutOfStockDisplayType = AttributeValueOutOfStockDisplayType.AlwaysDisplay;
                settingService.SaveSettingAsync(catalogSettings,0).Wait();
            }

            //#5482
            settingService.SetSettingAsync("avalarataxsettings.gettaxratebyaddressonly", true,0,0).Wait();
            settingService.SetSettingAsync("avalarataxsettings.taxratebyaddresscachetime", 480,0,0).Wait();

            //#5349
            if (!settingService.SettingExistsAsync(shippingSettings, settings => settings.EstimateShippingCityNameEnabled,0).Result)
            {
                shippingSettings.EstimateShippingCityNameEnabled = false;
                settingService.SaveSettingAsync(shippingSettings,0).Wait();
            }
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}