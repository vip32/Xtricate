﻿namespace Xtricate.Core.Templ
{
    public class NonEscapedString
    {
        private readonly string _value;

        public NonEscapedString(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}