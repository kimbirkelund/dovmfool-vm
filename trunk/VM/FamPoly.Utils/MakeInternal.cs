using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FamPoly.Utils {
    internal static class MakeInternal {
        static Regex findPublic = new Regex( @"(?<keep1>^\s*)public(?<keep2>.*(class|enum).*$)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline );
        static Regex makePartial = new Regex( @"(?<partial>partial)?(?<class>\s+class)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline );
        public static string Execute( string classDef ) {
            classDef = makePartial.Replace( classDef, " partial${class}" );
            classDef = findPublic.Replace( classDef, "${keep1}internal${keep2}" );
            return classDef;
        }
    }
}
