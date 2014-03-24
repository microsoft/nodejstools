Azure Deployment
================
You can deploy your Node.js application to Windows Azure directly from Visual Studio.  You can deploy to an Azure Web Site or Cloud Service (Web Role).

**Note**: Worker Role is currently not supported.


Publish to Azure Web Site using Web Deploy
------------------------------------------
This section describes how to use the **Publish** 
command to deploy your Node.js project to a Windows Azure Web Site.

To use this functionality, make sure to create a Windows Azure project. Windows Azure project templates have additional functionality for deploying to Windows Azure. There is a web.config file which configures Node.js for running under IIS Node.

Deployment will include all the files in your project, as well as the node_module folder, even if it's not part of the project.

Right-click on the project node in Solution Explorer, and select **Publish...**.

![Publish](Images/AzureWebSitePublishCommand.png)

This will bring up the publish profile which will allow you to specify the web site to which the project will be published.

![Selecting a publish target](Images/AzureWebSitePublishTarget.png)

The easiest way to do so is to select the web site from the picker by choosing the first option on the dialog above, and logging in using your Windows Azure credentials. This will open a new dialog that will provide a list of all web sites associated with your Windows Azure subscription, as well as the ability to create a new site.

![Selecting a web site](Images/AzureWebSitePublishSelectSite.png)

Alternatively, if you have a downloaded publish profile for your web site, you can use "Import" to use the corresponding publish settings without logging in.

If you plan on debugging your published project directly on Windows Azure servers by using node.js remote debugging, you need to publish the site in "Debug" configuration. This setting is separate from the current active solution configuration, and always defaults to "Release". To change it, open the "Settings" tab, and use the "Configuration" combo box:

![Changing the publish configuration](Images/AzureWebSitePublishConfig.png)

Note that publishing in "Debug" configuration will enable the debugging server on your web site, as well as a number of advanced logging options - you can see the detailed changes in Web.Debug.config file that is a part of your project. This configuration negatively affects the performance of your site and increases its attack surface, and so it should only be used for testing, and never for production websites.

Once you have the publish settings you're ready to deploy. You can click Preview to see the files that will be uploaded.

![Preview](Images/AzureWebSitePublishPreview.png)

Click **Publish** and the application will be deployed. Progress will be shown in the output window.

![Output Window](Images/AzureWebSiteOutputWindow.png)

When it's done, a new browser window will open to the site.

![Web Browser](Images/AzureWebSiteWebBrowser.png)

Publish to Azure Web Site using Git
-----------------------------------
Visual Studio 2013 has integrated Git support, so it's easy to deploy to Azure using Git, no command-line necessary.

**Note**: Git integration is also available for Visual Studio 2012 as an [extension](http://visualstudiogallery.msdn.microsoft.com/abafc7d6-dcaa-40f4-8a5e-d6724bdb980c) on the Visual Studio gallery.

From the [Windows Azure portal](http://manage.windowsazure.com), create a new Web Site.

![Create Web Site](Images/AzureGitWebSiteCreate.png)

Click on the newly created Web Site, and choose **Set up deployment from source control**.

![Set up deployment](Images/AzureGitWebSiteSetupDeployment.png)

Then select **Local Git Repository**.

![Select Local Git repository](Images/AzureGitWebSiteLocalRepo.png)

The **URL** to the repository can be copy/pasted from the portal.

![Repository ready](Images/AzureGitWebSiteRepoReady.png)

In Visual Studio, create a new Node.js project or load an existing project which isn't associated with source control.

**Note**: Git deployment to Azure does not require the use of the Windows Azure project types.  You may want to use them anyway, as you get a default web.config which you can configure. If you Git deploy a project without a web.config, Windows Azure will detect that it's a Node.js application and use an appropriate web.config for it.

Select **File**->**Add to Source Control**.

![Add to Source Control](Images/AzureGitAddToSC.png)

Then select Git as the source control system.

![Choose Source Control](Images/AzureGitChooseSC.png)

A Git repository will be created for your solution.  It will be listed in the **Team Explorer** window, under **Local Git Repositories**.

![Team Explorer Local Git Repositories](Images/AzureGitTeamExplorer.png)

From **Team Explorer**, click on the **Home** button on the toolbar. You may see a prompt to install Git command prompt tools. You may choose to install them, but they are not necessary.

![Team Explorer Home](Images/AzureGitTeamExplorerHome.png)

Click on **Changes** to see your pending changes. Review the files that will be included in the commit, enter a comment and click **Commit**.

**Note**: When using Git deployment, the node_modules are not checked in to the repository.  When Azure detects a new deployment, it will automatically npm install the modules specified in package.json.

![Team Explorer Pending Changes](Images/AzureGitPendingChanges.png)

Once it's done, you'll see a notification for your commit.

![Team Explorer Commit Notification](Images/AzureGitAfterCommit.png)

In the notification message, click **Sync** to bring up the **Unsynced Commits** page.

![Team Explorer Publish](Images/AzureGitPublish.png)

Enter the URL for the Git repository, as displayed in the Azure Portal.  Click **Publish**.  Enter your credentials when prompted.

The **Deployments** page for your Web Site will show the commit information.

![Deployment History](Images/AzureGitDeploymentHistory.png)

You can click the **Browse** button at the bottom of the **Deployments** page to navigate to your site.

![Web Browser](Images/AzureGitWebBrowser.png)


Publish to Cloud Service
---------------------------
This section describes how to use the **Publish** 
command to deploy your Node.js project to a Windows Azure Cloud Service (Web Role).

**Requirements**: Cloud Service support requires the following components which can be installed using Web Platform Installer by clicking on these links:

- [Windows Azure SDK for .NET for VS 2013](http://go.microsoft.com/fwlink/p/?linkid=323510&clcid=0x409) 
- [Windows Azure SDK for .NET for VS 2012](http://go.microsoft.com/fwlink/p/?linkid=323511&clcid=0x409) 
- [Windows Azure SDK for Node.js](http://go.microsoft.com/fwlink/?linkid=254279&clcid=0x409)

**Notes**:

- The **Windows Azure SDK for Node.js** is currently **not compatible** with the 64-bit version Node.js. 
- The **Windows Azure SDK for Node.js** is required for running in the Windows Azure emulator / IIS Node, but optional for Publish to Azure.
- The **Windows Azure SDK for .NET** provides the Visual Studio integration for Cloud Services.  The command to convert to a Cloud Service Project won't be available if you don't have the SDK installed.

To use this functionality, make sure to create a Windows Azure project. Windows Azure project templates have additional functionality for deploying to Windows Azure. There is a web.config file which configures Node.js for running under IIS Node as well as deployment scripts for starting a Node.js application on a Windows Azure web role.

You first need to add a cloud service project to your solution. Right-click on on the project in Solution Explorer and select **Convert to Windows Azure Cloud Service Project** menu item.

![Convert to Windows Azure Cloud Service Project](Images/AzureCloudServiceConvertCommand.png)

This will add a new project to your solution.

![Solution Explorer](Images/AzureCloudServiceSolutionExplorer.png)

**Important Known Issue Workaround**:  Now save, close and reopen the solution.  There is an known issue where a published Node.js application won't work properly unless you do this after the Cloud Service project is created.

Right-click on the new project's node in Solution Explorer, and choose **Publish...**.

![Publish](Images/AzureCloudServicePublishCommand.png)

**Note**: The Node.js project also has a Publish command, but that is for Web Deploy to an Azure Web Site.

First you'll need to select your Windows Azure subscription.  You can do so by downloading credentials, or sign-in to your account.

![Sign-in](Images/AzureCloudServicePublishSignIn.png)

Next you'll need to select a cloud service (virtual machine) to host your web role.  You can configure several different options including enabling remote desktop to connect to the virtual machine.

![Publish Settings](Images/AzureCloudServicePublishSettings.png)

If you don't have an existing Cloud Service, you can create one by selecting **Create New...** from the drop down.

![Create Cloud Service](Images/AzureCloudServiceCreate.png)

Finally, click **Publish**.  The files from your project will be collected and the publish will begin. You can monitor the progress in the Windows Azure Activity Log.

![Windows Azure Activity Log](Images/AzureCloudServiceActivityLog.png)

When it's done, click on the **Website URL** link in the Windows Azure Activity Log to open the site in your web browser.

![Web Browser](Images/AzureCloudServiceBrowser.png)

Known issues
------------

- If your site contains a deep nested hierarchy of node_modules folders publishing can fail if a path exceeds 260 characters.  This is a limitation of Windows file APIs.  If you encounter this you'll need to move your project to a directory with a shorter path.

- A Node.js application published to a **Windows Azure Cloud Service** may not work properly due to a timing issue when the Windows Azure Cloud Service project is created.  If this happens, you'll get a **500 Internal Server error** when browsing to the Cloud Service url.  To avoid this problem, **save, close and reopen the solution after creating the Cloud Service project** (Convert to Windows Azure Cloud Service Project command).
