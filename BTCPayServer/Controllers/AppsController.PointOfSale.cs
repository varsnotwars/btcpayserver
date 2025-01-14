﻿using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BTCPayServer.Data;
using BTCPayServer.Models.AppViewModels;
using BTCPayServer.Services.Apps;
using BTCPayServer.Services.Mails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTCPayServer.Controllers
{
    public partial class AppsController
    {
        public class PointOfSaleSettings
        {
            public PointOfSaleSettings()
            {
                Title = "Tea shop";
                Currency = "USD";
                Template =
                    "green tea:\n" +
                    "  price: 1\n" +
                    "  title: Green Tea\n" +
                    "  description:  Lovely, fresh and tender, Meng Ding Gan Lu ('sweet dew') is grown in the lush Meng Ding Mountains of the southwestern province of Sichuan where it has been cultivated for over a thousand years.\n" +
                    "  image: https://cdn.pixabay.com/photo/2015/03/26/11/03/green-tea-692339__480.jpg\n\n" +
                    "black tea:\n" +
                    "  price: 1\n" +
                    "  title: Black Tea\n" +
                    "  description: Tian Jian Tian Jian means 'heavenly tippy tea' in Chinese, and it describes the finest grade of dark tea. Our Tian Jian dark tea is from Hunan province which is famous for making some of the best dark teas available.\n" +
                    "  image: https://cdn.pixabay.com/photo/2016/11/29/13/04/beverage-1869716__480.jpg\n\n" +
                    "rooibos:\n" +
                    "  price: 1.2\n" +
                    "  title: Rooibos\n" +
                    "  description: Rooibos is a dramatic red tea made from a South African herb that contains polyphenols and flavonoids. Often called 'African redbush tea', Rooibos herbal tea delights the senses and delivers potential health benefits with each caffeine-free sip.\n" +
                    "  image: https://cdn.pixabay.com/photo/2017/01/08/08/14/water-1962388__480.jpg\n\n" +
                    "pu erh:\n" +
                    "  price: 2\n" +
                    "  title: Pu Erh\n" +
                    "  description: This loose pur-erh tea is produced in Yunnan Province, China. The process in a relatively high humidity environment has mellowed the elemental character of the tea when compared to young Pu-erh.\n" +
                    "  image: https://cdn.pixabay.com/photo/2018/07/21/16/56/tea-cup-3552917__480.jpg\n\n" +
                    "herbal tea:\n" +
                    "  price: 1.8\n" +
                    "  title: Herbal Tea\n" +
                    "  description: Chamomile tea is made from the flower heads of the chamomile plant. The medicinal use of chamomile dates back to the ancient Egyptians, Romans and Greeks. Pay us what you want!\n" +
                    "  image: https://cdn.pixabay.com/photo/2015/07/02/20/57/chamomile-829538__480.jpg\n" +
                    "  custom: true\n\n" +
                    "fruit tea:\n" +
                    "  price: 1.5\n" +
                    "  title: Fruit Tea\n" +
                    "  description: The Tibetan Himalayas, the land is majestic and beautiful—a spiritual place where, despite the perilous environment, many journey seeking enlightenment. Pay us what you want!\n" +
                    "  image: https://cdn.pixabay.com/photo/2016/09/16/11/24/darts-1673812__480.jpg\n" +
                    "  inventory: 5\n" +
                    "  custom: true";
                EnableShoppingCart = false;
                ShowCustomAmount = true;
                ShowDiscount = true;
                EnableTips = true;
            }
            public string Title { get; set; }
            public string Currency { get; set; }
            public string Template { get; set; }
            public bool EnableShoppingCart { get; set; }
            public bool ShowCustomAmount { get; set; }
            public bool ShowDiscount { get; set; }
            public bool EnableTips { get; set; }

            public const string BUTTON_TEXT_DEF = "Buy for {0}";
            public string ButtonText { get; set; } = BUTTON_TEXT_DEF;
            public const string CUSTOM_BUTTON_TEXT_DEF = "Pay";
            public string CustomButtonText { get; set; } = CUSTOM_BUTTON_TEXT_DEF;
            public const string CUSTOM_TIP_TEXT_DEF = "Do you want to leave a tip?";
            public string CustomTipText { get; set; } = CUSTOM_TIP_TEXT_DEF;
            public static readonly int[] CUSTOM_TIP_PERCENTAGES_DEF = new int[] { 15, 18, 20 };
            public int[] CustomTipPercentages { get; set; } = CUSTOM_TIP_PERCENTAGES_DEF;


            public string CustomCSSLink { get; set; }
            
            public string EmbeddedCSS { get; set; }
            
            public string Description { get; set; }
            public string NotificationEmail { get; set; }
            public string NotificationUrl { get; set; }
            public bool? RedirectAutomatically { get; set; }
        }

        [HttpGet]
        [Route("{appId}/settings/pos")]
        public async Task<IActionResult> UpdatePointOfSale(string appId)
        {
            var app = await GetOwnedApp(appId, AppType.PointOfSale);
            if (app == null)
                return NotFound();
            var settings = app.GetSettings<PointOfSaleSettings>();
          
            var vm = new UpdatePointOfSaleViewModel()
            {
                NotificationEmailWarning = !await IsEmailConfigured(app.StoreDataId),
                Id = appId,
                StoreId = app.StoreDataId,
                Title = settings.Title,
                EnableShoppingCart = settings.EnableShoppingCart,
                ShowCustomAmount = settings.ShowCustomAmount,
                ShowDiscount = settings.ShowDiscount,
                EnableTips = settings.EnableTips,
                Currency = settings.Currency,
                Template = settings.Template,
                ButtonText = settings.ButtonText ?? PointOfSaleSettings.BUTTON_TEXT_DEF,
                CustomButtonText = settings.CustomButtonText ?? PointOfSaleSettings.CUSTOM_BUTTON_TEXT_DEF,
                CustomTipText = settings.CustomTipText ?? PointOfSaleSettings.CUSTOM_TIP_TEXT_DEF,
                CustomTipPercentages = settings.CustomTipPercentages != null ? string.Join(",", settings.CustomTipPercentages) : string.Join(",", PointOfSaleSettings.CUSTOM_TIP_PERCENTAGES_DEF),
                CustomCSSLink = settings.CustomCSSLink,
                EmbeddedCSS = settings.EmbeddedCSS,
                Description = settings.Description,
                NotificationEmail = settings.NotificationEmail,
                NotificationUrl = settings.NotificationUrl,
                RedirectAutomatically = settings.RedirectAutomatically.HasValue? settings.RedirectAutomatically.Value? "true": "false" : "" 
            };
            if (HttpContext?.Request != null)
            {
                var appUrl = HttpContext.Request.GetAbsoluteRoot().WithTrailingSlash() + $"apps/{appId}/pos";
                var encoder = HtmlEncoder.Default;
                if (settings.ShowCustomAmount)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"<form method=\"POST\" action=\"{encoder.Encode(appUrl)}\">");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"amount\" value=\"100\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"email\" value=\"customer@example.com\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"orderId\" value=\"CustomOrderId\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"notificationUrl\" value=\"https://example.com/callbacks\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"redirectUrl\" value=\"https://example.com/thanksyou\" />");
                    builder.AppendLine($"  <button type=\"submit\">Buy now</button>");
                    builder.AppendLine($"</form>");
                    vm.Example1 = builder.ToString();
                }
                try
                {
                    var items = _AppService.Parse(settings.Template, settings.Currency);
                    var builder = new StringBuilder();
                    builder.AppendLine($"<form method=\"POST\" action=\"{encoder.Encode(appUrl)}\">");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"email\" value=\"customer@example.com\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"orderId\" value=\"CustomOrderId\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"notificationUrl\" value=\"https://example.com/callbacks\" />");
                    builder.AppendLine($"  <input type=\"hidden\" name=\"redirectUrl\" value=\"https://example.com/thanksyou\" />");
                    builder.AppendLine($"  <button type=\"submit\" name=\"choiceKey\" value=\"{items[0].Id}\">Buy now</button>");
                    builder.AppendLine($"</form>");
                    vm.Example2 = builder.ToString();
                }
                catch { }
                vm.InvoiceUrl = appUrl + "invoices/SkdsDghkdP3D3qkj7bLq3";
            }

            vm.ExampleCallback = "{\n  \"id\":\"SkdsDghkdP3D3qkj7bLq3\",\n  \"url\":\"https://btcpay.example.com/invoice?id=SkdsDghkdP3D3qkj7bLq3\",\n  \"status\":\"paid\",\n  \"price\":10,\n  \"currency\":\"EUR\",\n  \"invoiceTime\":1520373130312,\n  \"expirationTime\":1520374030312,\n  \"currentTime\":1520373179327,\n  \"exceptionStatus\":false,\n  \"buyerFields\":{\n    \"buyerEmail\":\"customer@example.com\",\n    \"buyerNotify\":false\n  },\n  \"paymentSubtotals\": {\n    \"BTC\":114700\n  },\n  \"paymentTotals\": {\n    \"BTC\":118400\n  },\n  \"transactionCurrency\": \"BTC\",\n  \"amountPaid\": \"1025900\",\n  \"exchangeRates\": {\n    \"BTC\": {\n      \"EUR\": 8721.690715789999,\n      \"USD\": 10817.99\n    }\n  }\n}";
            return View(vm);
        }
        [HttpPost]
        [Route("{appId}/settings/pos")]
        public async Task<IActionResult> UpdatePointOfSale(string appId, UpdatePointOfSaleViewModel vm)
        {
            if (_currencies.GetCurrencyData(vm.Currency, false) == null)
                ModelState.AddModelError(nameof(vm.Currency), "Invalid currency");
            try
            {
                _AppService.Parse(vm.Template, vm.Currency);
            }
            catch
            {
                ModelState.AddModelError(nameof(vm.Template), "Invalid template");
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var app = await GetOwnedApp(appId, AppType.PointOfSale);
            if (app == null)
                return NotFound();
            app.SetSettings(new PointOfSaleSettings()
            {
                Title = vm.Title,
                EnableShoppingCart = vm.EnableShoppingCart,
                ShowCustomAmount = vm.ShowCustomAmount,
                ShowDiscount = vm.ShowDiscount,
                EnableTips = vm.EnableTips,
                Currency = vm.Currency.ToUpperInvariant(),
                Template = vm.Template,
                ButtonText = vm.ButtonText,
                CustomButtonText = vm.CustomButtonText,
                CustomTipText = vm.CustomTipText,
                CustomTipPercentages = ListSplit(vm.CustomTipPercentages),
                CustomCSSLink = vm.CustomCSSLink,
                NotificationUrl = vm.NotificationUrl,
                NotificationEmail = vm.NotificationEmail,
                Description = vm.Description,
                EmbeddedCSS = vm.EmbeddedCSS,
                RedirectAutomatically = string.IsNullOrEmpty(vm.RedirectAutomatically)? (bool?) null: bool.Parse(vm.RedirectAutomatically)
                
            });
            await _AppService.UpdateOrCreateApp(app);
            StatusMessage = "App updated";
            return RedirectToAction(nameof(UpdatePointOfSale), new { appId });
        }


        private int[] ListSplit(string list, string separator = ",")
        {
            if (string.IsNullOrEmpty(list))
            {
                return Array.Empty<int>();
            } 
            else 
            {
                // Remove all characters except numeric and comma
                Regex charsToDestroy = new Regex(@"[^\d|\" + separator + "]");
                list = charsToDestroy.Replace(list, "");

                return list.Split(separator, System.StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            }
        }
    }
}
