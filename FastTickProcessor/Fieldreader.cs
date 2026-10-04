using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastTickProcessor
{


    // A ref struct lives only on the stack. It walks through a line and hands back
    // each '|' separated field as a slice of the original memory (no Substring, no string[]).
    public ref struct FieldReader
    {
        private ReadOnlySpan<char> _rest;

        public FieldReader(ReadOnlySpan<char> line) => _rest = line;

        public bool TryNext(out ReadOnlySpan<char> field)
        {
            if (_rest.IsEmpty)
            {
                field = default;
                return false;
            }

            int i = _rest.IndexOf('|');
            if (i < 0)
            {
                field = _rest;
                _rest = default;
            }
            else
            {
                field = _rest[..i];
                _rest = _rest[(i + 1)..];
            }
            return true;
        }
    }
}
