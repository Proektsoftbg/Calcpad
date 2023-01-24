﻿using Calcpad.OpenXml;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Calcpad.Wpf
{
    internal class WebBrowserWrapper
    {
        private readonly WebBrowser _wb;

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }
        private static readonly Guid SID_SWebBrowserApp = new("0002DF05-0000-0000-C000-000000000046");


        internal WebBrowserWrapper(WebBrowser wb)
        {
            _wb = wb;
        }

        internal string GetLinkData()
        {
            var tagName = _wb.InvokeScript("eval", "document.activeElement.tagName").ToString();
            if (tagName == "A")
            {
                var linkData = _wb.InvokeScript("eval", "document.activeElement.getAttribute('data-text')").ToString();
                if (linkData != "undefined")
                    return linkData;
            }
            return null;
        }

        internal string Units
        {
            get
            {
                var units = _wb.InvokeScript("eval", "$('#Units').val();").ToString();
                if (units is "cm" or "mm")
                    return units;

                return "m";
            }

            set => _wb.InvokeScript("eval", $"$('#Units').val('{value}');");
        }

        internal double ScrollY
        {
            get => double.Parse(_wb.InvokeScript("eval", "window.pageYOffset;").ToString());
            set => _wb.InvokeScript("eval", $"window.scrollTo(0, {value});");
        }

        internal double GetVerticalPosition(int line) =>
             (double)_wb.InvokeScript("eval", $"getVerticalPosition({line});");

        internal bool IsContextMenu
        {
            get
            {
                bool result = false;
                try
                {
                    result = (bool)_wb.InvokeScript("eval", "if(typeof(contextMenu)!='undefined'){contextMenu;}else{false;}");
                }
                catch { }
                return result;
            }
        }

        internal void Scroll(int line, double offset)
        {
            try
            {
                _wb.InvokeScript("eval", $"var e = document.getElementById('line-{line}'); if(e){{window.scrollTo(0, e.offsetTop - {offset});}}");
            }
            catch { }
        }
        internal void SetContent(int line, string content)
        {
            try
            {
                _wb.InvokeScript("eval", $"var e = document.getElementById('line-{line}'); if(e){{e.innerHTML='{content}';}}");
            }
            catch { }
        }

        internal void ClearHighlight() =>
             _wb.InvokeScript("eval", $"$(\".eq\").hover(function(){{$(this).css(\"background\",\"none\");}});");

        internal string ExportOpenXml(string path)
        {
            var html = _wb.InvokeScript("eval", "document.body.outerHTML").ToString();
            html = GetHtmlData(html);
            return new OpenXmlWriter().Convert(html, path);
        }

        internal void PrintPreview()
        {
            IServiceProvider serviceProvider = null;
            if (_wb.Document is not null)
            {
                serviceProvider = (IServiceProvider)_wb.Document;
            }

            Guid serviceGuid = SID_SWebBrowserApp;
            Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;

            object NullValue = null;
            SHDocVw.IWebBrowser2 target = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);
            target.ExecWB(SHDocVw.OLECMDID.OLECMDID_PRINTPREVIEW, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref NullValue, ref NullValue);
        }

        internal void ClipboardCopy()
        {
            var dataObject = new DataObject();
            var html = _wb.InvokeScript("eval", "document.documentElement.outerHTML").ToString();
            html = GetHtmlData(html);
            dataObject.SetData(DataFormats.Html, html);
            var text = GetText();
            dataObject.SetData(DataFormats.Text, text);
            Clipboard.SetDataObject(dataObject);
        }

        private string GetText()
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
            var text = _wb.InvokeScript("eval", script);
            return text.ToString();
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

        internal string GetContents()
        {
            var html = _wb.InvokeScript("eval", "document.documentElement.outerHTML").ToString();
            return ClearHtml(html);
        }

        private static string ClearHtml(string html) => Regex.Unescape(html.Trim('"'));

        internal string[] GetInputFields()
        {
            const string script = "$(\"input[type='text'][name='Var']\").map(function(){return this.className.split('-')[1] + ':' + $(this).val();}).get().join('│');";
            var s = _wb.InvokeScript("eval", script).ToString();
            return s.Trim('"').Split('│');
        }
    }
}