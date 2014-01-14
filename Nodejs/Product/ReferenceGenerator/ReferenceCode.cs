using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools {
    class ReferenceCode {
        internal const string PathRelativeBody = @"    
    // switch to forward slashes
    from = from.replace(/\\/g, '/');
    to = to.replace(/\\/g, '/');
    function fix_return(inp) {
        if(inp[inp.length - 1] == '/') {
            inp = inp.substr(0, inp.length - 1);
        }
        return inp.replace(/\//g, '\\');;
    }

    // fixup drive letters like Node
    if(from[1] == ':') {
        if(to[1] == ':') {
            if(from[0].toLowerCase() != to[0].toLowerCase()) {
                if(to[2] != '/') {
                    to = __dirname.replace(/\\/g, '/') + '/' + to.substr(2);
                }
                // fully qualified on different drives
                return fix_return(to);
            }
            if(to[2] != '/') {
                to = __dirname.replace(/\\/g, '/') + '/' + to.substr(2);
            }
        } else {
            if(to[0] != '/') {
                to = '/' + to;
            }
            to = __dirname.substr(0, 2) + to;
        }

        if(from[2] != '/' && from[0] == __dirname[0]) {
            from = __dirname.replace(/\\/g, '/') + '/' + from.substr(2);
        }
    } else if(to[1] == ':') {
        from = __dirname.substr(0, 2) + from;
        if(to[2] != '/') {
            to = __dirname.replace(/\\/g, '/') + '/' + to.substr(2);
        }
    }

    // normalize path endings
    if(from[from.length - 1] != '/') {
        from += '/';
    }
    if(to[to.length - 1] != '/') {
        to += '/';
    }    

    // compare the paths
    var i, start = -1;
    for(i = 0; i<from.length && i < to.length; i++) {
        if(from[i].toLowerCase() != to[i].toLowerCase()) {
            break;
       } else if(from[i] == '/') {
           start = i;
       }
    }
    if(start == -1 && to[1] == ':') {
        return fix_return(to);
    } else if(i == from.length && i == to.length) {
        return '';
    }
    var res = '';
    for(; i<from.length; i++) {
        if(from[i] == '/') {
            res += '../';
        }
    }
    if(res.length == 0 && to.length - 1 == start) {
        return './';
    }
    return fix_return(res + to.substr(start + 1));
";

        internal const string PathNormalizeBody = @"
    var parts = p.replace(/\\/g, '/').split('/');
    var res = [];
    for(var part = 0; part<parts.length; part++) {    
       var partText = parts[part];

       if ((partText == '' && part != 0)|| partText == '.') {
            continue;
       } else if(partText == '..' && res.length > 0) {
            res.splice(res.length - 1, 1);
       } else {
            res[res.length] = partText;
       }
    }
    var normalized = res.join('\\');
    if(p[p.length - 1] == '/' || p[p.length - 1] == '\\') {
        normalized += '\\';
    }
    return normalized;";

        internal const string PathResolveBody = @"
    var realTo = '';
    for(var i = arguments.length - 1; i>= 0; i--) {
        realTo = this.join(arguments[i], realTo);
        if(realTo[0] == '/' || realTo[0] == '\\') {
            // relative to drive which is available via __dirname
            return __dirname.substr(0, 2) + realTo;
        } else if((realTo[0].toUpperCase() >= 'A' && realTo[0].toUpperCase() <= 'Z')  && 
                  realTo[1] == ':' &&
                  (realTo[2] == '\\' || realTo[2] == '/')) {
            // absolute path
            return realTo;
        }
    }
    var res = this.join(__dirname, realTo);
    return res;";

        internal const string PathJoinBody = @"    
    var args = Array.prototype.slice.call(arguments);
    for (var i = 0; i<args.length; i++) {
        if (args[i] == '') {
            args.splice(i, 1);
        }
    }
	return this.normalize(args.join('//'));
";
    }
}
