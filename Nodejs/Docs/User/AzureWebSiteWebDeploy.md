Publish to Microsoft Azure Web Site using Web Deploy
====================================================

This section describes how to use the **Publish** command to deploy your Node.js project to an Azure Web Site.

To use this functionality, make sure to create an Azure project. Azure project templates have additional functionality for deploying to Azure. There is a web.config file which configures Node.js for running under IIS Node.

Deployment will include all the files in your project.  Files in the node_modules folder are included automatically, even if they are not part of the project.

Right-click on the project node in Solution Explorer, and select **Publish...**.

![Publish](Images/AzureWebSitePublishCommand.png)

This will bring up the publish profile which will allow you to specify the web site to which the project will be published.

![Selecting a publish target](Images/AzureWebSitePublishTarget.png)

The easiest way to do so is to select the web site from the picker by choosing the first option on the dialog above, and logging in using your Azure credentials. This will open a new dialog that will provide a list of all web sites associated with your Azure subscription, as well as the ability to create a new site.

![Selecting a web site](Images/AzureWebSitePublishSelectSite.png)

Alternatively, if you have a downloaded publish profile for your web site, you can use "Import" to use the corresponding publish settings without logging in.

If you plan on debugging your published project directly on Azure servers by using Node.js remote debugging, you need to publish the site in "Debug" configuration. This setting is separate from the current active solution configuration, and always defaults to "Release". To change it, open the "Settings" tab, and use the "Configuration" combo box:

![Changing the publish configuration](Images/AzureWebSitePublishConfig.png)

Note that publishing in "Debug" configuration will enable the debugging server on your web site, as well as a number of advanced logging options - you can see the detailed changes in Web.Debug.config file that is a part of your project. This configuration negatively affects the performance of your site and increases its attack surface, and so it should only be used for testing, and never for production websites.

Once you have the publish settings you're ready to deploy. You can click Preview to see the files that will be uploaded.

![Preview](Images/AzureWebSitePublishPreview.png)

Click **Publish** and the application will be deployed. Progress will be shown in the output window.

![Output Window](Images/AzureWebSiteOutputWindow.png)

When it's done, a new browser window will open to the site.

![Web Browser](Images/AzureWebSiteWebBrowser.png)

Known issues
------------

- If your site contains a deep nested hierarchy of node_modules folders publishing can fail if a path exceeds 260 characters.  This is a limitation of Windows file APIs.  If you encounter this you'll need to move your project to a directory with a shorter path.
