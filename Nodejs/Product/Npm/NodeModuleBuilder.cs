/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm{
    /// <summary>
    /// Mutable class for building immutable node module descriptions
    /// </summary>
    public class NodeModuleBuilder{
        private readonly List<IPackage> _dependencies;
        private StringBuilder _descriptionBuff = new StringBuilder();
        private StringBuilder _authorBuff = new StringBuilder();

        public NodeModuleBuilder(){
            _dependencies = new List<IPackage>();
        }

        public NodeModuleBuilder(IPackage module){
            Name = module.Name;
            Author = module.Author;
            Version = module.Version;
            RequestedVersionRange = module.RequestedVersionRange;
            AppendToDescription( module.Description );
            Flags = module.Flags;
            _dependencies = new List<IPackage>();
            _dependencies.AddRange(module.Modules);
        }

        public void Reset(){
            Name = null;
            _descriptionBuff.Length = 0;
            _authorBuff.Length = 0;
        }

        public void AddAuthor(string text){
            if (_authorBuff.Length > 0){
                _authorBuff.Append(' ');
            }
            _authorBuff.Append(text);
        }

        public IPerson Author{
            get{
                var text = _authorBuff.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : new Person(text);
            }
        }

        public string Name { get; set; }

        public SemverVersion Version { get; set; }

        public void AppendToDescription(string text){
            _descriptionBuff.Append(text);
        }

        public string Description{
            get{
                var text = _descriptionBuff.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : text;
            }
        }

        public IEnumerable<IPackage> Dependencies{
            get { return _dependencies; }
        }

        public PackageFlags Flags { get; set; }

        public string RequestedVersionRange { get; set; }

        public void AddDependency(IPackage module){
            _dependencies.Add(module);
        }

        public void AddDependencies(IEnumerable<IPackage> packages){
            _dependencies.AddRange(packages);
        }

        public IPackage Build(){
            PackageProxy proxy = new PackageProxy();
            proxy.Author = Author;
            proxy.Name = Name;
            proxy.Version = Version;
            proxy.Description = Description;
            proxy.RequestedVersionRange = RequestedVersionRange;
            proxy.Flags = Flags;

            var modules = new NodeModulesProxy();
            foreach (var dep in Dependencies){
                modules.AddModule(dep);
            }
            proxy.Modules = modules;
            return proxy;
        }
    }
}