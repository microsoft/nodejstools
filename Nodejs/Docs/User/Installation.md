Installation
============

Welcome to the Node.js Tools for Visual Studio community. Node.js Tools for Visual Studio, aka NTVS, is a free/OSS plug-in that turns Visual Studio into a Node.js IDE. It only takes a few steps to get setup so let's jump right into it.

Step 1: Install Node.js
-----------------------
Node.js is built for 32-bit and 64-bit architectures. NTVS supports both. Only one is required and the Node.js installer only supports one being installed at a time.  NTVS works with Node.js v0.10.20 or later.

* Node.js (x86): [http://nodejs.org/dist/v0.10.29/node-v0.10.29-x86.msi](http://nodejs.org/dist/v0.10.29/node-v0.10.29-x86.msi)
* Node.js (x64): [http://nodejs.org/dist/v0.10.29/x64/node-v0.10.29-x64.msi](http://nodejs.org/dist/v0.10.29/x64/node-v0.10.29-x64.msi)

Step 2: Install Visual Studio
-----------------------------
NTVS is a plug-in built on top of Visual Studio 2012 or 2013. NTVS supports the free Visual Studio Express 2013 for Web, and the full VS editions of 2012 and 2013 (Professional, Premium, Ultimate):

* Visual Studio 2013: [http://www.microsoft.com/visualstudio/eng/downloads#d-2013-editions](http://www.microsoft.com/visualstudio/eng/downloads#d-2013-editions)
* Visual Studio 2012: [http://www.microsoft.com/en-us/download/details.aspx?id=30678](http://www.microsoft.com/en-us/download/details.aspx?id=30678)

After installing Visual Studio, we highly recommend installing the latest update. This is **required** for Visual Studio Express 2013 for Web.

* Visual Studio 2013 Update 2: [http://www.microsoft.com/en-us/download/details.aspx?id=42666](http://www.microsoft.com/en-us/download/details.aspx?id=42666)
* Visual Studio 2012 Update 4: [http://www.microsoft.com/en-us/download/details.aspx?id=39305](http://www.microsoft.com/en-us/download/details.aspx?id=39305)

Step 3: Install Node.js Tools for Visual Studio
-----------------------------------------------
NTVS installs via msi and takes only a few minutes. Accept the license, click install, that's it!

![MSI](Images\InstallationMSI.png)

Step 4: Let's make sure everything installed OK
-----------------------------------------------
We can do this validation using the Node.js Interactive Window.  The interactive window is part of NTVS and found on View->Other Windows->Node.js Interactive Window.

![Node.js Interactive Window](Images\InstallationREPLCommand.png)

Now that the interactive window is open, let's type some JavaScript. The interactive window supports everything you can do in code including require(). I'll keep it simple showing a variable and the location of the node.js interpreter.

![Node.js Interactive Window](Images\InstallationREPL.png)

Step 5: That's it!
------------------
You now have a Node.js IDE for Visual Studio. Please explore our documentation to find out more about our features such as debugging, profiling, intellisense, etc...
