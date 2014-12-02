Install Node.js and get started with NTVS
=========================================

Thank you for installing Node.js Tools for Visual Studio (NTVS). Please follow these instructions to complete your installation.

Install Node.js
---------------
NTVS requires a Node.js interpreter to be installed. You can use a global installation of Node.js or you can specify the path to a local interpreter in each of your Node.js projects.

Node.js is built for 32-bit and 64-bit architectures. NTVS supports both. Only one is required and the Node.js installer only supports one being installed at a time.  NTVS works with Node.js v0.10.20 or later.

* Node.js (x86): [http://nodejs.org/dist/v0.10.33/node-v0.10.33-x86.msi](http://nodejs.org/dist/v0.10.33/node-v0.10.33-x86.msi)
* Node.js (x64): [http://nodejs.org/dist/v0.10.33/x64/node-v0.10.33-x64.msi](http://nodejs.org/dist/v0.10.33/x64/node-v0.10.33-x64.msi)

Install Azure Tools
-------------------
NTVS gives you the ability to easily deploy your Node.js applications to Azure Websites or Cloud Services. For some of these deployment options, you'll need to install the Azure Tools for Visual Studio. The tools give additional functionality such as Server Explorer integration for your Azure resources, as well as emulator support for Cloud Services.

To install, via Web Platform Installer:

* Azure Tools for VS 2013: [http://go.microsoft.com/fwlink/p/?linkid=323510](http://go.microsoft.com/fwlink/p/?linkid=323510)
* Azure Tools for VS 2012: [http://go.microsoft.com/fwlink/p/?linkid=323511](http://go.microsoft.com/fwlink/p/?linkid=323511)

Additionally, several Azure services, such as storage, service bus or management services can be used from your Node.js application using Azure SDKs for Node.js. These are available as npm packages.

To learn more, see the documentation for [wiki:"Azure Deployment" AzureDeployment] and the [Azure Node.js Developer Center](http://azure.microsoft.com/en-us/develop/nodejs/).

Let's make sure everything installed OK
---------------------------------------
We can do this validation using the Node.js Interactive Window.  The interactive window is part of NTVS and found at View->Other Windows->Node.js Interactive Window.

![Node.js Interactive Window](Images\InstallationREPLCommand.png)

Now that the interactive window is open, let's type some JavaScript. The interactive window supports everything you can do in code including require(). I'll keep it simple showing a variable and the location of the Node.js interpreter.

![Node.js Interactive Window](Images\InstallationREPL.png)

That's it!
----------
You now have a Node.js IDE for Visual Studio. Please explore our [wiki:"documentation" Documentation] to learn about using [wiki:"Projects" Projects], and find out more about our features such as [wiki:"debugging" Debugging], [wiki:"profiling" Profiling], [wiki:"IntelliSense" Editor], etc...
