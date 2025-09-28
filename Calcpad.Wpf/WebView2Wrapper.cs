using Calcpad.OpenXml;
using DocumentFormat.OpenXml.Math;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Calcpad.Wpf
{
    internal class WebView2Wrapper
    {
        private readonly WebView2 _wv2;
        private readonly string _blankPagePath;

        internal WebView2Wrapper(WebView2 wv2, string blankPagePath)
        {
            _wv2 = wv2;
            _blankPagePath = blankPagePath ?? "about:blank";
        }
        private async Task<T> InvokeScriptAsync<T>(string script)
        {   
            var json = await _wv2.ExecuteScriptAsync(script);
            return JsonSerializer.Deserialize<T>(json);
        }

        internal async Task<bool> CheckIsReportAsync()
        {
            try
            {
                if (_wv2.IsInitialized)
                {
                    var title = await InvokeScriptAsync<string>("document.title");
                    return title.StartsWith("Created");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        internal async Task<string> GetLinkDataAsync()
        {
            try
            {
                var tagName = await InvokeScriptAsync<string>("document.activeElement.tagName");
                if (tagName == "A")
                {
                    var linkData = await InvokeScriptAsync<string>("document.activeElement.getAttribute('data-text')");
                    if (linkData != "undefined")
                        return linkData;
                }
            }
            catch { }
            return null;
        }

        internal async Task<string> GetUnitsAsync()
        {
            var units = await InvokeScriptAsync<string>("$('#Units').val();");
            if (units is "cm" or "mm")
                return units;

            return "m";
        }

        internal async void SetUnitsAsync(string value) => await _wv2.ExecuteScriptAsync($"$('#Units').val('{value}');");

        internal async Task<double> GetScrollYAsync()
        {
            try
            {
                return await InvokeScriptAsync<double>("window.pageYOffset");
            }
            catch
            {
                return 0d;
            }

        }

        internal async Task SetScrollYAsync(double value)
        {
            try
            {
                if (value > 0)
                    await _wv2.ExecuteScriptAsync($"window.scrollTo(0, {value});");
            }
            catch { }
        }

        internal async Task<double> GetVerticalPositionAsync(int line)
        {
            try
            {
                if (!await CheckIsReportAsync())
                    return 0d;

                return await InvokeScriptAsync<double>($"getVerticalPosition({line});");
            }
            catch
            {
                return 0d;
            }
        }


        internal async Task<bool> CheckIsContextMenuAsync()
        {

            bool result = false;
            try
            {
                result = await InvokeScriptAsync<bool>("if(typeof(contextMenu)!='undefined'){contextMenu;}else{false;}");
            }
            catch { }
            return result;
        }

        internal async Task ScrollAsync(int line, double offset)
        {
            try
            {
                if (await CheckIsReportAsync())
                    await _wv2.ExecuteScriptAsync($"var e = document.getElementById('line-{line}'); if(e){{window.scrollTo(0, e.offsetTop - {offset});}}");
            }
            catch { }
        }

        internal async void SetContentAsync(int line, string content)
        {
            try
            {
                if (await CheckIsReportAsync())
                    await _wv2.ExecuteScriptAsync($"var e = document.getElementById('line-{line}'); if(e){{e.innerHTML='{content}';}}");
            }
            catch { }
        }

        internal async Task<string> ExportOpenXmlAsync(string path, List<string> expressions)
        {
            string html = null;
            try
            {
                html = await InvokeScriptAsync<string>("getHtmlWithInput()");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            html ??= await InvokeScriptAsync<string>("document.body.outerHTML");
            html = GetHtmlData(html);
            return new OpenXmlWriter(expressions).Convert(html, path);
        }

        internal async void PrintPreviewAsync()
        {
            await _wv2.ExecuteScriptAsync(@"var savedHtml = document.documentElement.outerHTML;
var printWindow = window.open('', '_blank');
printWindow.document.open();
printWindow.document.write(savedHtml);
printWindow.document.close();
printWindow.onafterprint = function(){printWindow.close();};
printWindow.onload = setTimeout(function(){
printWindow.focus(); 
printWindow.print();
}, 100);");
        }

        internal async void ClipboardCopyAsync()
        {
            if (await InvokeScriptAsync<bool>("window.getSelection().toString().length > 0"))
                _wv2.ExecuteScriptAsync("document.execCommand('copy');");
            else
            {
                var dataObject = new DataObject();
                var html = await InvokeScriptAsync<string>("document.documentElement.outerHTML");
                html = GetHtmlData(html);
                dataObject.SetData(DataFormats.Html, html);
                var text = await GetTextAsync();
                dataObject.SetData(DataFormats.Text, text);
                Clipboard.SetDataObject(dataObject);
            }
        }

        private async Task<string> GetTextAsync()
        {
            const string script = @"$('body').children().not('script').map(function() {
    var e = $(this); 
    var text = e.text(); 
    if (e.is('p, h1, h2, h3, h4, h5')) 
    {
        return text + '\n';
    } 
    return text;
}).get().join();";
            try
            {
                var text = await InvokeScriptAsync<string>(script);
                return text;
            }
            catch { }
            return string.Empty;
        }

        private static string GetHtmlData(string html)
        {
            var sb = new StringBuilder(500);
            const string header =
@"Version:1.0
StartHTML:0000000001
EndHTML:0000000002
StartFragment:0000000003
EndFragment:0000000004";
            const string startFragmentText = "<!DOCTYPE HTML><!--StartFragment-->";
            const string endFragmentText = "<!--EndFragment-->";
            var startHtml = header.Length;
            var startFragment = startHtml + startFragmentText.Length;
            var endFragment = startFragment + html.Length;
            var endHtml = endFragment + endFragmentText.Length;
            sb.Append(header);
            sb.Replace("0000000001", $"{startHtml,8}");
            sb.Replace("0000000002", $"{endHtml,8}");
            sb.Replace("0000000003", $"{startFragment,8}");
            sb.Replace("0000000004", $"{endFragment,8}");
            sb.Append(startFragmentText);
            sb.Append(html);
            sb.Append(endFragmentText);
            return sb.ToString();
        }

        internal async Task<string> GetSelectedTextAsync()
        {
            string result = await _wv2.ExecuteScriptAsync("window.getSelection().toString();");
            return JsonSerializer.Deserialize<string>(result);
        }

        internal async Task<string> GetContentsAsync() => await InvokeScriptAsync<string>("document.documentElement.outerHTML");

        internal async Task<string[]> GetInputFieldsAsync()
        {
            const string script = "$(\"input[type='text'][name='Var']\").map(function(){return this.className.split('-')[1] + ':' + $(this).val();}).get().join('│');";
            try
            {
                var s = await InvokeScriptAsync<string>(script);
                return s.Trim('"').Split('│');
            }
            catch
            {
                MessageBox.Show(AppMessages.Error_getting_input_fields_values, "Calcpad", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        internal void ReportInputFieldError(int index)
        {
            try
            {
                _wv2.ExecuteScriptAsync($"$('input:eq({index})').css('color', 'red').attr('title', 'Invalid  value').focus();");
            }
            catch { }
        }

        internal void Navigate(string path) => _wv2.CoreWebView2.Navigate(path);
        internal void NavigateToBlank() => _wv2.CoreWebView2.Navigate(_blankPagePath);

        internal async Task NavigateToStringAsync(string html)
        {
            var zoom = _wv2.ZoomFactor;
            _wv2.CoreWebView2.Navigate(_blankPagePath);
            _wv2.ZoomFactor = zoom;
            string encodedHtml = JsonSerializer.Serialize(html);
            string script = $"document.open();document.write({encodedHtml});document.close();";
            await _wv2.ExecuteScriptAsync(script);
        }

        internal CoreWebView2PrintSettings CreatePrintSettings()
        {
            var settings = _wv2.CoreWebView2.Environment.CreatePrintSettings();
            settings.Orientation = CoreWebView2PrintOrientation.Portrait;
            settings.ScaleFactor = 1.0;
            settings.PageWidth = 8.5; // A4 width in inches
            settings.PageHeight = 11; // A4 height in inches
            settings.MarginTop = 0.6;
            settings.MarginBottom = 0.6;
            settings.MarginLeft = 0.6;
            settings.MarginRight = 0.4;
            settings.ShouldPrintBackgrounds = true;
            settings.ShouldPrintSelectionOnly = false;
            settings.ShouldPrintHeaderAndFooter = true;
            settings.PageRanges = "1-";
            settings.PagesPerSide = 1;
            settings.Copies = 1;
            settings.Collation = CoreWebView2PrintCollation.Collated;
            settings.ColorMode = CoreWebView2PrintColorMode.Color;
            settings.Duplex = CoreWebView2PrintDuplex.OneSided;
            settings.MediaSize = CoreWebView2PrintMediaSize.Default;
            settings.HeaderTitle = "Created with Calcpad";
            settings.FooterUri = "";
            settings.PrinterName = "Calcpad";
            return settings;
        }
    }
}