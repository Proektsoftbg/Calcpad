using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Calcpad.web.Validation
{
    public class NoScript : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string hasScript = HasScript(value.ToString());
            if (hasScript.Length > 0)
                return new ValidationResult($"Content not allowed: \"{hasScript}\".");

            return ValidationResult.Success;
        }

        private string HasScript(string text)
        {
            StringBuilder sb = new();
            bool isTag = false, isComment = false, isAttribute = false, isImage = false; ;
            char quote = ' ';
            foreach (char c in text + ' ')
            {
                if (isComment)
                {
                    if (c == quote)
                        isComment = false;
                    else if (isTag)
                    {
                        if (c == ':' && sb.Length > 5)
                        {
                            string s = sb.ToString(sb.Length - 6, 6).ToLowerInvariant();
                            if (s == "script")
                                return sb.ToString() + c;
                        }
                        else if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '-')
                            sb.Append(c);
                        else
                        {
                            string s = sb.ToString().ToLowerInvariant();
                            sb.Clear();
                            if (c == '>' || char.IsWhiteSpace(c))
                            {
                                if (isAttribute)
                                {
                                    if (CheckAttribute(s))
                                        return s;
                                }
                                else
                                {
                                    if (tags.Contains(s))
                                        return s;
                                    isImage = s == "img";
                                    isAttribute = true;
                                }
                                if (c == '>')
                                    isTag = false;
                            }
                            else if (c == '=' && (isAttribute || s.StartsWith("on")) && CheckAttribute(s))
                                return s;
                        }
                    }
                    else if (c == '<')
                    {
                        isTag = true;
                        isAttribute = false;
                    }
                }
                else if (c == '\"' || c == '\'')
                {
                    quote = c;
                    isComment = true;
                }
            }
            return sb.ToString();

            bool CheckAttribute(string s) => (!isImage || s != "src") && attributes.Contains(s);
        }

        private static readonly HashSet<string> tags = new()
        {
            "applet",
            "base",
            "body",
            "embed",
            "frame",
            "frameset",
            "head",
            "html",
            "iframe",
            "link",
            "meta",
            "noframes",
            "noscript",
            "object",
            "param",
            "script",
            "title"
        };

        private static readonly HashSet<string> attributes = new()
        {
            "action",
            "async",
            "content",
            "data",
            "defer",
            "download",
            "form",
            "formaction",
            "fscommand",
            "http-equiv",
            "method",
            "novalidate",
            "onabort",
            "onactivate",
            "onafterprint",
            "onafterupdate",
            "onbeforeactivate",
            "onbeforecopy",
            "onbeforecut",
            "onbeforedeactivate",
            "onbeforeeditfocus",
            "onbeforepaste",
            "onbeforeprint",
            "onbeforeunload",
            "onbeforeupdate",
            "onbegin",
            "onblur",
            "onbounce",
            "oncellchange",
            "onchange",
            "onclick",
            "oncontextmenu",
            "oncontrolselect",
            "oncopy",
            "oncut",
            "ondataavailable",
            "ondatasetchanged",
            "ondatasetcomplete",
            "ondblclick",
            "ondeactivate",
            "ondrag",
            "ondragend",
            "ondragleave",
            "ondragenter",
            "ondragover",
            "ondragdrop",
            "ondragstart",
            "ondrop",
            "onend",
            "onerror",
            "onerrorupdate",
            "onfilterchange",
            "onfinish",
            "onfocus",
            "onfocusin",
            "onfocusout",
            "onhashchange",
            "onhelp",
            "oninput",
            "onkeydown",
            "onkeypress",
            "onkeyup",
            "onlayoutcomplete",
            "onload",
            "onlosecapture",
            "onmediacomplete",
            "onmediaerror",
            "onmessage",
            "onmousedown",
            "onmouseenter",
            "onmouseleave",
            "onmousemove",
            "onmouseout",
            "onmouseover",
            "onmouseup",
            "onmousewheel",
            "onmove",
            "onmoveend",
            "onmovestart",
            "onoffline",
            "ononline",
            "onoutofsync",
            "onpaste",
            "onpause",
            "onpopstate",
            "onprogress",
            "onpropertychange",
            "onreadystatechange",
            "onredo",
            "onrepeat",
            "onreset",
            "onresize",
            "onresizeend",
            "onresizestart",
            "onresume",
            "onreverse",
            "onrowsenter",
            "onrowexit",
            "onrowdelete",
            "onrowinserted",
            "onscroll",
            "onseek",
            "onselect",
            "onselectionchange",
            "onselectstart",
            "onstart",
            "onstop",
            "onstorage",
            "onsyncrestored",
            "onsubmit",
            "ontimeerror",
            "ontrackchange",
            "onundo",
            "onunload",
            "onurlflip",
            "seeksegmenttime",
            "open",
            "rel",
            "src",
            "srcdoc"
        };
    }
}
