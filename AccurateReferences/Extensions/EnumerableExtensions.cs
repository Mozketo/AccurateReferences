using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BenClarkRobinson.AccurateReferences.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Takes an IEnumerable of T and converts it to a Comma separated string.
        /// You can override the default separated of "," if you need to change this behaviour.
        /// The trailing separated will be automatically removed.
        /// Usage: string output = list.ToCsv(i => i.Email);
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/330493/join-collection-of-objects-into-comma-separated-string"/>
        public static string ToCsv<T>(this IEnumerable<T> things, Func<T, string> toStringMethod, string seperator = ",")
        {
            var sb = new StringBuilder();
            foreach (T thing in things)
                sb.Append(toStringMethod(thing)).Append(seperator);
            return sb.ToString().TrimEnd(seperator.ToCharArray()); //remove trailing ',' (or custom separator)
        }
    }
}
