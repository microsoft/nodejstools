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
We'll create a new Node.js project and verify that the debugger works.

To create a project, select File->New->Project. If you are using Visual Studio Express for Web, it's File->New Project.

![New Project](Images\InstallationNewProjectMenu.png)

There are several project templates to choose from, including creating a project for your existing code.

For now, we'll use the simplest web server template, which is Blank Node.js Web Application. Enter a project name and location and click OK. Note that for most Node.js projects, you will want to use a location with a short path to avoid MAX_PATH issues.

![New Project](Images\InstallationNewProject.png)

The project will be created, and server.js will open in the editor. Click in the margin (or press F9) to insert a breakpoint in the createServer callback.

![Breakpoint](Images\InstallationBreakpoint.png)

From the Debug menu, select Start Debugging (or press F5).

If this is the first time you use Node.js, you may see a warning from the Windows Firewall. Click allow access if you want to allow connections to/from other machines.

![Windows Firewall](Images\InstallationWindowsFirewall.png)

A web browser will launch, and execution will break in the debugger. You can inspect variables in the Locals and Watch window, navigate the call stack, etc.

![Debugger](Images\InstallationDebugger.png)

Press F5 to continue, and you'll see the page rendered in the browser.

![Browser](Images\InstallationBrowser.png)

Now let's try the Node.js Interactive Window, which is found at View->Other Windows->Node.js Interactive Window.

![Node.js Interactive Window](Images\InstallationREPLCommand.png)

The interactive window supports everything you can do in code including require(). The code in the screenshot defines a variable and displays the location of the Node.js interpreter.

![Node.js Interactive Window](Images\InstallationREPL.png)

That's it!
----------
You now have a Node.js IDE for Visual Studio. Please explore our [wiki:"documentation" Documentation] to learn about using [wiki:"Projects" Projects], and find out more about our features such as [wiki:"debugging" Debugging], [wiki:"profiling" Profiling], [wiki:"IntelliSense" Editor], etc...
