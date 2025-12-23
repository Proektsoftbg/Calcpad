using System;
using System.Collections.Generic;
using System.Text;

namespace Calcpad.Document.Core.Segments.Input
{
    public enum InputFieldType
    {
        // Variable input field, used to define variables
        // generally formatted as field = ? {value}
        Variable,

        // #Include micro input field, used to include other files
        // generally formatted as #include "filename" #{value1;value2;valueN}
        Include
    }

    /// <summary>
    /// Input line definition
    /// such as variable input, #include, #read
    /// </summary>
    /// <param name="rowIndex"></param>
    public class InputField(string[] values, string name) : StringSegment(string.Empty)
    {
        public InputFieldType Type { get; set; }

        /// <summary>
        /// field/variable name
        /// </summary>
        public string Name => name;

        public string[] Values => values;

        public void UpdateValues(string[] newValues)
        {
            // if existing values is empty, use new values length
            var length = Values.Length == 0 ? newValues.Length : Values.Length;
            if (Values.Length == 0)
                Array.Resize(ref values, length);
            Array.Copy(newValues, values, length);
        }

        public override string ToString()
        {
            switch (Type)
            {
                case InputFieldType.Variable:
                {
                    var sb = new StringBuilder();
                    sb.Append(Name);
                    sb.Append(" = ? ");
                    sb.Append('{');
                    sb.Append(string.Join(';', Values));
                    sb.Append('}');
                    return sb.ToString();
                }
                case InputFieldType.Include:
                {
                    if (values.Length > 0)
                    {
                        var sb = new StringBuilder();
                        if (!string.IsNullOrEmpty(name))
                        {
                            sb.Append(name);
                            sb.Append(' ');
                        }
                        sb.Append("#{");
                        sb.Append(string.Join(';', Values));
                        sb.Append('}');
                        return sb.ToString();
                    }
                    return string.Empty;
                }
                default:
                    return string.Empty;
            }
        }
    }
}
