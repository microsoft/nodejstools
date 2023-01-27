// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.NodejsTools", CodeBase = "Microsoft.NodejsTools.dll", Version = "1.0.0.0")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.NodejsTools.Npm", CodeBase = "Microsoft.NodejsTools.Npm.dll", Version = "1.0.0.0")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.NodejsTools.PressAnyKey", CodeBase = "Microsoft.NodejsTools.PressAnyKey.exe", Version = "1.0.0.0")]
[assembly: ProvideCodeBase(AssemblyName = "Microsoft.NodejsTools.ProjectWizard", CodeBase = "Microsoft.NodejsTools.ProjectWizard.dll", Version = "1.0.0.0")]
[assembly: ProvideCodeBase(AssemblyName = "MigrateToJsps", CodeBase = "MigrateToJsps.dll", Version = "1.0.0.0")]

[assembly: InternalsVisibleTo("TestUtilities.NodeJS, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]
[assembly: InternalsVisibleTo("NodeTests, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]
[assembly: InternalsVisibleTo("Nodejs.Tests.UI, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

// Only import DLLs from safe directories (in particular, don't search the
// current working directory).
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)] 
