using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodeTools {
    class OAProperty : EnvDTE.Property {
        private object _value;
        private readonly EnvDTE.Properties _properties;
        private readonly string _name;

        public OAProperty(EnvDTE.Properties properties, string name, object value) {
            _value = value;
            _name = name;
            _properties = properties;
        }

        #region Property Members

        public object Application {
            get { return _properties.Application; }
        }

        public EnvDTE.Properties Collection {
            get { return _properties; }
        }

        public EnvDTE.DTE DTE {
            get { return _properties.DTE; }
        }

        public string Name {
            get { return _name; }
        }

        public short NumIndices {
            get { return 1; }
        }

        public object Object {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public EnvDTE.Properties Parent {
            get { return _properties; }
        }

        public object Value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }

        public object get_IndexedValue(object Index1, [System.Runtime.InteropServices.OptionalAttribute]object Index2, [System.Runtime.InteropServices.OptionalAttribute]object Index3, [System.Runtime.InteropServices.OptionalAttribute]object Index4) {
            throw new NotImplementedException();
        }

        public void let_Value(object lppvReturn) {
            throw new NotImplementedException();
        }

        public void set_IndexedValue(object Index1, [System.Runtime.InteropServices.OptionalAttribute]object Index2, [System.Runtime.InteropServices.OptionalAttribute]object Index3, [System.Runtime.InteropServices.OptionalAttribute]object Index4, object Val) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
