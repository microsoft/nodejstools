Publish to Azure Web Site using Git
===================================

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
