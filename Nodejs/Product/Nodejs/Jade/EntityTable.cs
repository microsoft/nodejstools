// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Jade
{
    internal static class EntityTable
    {
        public static Dictionary<string, char> Entities { get; }

        public static bool IsEntity(string candidate, out char mappedChar)
        {
            return Entities.TryGetValue(candidate, out mappedChar);
        }

        public static ICollection<string> EntityNames => Entities.Keys;
        static EntityTable()
        {
            Entities = new Dictionary<string, char>
            {
                { "quot", '"' },
                { "amp", '&' },
                { "lt", '<' },
                { "gt", '>' },
                { "nbsp", ' ' },

                { "iexcl", (char)161 },
                { "cent", (char)162 },
                { "pound", (char)163 },
                { "curren", (char)164 },
                { "yen", (char)165 },
                { "brvbar", (char)166 },

                { "sect", (char)167 },
                { "uml", (char)168 },
                { "copy", (char)169 },
                { "ordf", (char)170 },
                { "laquo", (char)171 },
                { "not", (char)172 },

                { "shy", (char)173 },
                { "reg", (char)174 },
                { "macr", (char)175 },
                { "deg", (char)176 },
                { "plusmn", (char)177 },
                { "sup2", (char)178 },

                { "sup3", (char)179 },
                { "acute", (char)180 },
                { "micro", (char)181 },
                { "para", (char)182 },
                { "middot", (char)183 },
                { "cedil", (char)184 },

                { "sup1", (char)185 },
                { "ordm", (char)186 },
                { "raquo", (char)187 },
                { "frac14", (char)188 },
                { "frac12", (char)189 },
                { "frac34", (char)190 },

                { "iquest", (char)191 },
                { "Agrave", (char)192 },
                { "Aacute", (char)193 },
                { "Acirc", (char)194 },
                { "Atilde", (char)195 },
                { "Auml", (char)196 },

                { "Aring", (char)197 },
                { "AElig", (char)198 },
                { "Ccedil", (char)199 },
                { "Egrave", (char)200 },
                { "Eacute", (char)201 },
                { "Ecirc", (char)202 },

                { "Euml", (char)203 },
                { "Igrave", (char)204 },
                { "Iacute", (char)205 },
                { "Icirc", (char)206 },
                { "Iuml", (char)207 },
                { "ETH", (char)208 },

                { "Ntilde", (char)209 },
                { "Ograve", (char)210 },
                { "Oacute", (char)211 },
                { "Ocirc", (char)212 },
                { "Otilde", (char)213 },
                { "Ouml", (char)214 },

                { "times", (char)215 },
                { "Oslash", (char)216 },
                { "Ugrave", (char)217 },
                { "Uacute", (char)218 },
                { "Ucirc", (char)219 },
                { "Uuml", (char)220 },

                { "Yacute", (char)221 },
                { "THORN", (char)222 },
                { "szlig", (char)223 },
                { "agrave", (char)224 },
                { "aacute", (char)225 },
                { "acirc", (char)226 },

                { "atilde", (char)227 },
                { "auml", (char)228 },
                { "aring", (char)229 },
                { "aelig", (char)230 },
                { "ccedil", (char)231 },
                { "egrave", (char)232 },

                { "eacute", (char)233 },
                { "ecirc", (char)234 },
                { "euml", (char)235 },
                { "igrave", (char)236 },
                { "iacute", (char)237 },
                { "icirc", (char)238 },

                { "iuml", (char)239 },
                { "eth", (char)240 },
                { "ntilde", (char)241 },
                { "ograve", (char)242 },
                { "oacute", (char)243 },
                { "ocirc", (char)244 },

                { "otilde", (char)245 },
                { "ouml", (char)246 },
                { "divide", (char)247 },
                { "oslash", (char)248 },
                { "ugrave", (char)249 },
                { "uacute", (char)250 },

                { "ucirc", (char)251 },
                { "uuml", (char)252 },
                { "yacute", (char)253 },
                { "thorn", (char)254 },
                { "yuml", (char)255 },

                { "OElig", (char)338 }, // latin capital ligature oe, U0152 ISOlat2
                { "oelig", (char)339 }, // latin small ligature oe, U0153 ISOlat2
                { "Scaron", (char)352 }, // latin capital letter s with caron, U0160 ISOlat2
                { "scaron", (char)353 }, // latin small letter s with caron, U0161 ISOlat2
                { "Yuml", (char)376 }, // latin capital letter y with diaeresis, U0178 ISOlat2

                { "fnof", (char)402 }, // latin small f with hook, =function, =florin, U0192 ISOtech
                { "circ", (char)710 }, // modifier letter circumflex accent, U02C6 ISOpub
                { "tilde", (char)732 }, // small tilde, U02DC ISOdia
                { "Alpha", (char)913 }, // greek capital letter alpha
                { "Beta", (char)914 }, // greek capital letter beta
                { "Gamma", (char)915 }, // greek capital letter gamma

                { "Delta", (char)916 }, // greek capital letter delta
                { "Epsilon", (char)917 }, // greek capital letter epsilon
                { "Zeta", (char)918 }, // greek capital letter zeta
                { "Eta", (char)919 }, // greek capital letter eta
                { "Theta", (char)920 }, // greek capital letter theta
                { "Iota", (char)921 }, // greek capital letter iota 

                { "Kappa", (char)922 }, // greek capital letter kappa
                { "Lambda", (char)923 }, // greek capital letter lambda
                { "Mu", (char)924 }, // greek capital letter mu
                { "Nu", (char)925 }, // greek capital letter nu
                { "Xi", (char)926 }, // greek capital letter xi
                { "Omicron", (char)927 }, // greek capital letter omicron

                { "Pi", (char)928 }, // greek capital letter pi
                { "Rho", (char)929 }, // greek capital letter rho
                { "Sigma", (char)931 }, // greek capital letter sigma
                { "Tau", (char)932 }, // greek capital letter tau
                { "Upsilon", (char)933 }, // greek capital letter upsilon
                { "Phi", (char)934 }, // greek capital letter phi

                { "Chi", (char)935 }, // greek capital letter chi
                { "Psi", (char)936 }, // greek capital letter psi   
                { "Omega", (char)937 }, // greek capital letter omega
                { "alpha", (char)945 }, // greek small letter alpha
                { "beta", (char)946 }, // greek small letter beta
                { "gamma", (char)947 }, // greek small letter gamma

                { "delta", (char)948 }, // greek small letter delta
                { "epsilon", (char)949 }, // greek small letter epsilon
                { "zeta", (char)950 }, // greek small letter zeta
                { "eta", (char)951 }, // greek small letter eta
                { "theta", (char)952 }, // greek small letter theta
                { "iota", (char)953 }, // greek small letter iota 

                { "kappa", (char)954 }, // greek small letter kappa
                { "lambda", (char)955 }, // greek small letter lambda
                { "mu", (char)956 }, // greek small letter mu
                { "nu", (char)957 }, // greek small letter nu
                { "xi", (char)958 }, // greek small letter xi
                { "omicron", (char)959 }, // greek small letter omicron

                { "pi", (char)960 }, // greek small letter pi
                { "rho", (char)961 }, // greek small letter rho
                { "sigmaf", (char)962 }, // greek small final sigma
                { "sigma", (char)963 }, // greek small letter sigma
                { "tau", (char)964 }, // greek small letter tau
                { "upsilon", (char)965 }, // greek small letter upsilon

                { "phi", (char)966 }, // greek small letter phi
                { "chi", (char)967 }, // greek small letter chi
                { "psi", (char)968 }, // greek small letter psi   
                { "omega", (char)969 }, // greek small letter omega
                { "thetasym", (char)977 }, // greek small letter theta symbol, U03D1 NEW
                { "upsih", (char)978 }, // greek upsilon with hook symbol

                { "piv", (char)982 }, // greek pi symbol
                { "ensp", (char)8194 }, // en space, U2002 ISOpub
                { "emsp", (char)8195 }, // em space, U2003 ISOpub
                { "thinsp", (char)8201 }, // thin space, U2009 ISOpub
                { "zwsp", (char)8203 }, // zero width space, U200B NEW RFC 2070
                { "zwnj", (char)8204 }, // zero width non-joiner, U200C NEW RFC 2070

                { "zwj", (char)8205 }, // zero width joiner, U200D NEW RFC 2070
                { "lrm", (char)8206 }, // left-to-right mark, U200E NEW RFC 2070
                { "rlm", (char)8207 }, // right-to-left mark, U200F NEW RFC 2070
                { "ndash", (char)8211 }, // en dash, U2013 ISOpub
                { "mdash", (char)8212 }, // em dash, U2014 ISOpub
                { "lsquo", (char)8216 }, // left single quotation mark, U2018 ISOnum

                { "rsquo", (char)8217 }, // right single quotation mark, U2019 ISOnum
                { "sbquo", (char)8218 }, // single low-9 quotation mark, U201A NEW
                { "ldquo", (char)8220 }, // left double quotation mark, U201C ISOnum
                { "rdquo", (char)8221 }, // right double quotation mark, U201D ISOnum
                { "bdquo", (char)8222 }, // double low-9 quotation mark, U201E NEW
                { "dagger", (char)8224 }, // dagger, U2020 ISOpub

                { "Dagger", (char)8225 }, // double dagger, U2021 ISOpub
                { "bull", (char)8226 }, // bullet, =black small circle, U2022 ISOpub
                { "hellip", (char)8230 }, // horizontal ellipsis, =three dot leader, U2026 ISOpub
                { "lre", (char)8234 }, // Left-to-right embedding, U200F NEW RFC 2070
                { "rle", (char)8235 }, // Right-to-left embedding, U200F NEW RFC 2070
                { "pdf", (char)8236 }, // Pop direction format, U200F NEW RFC 2070

                { "lro", (char)8237 }, // Left-to-right override, U200F NEW RFC 2070
                { "rlo", (char)8238 }, // Right-to-left override, U200F NEW RFC 2070
                { "permil", (char)8240 }, // per mille sign, U2030 ISOtech
                { "prime", (char)8242 }, // prime, =minutes, =feet, U2032 ISOtech
                { "Prime", (char)8243 }, // double prime, =seconds, =inches, U2033 ISOtech
                { "lsaquo", (char)8249 }, // single left-pointing angle quotation mark, U2039 ISO proposed

                { "rsaquo", (char)8250 }, // single right-pointing angle quotation mark, U203A ISO proposed
                { "oline", (char)8254 }, // overline, spacing overscore
                { "frasl", (char)8260 }, // fraction slash
                { "iss", (char)8298 }, // Inhibit symmetric, U200F NEW RFC 2070swapping
                { "ass", (char)8299 }, // Activate symmetric, U200F NEW RFC 2070 swapping
                { "iafs", (char)8300 }, // Inhibit Arabic form, U200F NEW RFC 2070 shaping

                { "aafs", (char)8301 }, // Activate Arabic form, U200F NEW RFC 2070 shaping
                { "nads", (char)8302 }, // National digit shapes, U200F NEW RFC 2070
                { "nods", (char)8303 }, // Nominal digit shapes, U200F NEW RFC 2070
                { "euro", (char)8364 }, // Euro, U20AC
                { "image", (char)8465 }, // blackletter capital I, =imaginary part, U2111 ISOamso 
                { "weierp", (char)8472 }, // script capital P, =power set, =Weierstrass p, U2118 ISOamso 

                { "real", (char)8476 }, // blackletter capital R, =real part symbol, U211C ISOamso 
                { "trade", (char)8482 }, // trade mark sign, U2122 ISOnum 
                { "alefsym", (char)8501 }, // alef symbol, =first transfinite cardinal, U2135 NEW 
                { "larr", (char)8592 }, // leftwards arrow, U2190 ISOnum 
                { "uarr", (char)8593 }, // upwards arrow, U2191 ISOnum
                { "rarr", (char)8594 }, // rightwards arrow, U2192 ISOnum 

                { "darr", (char)8595 }, // downwards arrow, U2193 ISOnum 
                { "harr", (char)8596 }, // left right arrow, U2194 ISOamsa 
                { "crarr", (char)8629 }, // downwards arrow with corner leftwards, =carriage return, U21B5 NEW 
                { "lArr", (char)8656 }, // leftwards double arrow, U21D0 ISOtech 
                { "uArr", (char)8657 }, // upwards double arrow, U21D1 ISOamsa 
                { "rArr", (char)8658 }, // rightwards double arrow, U21D2 ISOtech 

                { "dArr", (char)8659 }, // downwards double arrow, U21D3 ISOamsa 
                { "hArr", (char)8660 }, // left right double arrow, U21D4 ISOamsa 
                { "forall", (char)8704 }, // for all, U2200 ISOtech 
                { "part", (char)8706 }, // partial differential, U2202 ISOtech  
                { "exist", (char)8707 }, // there exists, U2203 ISOtech 
                { "empty", (char)8709 }, // empty set, =null set, =diameter, U2205 ISOamso 

                { "nabla", (char)8711 }, // nabla, =backward difference, U2207 ISOtech 
                { "isin", (char)8712 }, // element of, U2208 ISOtech 
                { "notin", (char)8713 }, // not an element of, U2209 ISOtech 
                { "ni", (char)8715 }, // contains as member, U220B ISOtech 
                { "prod", (char)8719 }, // n-ary product, =product sign, U220F ISOamsb 
                { "sum", (char)8721 }, // n-ary sumation, U2211 ISOamsb 

                { "minus", (char)8722 }, // minus sign, U2212 ISOtech 
                { "lowast", (char)8727 }, // asterisk operator, U2217 ISOtech 
                { "radic", (char)8730 }, // square root, =radical sign, U221A ISOtech 
                { "prop", (char)8733 }, // proportional to, U221D ISOtech 
                { "infin", (char)8734 }, // infinity, U221E ISOtech 
                { "ang", (char)8736 }, // angle, U2220 ISOamso 

                { "and", (char)8743 }, // logical and, =wedge, U2227 ISOtech 
                { "or", (char)8744 }, // logical or, =vee, U2228 ISOtech 
                { "cap", (char)8745 }, // intersection, =cap, U2229 ISOtech 
                { "cup", (char)8746 }, // union, =cup, U222A ISOtech 
                { "int", (char)8747 }, // integral, U222B ISOtech 
                { "there4", (char)8756 }, // therefore, U2234 ISOtech 

                { "sim", (char)8764 }, // tilde operator, =varies with, =similar to, U223C ISOtech 
                { "cong", (char)8773 }, // approximately equal to, U2245 ISOtech 
                { "asymp", (char)8776 }, // almost equal to, =asymptotic to, U2248 ISOamsr 
                { "ne", (char)8800 }, // not equal to, U2260 ISOtech 
                { "equiv", (char)8801 }, // identical to, U2261 ISOtech 
                { "le", (char)8804 }, // less-than or equal to, U2264 ISOtech 

                { "ge", (char)8805 }, // greater-than or equal to, U2265 ISOtech 
                { "sub", (char)8834 }, // subset of, U2282 ISOtech 
                { "sup", (char)8835 }, // superset of, U2283 ISOtech 
                { "nsub", (char)8836 }, // not a subset of, U2284 ISOamsn 
                { "sube", (char)8838 }, // subset of or equal to, U2286 ISOtech 
                { "supe", (char)8839 }, // superset of or equal to, U2287 ISOtech 

                { "oplus", (char)8853 }, // circled plus, =direct sum, U2295 ISOamsb 
                { "otimes", (char)8855 }, // circled times, =vector product, U2297 ISOamsb 
                { "perp", (char)8869 }, // up tack, =orthogonal to, =perpendicular, U22A5 ISOtech 
                { "sdot", (char)8901 }, // dot operator, U22C5 ISOamsb 
                { "lceil", (char)8968 }, // left ceiling, =apl upstile, U230(char)ISOamsc  
                { "rceil", (char)8969 }, // right ceiling, U2309, ISOamsc  

                { "lfloor", (char)8970 }, // left floor, =apl downstile, U230A, ISOamsc  
                { "rfloor", (char)8971 }, // right floor, U230B, ISOamsc  
                { "lang", (char)9001 }, // left-pointing angle bracket, =bra, U2329 ISOtech 
                { "rang", (char)9002 }, // right-pointing angle bracket, =ket, U232A ISOtech 
                { "loz", (char)9674 }, // lozenge, U25CA ISOpub 
                { "spades", (char)9824 }, // black spade suit, U2660 ISOpub 

                { "clubs", (char)9827 }, // black club suit, =shamrock, U2663 ISOpub 
                { "hearts", (char)9829 }, // black heart suit, =valentine, U2665 ISOpub 
                { "diams", (char)9830 } // black diamond suit, U2666 ISOpub 
            };
        }
    }
}
