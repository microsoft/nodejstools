//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Jade
{
    internal static class EntityTable
    {
        public static Dictionary<string, char> Entities { get; private set; }

        public static bool IsEntity(string candidate, out char mappedChar)
        {
            return Entities.TryGetValue(candidate, out mappedChar);
        }

        public static ICollection<string> EntityNames
        {
            get { return Entities.Keys; }
        }

        static EntityTable()
        {
            Entities = new Dictionary<string, char>();

            Entities.Add("quot", '"');
            Entities.Add("amp", '&');
            Entities.Add("lt", '<');
            Entities.Add("gt", '>');
            Entities.Add("nbsp", ' ');

            Entities.Add("iexcl", (char)161);
            Entities.Add("cent", (char)162);
            Entities.Add("pound", (char)163);
            Entities.Add("curren", (char)164);
            Entities.Add("yen", (char)165);
            Entities.Add("brvbar", (char)166);

            Entities.Add("sect", (char)167);
            Entities.Add("uml", (char)168);
            Entities.Add("copy", (char)169);
            Entities.Add("ordf", (char)170);
            Entities.Add("laquo", (char)171);
            Entities.Add("not", (char)172);

            Entities.Add("shy", (char)173);
            Entities.Add("reg", (char)174);
            Entities.Add("macr", (char)175);
            Entities.Add("deg", (char)176);
            Entities.Add("plusmn", (char)177);
            Entities.Add("sup2", (char)178);

            Entities.Add("sup3", (char)179);
            Entities.Add("acute", (char)180);
            Entities.Add("micro", (char)181);
            Entities.Add("para", (char)182);
            Entities.Add("middot", (char)183);
            Entities.Add("cedil", (char)184);

            Entities.Add("sup1", (char)185);
            Entities.Add("ordm", (char)186);
            Entities.Add("raquo", (char)187);
            Entities.Add("frac14", (char)188);
            Entities.Add("frac12", (char)189);
            Entities.Add("frac34", (char)190);

            Entities.Add("iquest", (char)191);
            Entities.Add("Agrave", (char)192);
            Entities.Add("Aacute", (char)193);
            Entities.Add("Acirc", (char)194);
            Entities.Add("Atilde", (char)195);
            Entities.Add("Auml", (char)196);

            Entities.Add("Aring", (char)197);
            Entities.Add("AElig", (char)198);
            Entities.Add("Ccedil", (char)199);
            Entities.Add("Egrave", (char)200);
            Entities.Add("Eacute", (char)201);
            Entities.Add("Ecirc", (char)202);

            Entities.Add("Euml", (char)203);
            Entities.Add("Igrave", (char)204);
            Entities.Add("Iacute", (char)205);
            Entities.Add("Icirc", (char)206);
            Entities.Add("Iuml", (char)207);
            Entities.Add("ETH", (char)208);

            Entities.Add("Ntilde", (char)209);
            Entities.Add("Ograve", (char)210);
            Entities.Add("Oacute", (char)211);
            Entities.Add("Ocirc", (char)212);
            Entities.Add("Otilde", (char)213);
            Entities.Add("Ouml", (char)214);

            Entities.Add("times", (char)215);
            Entities.Add("Oslash", (char)216);
            Entities.Add("Ugrave", (char)217);
            Entities.Add("Uacute", (char)218);
            Entities.Add("Ucirc", (char)219);
            Entities.Add("Uuml", (char)220);

            Entities.Add("Yacute", (char)221);
            Entities.Add("THORN", (char)222);
            Entities.Add("szlig", (char)223);
            Entities.Add("agrave", (char)224);
            Entities.Add("aacute", (char)225);
            Entities.Add("acirc", (char)226);

            Entities.Add("atilde", (char)227);
            Entities.Add("auml", (char)228);
            Entities.Add("aring", (char)229);
            Entities.Add("aelig", (char)230);
            Entities.Add("ccedil", (char)231);
            Entities.Add("egrave", (char)232);

            Entities.Add("eacute", (char)233);
            Entities.Add("ecirc", (char)234);
            Entities.Add("euml", (char)235);
            Entities.Add("igrave", (char)236);
            Entities.Add("iacute", (char)237);
            Entities.Add("icirc", (char)238);

            Entities.Add("iuml", (char)239);
            Entities.Add("eth", (char)240);
            Entities.Add("ntilde", (char)241);
            Entities.Add("ograve", (char)242);
            Entities.Add("oacute", (char)243);
            Entities.Add("ocirc", (char)244);

            Entities.Add("otilde", (char)245);
            Entities.Add("ouml", (char)246);
            Entities.Add("divide", (char)247);
            Entities.Add("oslash", (char)248);
            Entities.Add("ugrave", (char)249);
            Entities.Add("uacute", (char)250);

            Entities.Add("ucirc", (char)251);
            Entities.Add("uuml", (char)252);
            Entities.Add("yacute", (char)253);
            Entities.Add("thorn", (char)254);
            Entities.Add("yuml", (char)255);

            Entities.Add("OElig", (char)338); // latin capital ligature oe, U0152 ISOlat2
            Entities.Add("oelig", (char)339); // latin small ligature oe, U0153 ISOlat2
            Entities.Add("Scaron", (char)352); // latin capital letter s with caron, U0160 ISOlat2
            Entities.Add("scaron", (char)353); // latin small letter s with caron, U0161 ISOlat2
            Entities.Add("Yuml", (char)376); // latin capital letter y with diaeresis, U0178 ISOlat2

            Entities.Add("fnof", (char)402); // latin small f with hook, =function, =florin, U0192 ISOtech
            Entities.Add("circ", (char)710); // modifier letter circumflex accent, U02C6 ISOpub
            Entities.Add("tilde", (char)732); // small tilde, U02DC ISOdia
            Entities.Add("Alpha", (char)913); // greek capital letter alpha
            Entities.Add("Beta", (char)914); // greek capital letter beta
            Entities.Add("Gamma", (char)915); // greek capital letter gamma

            Entities.Add("Delta", (char)916); // greek capital letter delta
            Entities.Add("Epsilon", (char)917); // greek capital letter epsilon
            Entities.Add("Zeta", (char)918); // greek capital letter zeta
            Entities.Add("Eta", (char)919); // greek capital letter eta
            Entities.Add("Theta", (char)920); // greek capital letter theta
            Entities.Add("Iota", (char)921); // greek capital letter iota 

            Entities.Add("Kappa", (char)922); // greek capital letter kappa
            Entities.Add("Lambda", (char)923); // greek capital letter lambda
            Entities.Add("Mu", (char)924); // greek capital letter mu
            Entities.Add("Nu", (char)925); // greek capital letter nu
            Entities.Add("Xi", (char)926); // greek capital letter xi
            Entities.Add("Omicron", (char)927); // greek capital letter omicron

            Entities.Add("Pi", (char)928); // greek capital letter pi
            Entities.Add("Rho", (char)929); // greek capital letter rho
            Entities.Add("Sigma", (char)931); // greek capital letter sigma
            Entities.Add("Tau", (char)932); // greek capital letter tau
            Entities.Add("Upsilon", (char)933); // greek capital letter upsilon
            Entities.Add("Phi", (char)934); // greek capital letter phi

            Entities.Add("Chi", (char)935); // greek capital letter chi
            Entities.Add("Psi", (char)936); // greek capital letter psi   
            Entities.Add("Omega", (char)937); // greek capital letter omega
            Entities.Add("alpha", (char)945); // greek small letter alpha
            Entities.Add("beta", (char)946); // greek small letter beta
            Entities.Add("gamma", (char)947); // greek small letter gamma

            Entities.Add("delta", (char)948); // greek small letter delta
            Entities.Add("epsilon", (char)949); // greek small letter epsilon
            Entities.Add("zeta", (char)950); // greek small letter zeta
            Entities.Add("eta", (char)951); // greek small letter eta
            Entities.Add("theta", (char)952); // greek small letter theta
            Entities.Add("iota", (char)953); // greek small letter iota 

            Entities.Add("kappa", (char)954); // greek small letter kappa
            Entities.Add("lambda", (char)955); // greek small letter lambda
            Entities.Add("mu", (char)956); // greek small letter mu
            Entities.Add("nu", (char)957); // greek small letter nu
            Entities.Add("xi", (char)958); // greek small letter xi
            Entities.Add("omicron", (char)959); // greek small letter omicron

            Entities.Add("pi", (char)960); // greek small letter pi
            Entities.Add("rho", (char)961); // greek small letter rho
            Entities.Add("sigmaf", (char)962); // greek small final sigma
            Entities.Add("sigma", (char)963); // greek small letter sigma
            Entities.Add("tau", (char)964); // greek small letter tau
            Entities.Add("upsilon", (char)965); // greek small letter upsilon

            Entities.Add("phi", (char)966); // greek small letter phi
            Entities.Add("chi", (char)967); // greek small letter chi
            Entities.Add("psi", (char)968); // greek small letter psi   
            Entities.Add("omega", (char)969); // greek small letter omega
            Entities.Add("thetasym", (char)977); // greek small letter theta symbol, U03D1 NEW
            Entities.Add("upsih", (char)978); // greek upsilon with hook symbol

            Entities.Add("piv", (char)982); // greek pi symbol
            Entities.Add("ensp", (char)8194); // en space, U2002 ISOpub
            Entities.Add("emsp", (char)8195); // em space, U2003 ISOpub
            Entities.Add("thinsp", (char)8201); // thin space, U2009 ISOpub
            Entities.Add("zwsp", (char)8203); // zero width space, U200B NEW RFC 2070
            Entities.Add("zwnj", (char)8204); // zero width non-joiner, U200C NEW RFC 2070

            Entities.Add("zwj", (char)8205); // zero width joiner, U200D NEW RFC 2070
            Entities.Add("lrm", (char)8206); // left-to-right mark, U200E NEW RFC 2070
            Entities.Add("rlm", (char)8207); // right-to-left mark, U200F NEW RFC 2070
            Entities.Add("ndash", (char)8211); // en dash, U2013 ISOpub
            Entities.Add("mdash", (char)8212); // em dash, U2014 ISOpub
            Entities.Add("lsquo", (char)8216); // left single quotation mark, U2018 ISOnum

            Entities.Add("rsquo", (char)8217); // right single quotation mark, U2019 ISOnum
            Entities.Add("sbquo", (char)8218); // single low-9 quotation mark, U201A NEW
            Entities.Add("ldquo", (char)8220); // left double quotation mark, U201C ISOnum
            Entities.Add("rdquo", (char)8221); // right double quotation mark, U201D ISOnum
            Entities.Add("bdquo", (char)8222); // double low-9 quotation mark, U201E NEW
            Entities.Add("dagger", (char)8224); // dagger, U2020 ISOpub

            Entities.Add("Dagger", (char)8225); // double dagger, U2021 ISOpub
            Entities.Add("bull", (char)8226); // bullet, =black small circle, U2022 ISOpub
            Entities.Add("hellip", (char)8230); // horizontal ellipsis, =three dot leader, U2026 ISOpub
            Entities.Add("lre", (char)8234); // Left-to-right embedding, U200F NEW RFC 2070
            Entities.Add("rle", (char)8235); // Right-to-left embedding, U200F NEW RFC 2070
            Entities.Add("pdf", (char)8236); // Pop direction format, U200F NEW RFC 2070

            Entities.Add("lro", (char)8237); // Left-to-right override, U200F NEW RFC 2070
            Entities.Add("rlo", (char)8238); // Right-to-left override, U200F NEW RFC 2070
            Entities.Add("permil", (char)8240); // per mille sign, U2030 ISOtech
            Entities.Add("prime", (char)8242); // prime, =minutes, =feet, U2032 ISOtech
            Entities.Add("Prime", (char)8243); // double prime, =seconds, =inches, U2033 ISOtech
            Entities.Add("lsaquo", (char)8249); // single left-pointing angle quotation mark, U2039 ISO proposed

            Entities.Add("rsaquo", (char)8250); // single right-pointing angle quotation mark, U203A ISO proposed
            Entities.Add("oline", (char)8254); // overline, spacing overscore
            Entities.Add("frasl", (char)8260); // fraction slash
            Entities.Add("iss", (char)8298); // Inhibit symmetric, U200F NEW RFC 2070swapping
            Entities.Add("ass", (char)8299); // Activate symmetric, U200F NEW RFC 2070 swapping
            Entities.Add("iafs", (char)8300); // Inhibit Arabic form, U200F NEW RFC 2070 shaping

            Entities.Add("aafs", (char)8301); // Activate Arabic form, U200F NEW RFC 2070 shaping
            Entities.Add("nads", (char)8302); // National digit shapes, U200F NEW RFC 2070
            Entities.Add("nods", (char)8303); // Nominal digit shapes, U200F NEW RFC 2070
            Entities.Add("euro", (char)8364); // Euro, U20AC
            Entities.Add("image", (char)8465); // blackletter capital I, =imaginary part, U2111 ISOamso 
            Entities.Add("weierp", (char)8472); // script capital P, =power set, =Weierstrass p, U2118 ISOamso 

            Entities.Add("real", (char)8476); // blackletter capital R, =real part symbol, U211C ISOamso 
            Entities.Add("trade", (char)8482); // trade mark sign, U2122 ISOnum 
            Entities.Add("alefsym", (char)8501); // alef symbol, =first transfinite cardinal, U2135 NEW 
            Entities.Add("larr", (char)8592); // leftwards arrow, U2190 ISOnum 
            Entities.Add("uarr", (char)8593); // upwards arrow, U2191 ISOnum
            Entities.Add("rarr", (char)8594); // rightwards arrow, U2192 ISOnum 

            Entities.Add("darr", (char)8595); // downwards arrow, U2193 ISOnum 
            Entities.Add("harr", (char)8596); // left right arrow, U2194 ISOamsa 
            Entities.Add("crarr", (char)8629); // downwards arrow with corner leftwards, =carriage return, U21B5 NEW 
            Entities.Add("lArr", (char)8656); // leftwards double arrow, U21D0 ISOtech 
            Entities.Add("uArr", (char)8657); // upwards double arrow, U21D1 ISOamsa 
            Entities.Add("rArr", (char)8658); // rightwards double arrow, U21D2 ISOtech 

            Entities.Add("dArr", (char)8659); // downwards double arrow, U21D3 ISOamsa 
            Entities.Add("hArr", (char)8660); // left right double arrow, U21D4 ISOamsa 
            Entities.Add("forall", (char)8704); // for all, U2200 ISOtech 
            Entities.Add("part", (char)8706); // partial differential, U2202 ISOtech  
            Entities.Add("exist", (char)8707); // there exists, U2203 ISOtech 
            Entities.Add("empty", (char)8709); // empty set, =null set, =diameter, U2205 ISOamso 

            Entities.Add("nabla", (char)8711); // nabla, =backward difference, U2207 ISOtech 
            Entities.Add("isin", (char)8712); // element of, U2208 ISOtech 
            Entities.Add("notin", (char)8713); // not an element of, U2209 ISOtech 
            Entities.Add("ni", (char)8715); // contains as member, U220B ISOtech 
            Entities.Add("prod", (char)8719); // n-ary product, =product sign, U220F ISOamsb 
            Entities.Add("sum", (char)8721); // n-ary sumation, U2211 ISOamsb 

            Entities.Add("minus", (char)8722); // minus sign, U2212 ISOtech 
            Entities.Add("lowast", (char)8727); // asterisk operator, U2217 ISOtech 
            Entities.Add("radic", (char)8730); // square root, =radical sign, U221A ISOtech 
            Entities.Add("prop", (char)8733); // proportional to, U221D ISOtech 
            Entities.Add("infin", (char)8734); // infinity, U221E ISOtech 
            Entities.Add("ang", (char)8736); // angle, U2220 ISOamso 

            Entities.Add("and", (char)8743); // logical and, =wedge, U2227 ISOtech 
            Entities.Add("or", (char)8744); // logical or, =vee, U2228 ISOtech 
            Entities.Add("cap", (char)8745); // intersection, =cap, U2229 ISOtech 
            Entities.Add("cup", (char)8746); // union, =cup, U222A ISOtech 
            Entities.Add("int", (char)8747); // integral, U222B ISOtech 
            Entities.Add("there4", (char)8756); // therefore, U2234 ISOtech 

            Entities.Add("sim", (char)8764); // tilde operator, =varies with, =similar to, U223C ISOtech 
            Entities.Add("cong", (char)8773); // approximately equal to, U2245 ISOtech 
            Entities.Add("asymp", (char)8776); // almost equal to, =asymptotic to, U2248 ISOamsr 
            Entities.Add("ne", (char)8800); // not equal to, U2260 ISOtech 
            Entities.Add("equiv", (char)8801); // identical to, U2261 ISOtech 
            Entities.Add("le", (char)8804); // less-than or equal to, U2264 ISOtech 

            Entities.Add("ge", (char)8805); // greater-than or equal to, U2265 ISOtech 
            Entities.Add("sub", (char)8834); // subset of, U2282 ISOtech 
            Entities.Add("sup", (char)8835); // superset of, U2283 ISOtech 
            Entities.Add("nsub", (char)8836); // not a subset of, U2284 ISOamsn 
            Entities.Add("sube", (char)8838); // subset of or equal to, U2286 ISOtech 
            Entities.Add("supe", (char)8839); // superset of or equal to, U2287 ISOtech 

            Entities.Add("oplus", (char)8853); // circled plus, =direct sum, U2295 ISOamsb 
            Entities.Add("otimes", (char)8855); // circled times, =vector product, U2297 ISOamsb 
            Entities.Add("perp", (char)8869); // up tack, =orthogonal to, =perpendicular, U22A5 ISOtech 
            Entities.Add("sdot", (char)8901); // dot operator, U22C5 ISOamsb 
            Entities.Add("lceil", (char)8968); // left ceiling, =apl upstile, U230(char)ISOamsc  
            Entities.Add("rceil", (char)8969); // right ceiling, U2309, ISOamsc  

            Entities.Add("lfloor", (char)8970); // left floor, =apl downstile, U230A, ISOamsc  
            Entities.Add("rfloor", (char)8971); // right floor, U230B, ISOamsc  
            Entities.Add("lang", (char)9001); // left-pointing angle bracket, =bra, U2329 ISOtech 
            Entities.Add("rang", (char)9002); // right-pointing angle bracket, =ket, U232A ISOtech 
            Entities.Add("loz", (char)9674); // lozenge, U25CA ISOpub 
            Entities.Add("spades", (char)9824); // black spade suit, U2660 ISOpub 

            Entities.Add("clubs", (char)9827); // black club suit, =shamrock, U2663 ISOpub 
            Entities.Add("hearts", (char)9829); // black heart suit, =valentine, U2665 ISOpub 
            Entities.Add("diams", (char)9830); // black diamond suit, U2666 ISOpub 
        }
    }
}
