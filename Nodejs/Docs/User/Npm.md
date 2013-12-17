## npm


**Npm** is **Node**'s package manager and allows you to install packages for use in your Node apps. You can install packages from the public **npm** repository, as well as packages you've authored yourself.

*Node Tools for Visual Studio* provides an easy to use interface to **npm** and, over the coming months, will significantly expand **npm** integration within *Visual Studio*.


## Managing installed packages with npm


### The npm Package Management dialog


To use **npm** in your *Node Tools for Visual Studio* project, right-click on the **npm** node in **Solution Explorer**, then click **Manage npm Modules**.


![](Images/npm-context-menu.png)


This will bring up the **npm Package Management** dialog. In here you can manage local packages, which are available only to your application, as well as global packages, which are available to all **Node** applications. Additionally, you can quickly and easily search and install packages from the public **npm** repository which, as of this writing, contains over 48,000 packages!


![](Images/npm-package-manager.png)


The **npm Package Management** dialog is divided into two main areas. On the left you can see packages that are installed, with separate tabs for local and global packages, whilst on the right you can see tabs that allow you to install packages.


### Local and global packages


If you created a blank **Node** project the **Local Packages** tab will be empty. If on the other hand you used, for example, the express template you should see that some packages have automatically been included in your project.

Local packages are available only to your application, and each application you create will have its own copies of whatever local packages you install. Thus it's perfectly fine to have different applications depend upon different versions of a given package.

As its name suggests, the **Global Packages** tab contains Node packages that have been installed globally and are therefore available to all applications. Depending upon whether or not you already have some **Node** applications installed on your machine that have installed global packages, this list may or may not be empty.

You should be careful about making changes to global packages because these changes can impact other applications and may stop them working properly, or indeed at all. The **npm Package Management** dialog will therefore warn you when you are operating on global packages, as shown below.


![](Images/npm-global-warning.png)


You should also beware that some packages do not work well when installed globally, whereas for some a global install is preferred. If you are unsure whether to install a package locally or globally, refer to its documentation.

We recommend that in general you install packages locally first and only install them globally where absolutely required.


### Installing packages by name (and version)


If you already know the name of the package you want to install the easiest way is to do this by name.

1. In the **npm Package Management** dialog, click on either **Local Packages** or **Global Packages**, depending upon whether you want to install a local or global package.

2. Click on **Specify Package Details**. You'll see a pane, as follows, that allows you to enter the details of your package:


	![](Images/npm-specify-package-details.png)


3. Enter the package name in the **Package Name** field.

4. Optionally, enter the version or tag that you'd like to install. If you leave this blank the latest version of the package will be installed. Note that you can enter a version range here, as described at [https://npmjs.org/doc/install.html], but that **npm install** will not write this range to your *package.json* file - it will only write the version that it actually installs. If you wish to specify a version range in *package.json* you should edit the file directly.

5. Click **Install Locally** or **Install Globally** to install the package.

6. A window will appear showing npm's progress and log, including any errors that occur during installation.


	![](Images/npm-install-from-details.png)

	![](Images/npm-install-from-details-log.png)


	Note that whilst you can cancel the installation this is *not* a transactional operation and will generally leave you with a partially installed package hierarchy. Fortunately this is easy to fix: please see **Troubleshooting** below.

7. Click **Close** on the progress window.


### Using npm search to install packages


Searching npm's package repository can often be the best way to find and install packages, since many of them have similar names, especially those that are related.

1. In the **npm Package Management** dialog, click on either **Local Packages** or **Global Packages**, depending upon whether you want to install a local or global package.

2. Click on Search npm Repository. The search pane will appear. If this is the first time you've opened the **npm Package Management** dialog during your session you'll see that the repository catalog is being downloaded. Eventually we'll cache the catalog between sessions to allow for quicker searching right away. You'll still be able to trigger a refresh from the **npm** repository whenever you need to.


	![](Images/npm-catalog-retrieval.png)


3. Enter your search term in the **Find** field. Once the module catalog list has populated it will filter automatically as you type.


	![](Images/npm-search-filtering.png)


4. Click on the module in the list that you would like to install to select it.

5. Click **Install Locally** or **Install Globally** to install the package.

6. A window will appear showing npm's progress and log, including any errors that occur during installation.


	![](Images/npm-install-from-search.png)

	![](Images/npm-install-from-search-log.png)


	Note that whilst you can cancel the installation this is *not* a transactional operation and will generally leave you with a partially installed package hierarchy. Fortunately this is easy to fix: please see **Troubleshooting** below.

7. Click **Close** on the progress window.


*(New in NTVS 1.0 Beta)*


Since the **npm** package catalogue can take a while to retrieve from the repository, we now cache the results and reuse them next time you open Visual Studio so you won't have to wait.

This is fine but, obviously, new packages are being published all the time: in the last month alone some 3000 more packages have been added to the repository. This means that your cache can become out of date quite quickly. This may not matter so much if you're using only popular, tried and true modules - such as express - but will be more problematic if you want to use modules that have only just been published.

You therefore need some way to make sure your cache is brought up to date on a regular basis. Rather than force you to update your cache on a schedule, we've added a **Refresh Catalog** button to the **Search npm Repository** tab that you can use to refresh the cache whenever you need.


### Working with dev, optional, and bundled dependencies


Mostly the packages you install will appear in the **dependencies** collection in your project's package.json file. These are the dependencies that *must* be installed for your app to work.

This will generally be what you want, but npm also supports two special types of dependency:

- **Dev dependencies**: These are packages that are only use for developing your app. A good example would be a package for unit testing.

- **Optional dependencies**: These are packages that allow your app to provide additional functionality but, without which, it will still work with gracefully degraded function. **Note that it is YOUR responsibility to check whether or not optional packages are installed in code and gracefully handle the situation where they are not.**

To install dev and optional dependencies use the **Install As** dropdown at the bottom of the **npm Package Management** dialog. (Note that this is only available when **Local Packages** is selected, since it makes no sense for global packages.)


![](Images/npm-install-as-combo.png)


This dropdown has three values:

- **Standard Dependency**: packages are required to run your app and will be added to the **dependencies** collection in *package.json*.
- **Dev Dependency**: packages are required only for development and will be added to the **devDependencies** collection in *package.json*. Packages should *not* be required to run your app.
- **Optional Dependency**: packages are not required to run your app, but may enable richer functionality if installed, and will be added to the **optionalDependencies** collection in *package.json*. Packages should *not* be required to run your app.

To install a package as a dev or optional dependency just select the appropriate value in the combo before you click **Install Locally**.

Dev and optional dependencies are listed with a different icon from normal dependencies in both the **npm Package Management** dialog and **Solution Explorer**.

Bundled dependencies are listed by name only in the **bundledDepencies** list in *package.json*. As their name suggests, they will be bundled with your app when you publish it as a package.

The main advantage of doing this is that it should lead to reliable deployments, since you will always know that deployed packages will be the same versions used for development by all members of your team. The downside is that they will significantly increase the size of your package.

In general, therefore, you should *not* bundle dependencies when publishing packages to the public npm repository.

*NTVS* currently recognises bundled dependencies, and will highlight them as such in **Solution Explorer** (see **The npm node in Solution Explorer** below), but does not yet provide any support for configuring them. Therefore, to configure bundled dependencies you should edit *package.json* directly.


![](Images/npm-dev-opt-management.png)


![](Images/npm-dev-opt-explorer.png)


### Uninstalling packages


To uninstall a package from your project:


1. In the **npm Package Management** dialog, click on either **Local Packages** or **Global Packages**, depending upon whether you want to uninstall a local or global package.

2. In the list of installed packages, select the package you'd like to uninstall.


	![](Images/npm-uninstall.png)


3. Click **Uninstall**.

4. A window will appear showing npm's progress and log, including any errors that occur during uninstall. Generally the log is much less extensive than for an install.


	![](Images/npm-uninstall-local-log.png)


	Note that whilst you can cancel the uninstall this is *not* a transactional operation and will generally leave you with a partially uninstalled package hierarchy. Fortunately this is easy to fix: please see **Troubleshooting** below.

5. Click **Close** on the progress window.


### npm in Solution Explorer

Visual Studio's Solution Explorer provides a visual representation of the packages you are using under the **npm** node in your project.

*(New in NTVS 1.0 Beta)*

It also allows you to execute some **npm** commands directly without needing to open the **npm Package Management** dialog.

#### The npm node

The **npm** node in solution explorer provides a high-level view of the packages installed in your Node app. You can also drill further into these packages to see which other packages they depend upon.


![](Images/npm-solution-explorer.png)


Various different icons are used to indicate the type and status of the packages you have installed.


- ![](Images/Dependency_16.png) - installed package that is listed in *package.json*.

- ![](Images/DependencyBundled_16.png) - installed package that is listed in *package.json* and marked as a bundled dependency.

- ![](Images/DependencyDev_16.png) - installed package listed as a dev dependency in *package.json*.

- ![](Images/DependencyOptional_16.png) - installed package listed as an optional dependency in *package.json*.

- ![](Images/DependencyExtraneous_16.png) - installed package not listed as a dependency of any type in *package.json*.

- ![](Images/DependencyExtraneousBundled_16.png) - installed package not listed in **dependencies**, **devDependencies**, or **optionalDependencies** in *package.json* but nevertheless marked as a bundled dependency.

- ![](Images/DependencyMissing_16.png) - dependency listed in *package.json* but corresponding package not installed.

- ![](Images/DependencyBundledMissing_16.png) - bundled dependency listed in *package.json* but corresponding package not installed.

- ![](Images/DependencyDevMissing_16.png) - dev dependency listed in *package.json* but corresponding package not installed.

- ![](Images/DependencyOptionalMissing_16.png) - optional dependency listed in *package.json* but corresponding package not installed.


Notes:

- Missing dev and optional dependencies of sub-packages are not shown. This is to avoid polluting Solution Explorer with large numbers of irrelevant nodes since many packages in the repository have both dev and optional dependencies that will not be downloaded by npm when they are installed.

- You can obviously install and uninstall packages both from within *Visual Studio* using the **npm Package Management** dialog, and from the command line. The **npm** node will track your changes regardless of how you make them, but you may sometimes notice a short delay before the tree updates.

- If you need to run npm from the command line, the easiest way to do this is to right-click on your **Node** project node in **Solution Explorer**, then click on **Open Command Prompt Here** in the context menu.


#### npm commands in Solution Explorer *(New in NTVS 1.0 Beta)*


**npm** commands can be found in two places in Solution Explorer:

- On the **npm** node context menu
- On the context menus for packages immediately below the **npm** node in the hierarchy.

**npm** commands are not available for sub-packages since these represent dependencies required by the packages you are using in your app. If you were to try to manage them manually with npm your app would almost certainly break, or start to behave strangely.

Commands on the **npm** node context menu are as follows:

- **Manage npm Modules** - brings up the **npm Package Management** dialog, described above.
- **Install Missing npm Modules** - executes **npm install** to install any modules listed in *package.json* that are not already installed; only enabled if missing modules are listed.
- **Update npm Modules** - updates all **npm** modules listed in *package.json* to their latest versions; only enabled when one or more modules is installed, or listed as missing.

Commands on the top level modules listed underneath the **npm** node:

- **Install Missing npm Module** - executes **npm install** to install the missing module you have selected; only enabled on missing modules.
- **Update Module** - updates the selected **npm** module to the latest version.
- **Uninstall npm Module** - uninstalls the selected **npm** module.

When you execute **npm** commands in Solution Explorer, the output from these commands is written to the Output window, and the Visual Studio status bar so that you can always see when **npm** is executing.


### Viewing the npm log


If you've already installed some packages you'll have seen that we capture and display the **npm** output for the current command. However, you can also view the **npm** output for your entire *Visual Studio* session. To see this:


1. Click **View > Output** on the *Visual Studio* main menu.

2. In the **Show output from** dropdown, select **Npm**.

3. You should see something similar to the following:


	![](Images/npm-log.png)


The exact content of the log will obviously depend upon what actions you have performed.

Note that we do not include the **npm** output from the repository catalogue retrieval simply because it's huge and not terribly interesting.

*(New in NTVS 1.0 Beta)*

To configure whether or not the Output window should open automatically when you execute an **npm** command:

1. Click **Tools > Options** on the *Visual Studio* main menu.

2. In the options tree on the left select **Node.js Tools > General**.

3. Check or uncheck **Show Output window when executing npm**, depending upon your preference. By default this setting is checked.

Output from **npm** will always be visible in the status bar so that you know when **npm** is running and can tell whether it completed successfully, or whether there were errors.

If any errors occur whilst executing **npm** you can always see them by clicking **View > Output**, as described above.


## Troubleshooting


### I see a message that says npm cannot be found


If you see this it means that either **Node.exe** is installed in a non-default location on your system, and isn't on your **PATH**, or that it's not installed at all.

If you need to install **Node** you can download it from [http://nodejs.org/](http://nodejs.org).

If you already have it installed you just need to tell *NTVS* where to find it. To do this:

1. Right-click on the project node for your **Node** project in **Solution Explorer**.

2. Click **Properties**.

3. Specify the location of **Node.exe** by browsing or typing the location into the **Node.exe path** field.

4. Right-click the **npm** node in **Solution Explorer**.

5. Click **Manage npm Modules** to reopen the **npm Package Management Dialog**.

You should now be able to install and uninstall packages. NTVS uses the location of **Node.exe** to work out where to find **npm.cmd**. Node has included **npm** since version 0.6.3. If you are using an earlier version of node you should consider upgrading! If you have to install **npm** manually you should ensure that **npm.cmd** is installed in the same directory as **node.exe** otherwise *NTVS* won't be able to find it.


### How do I install missing modules?


*(New in NTVS 1.0 Beta)*

To install all missing modules:


1. Right-click on the **npm** node in Solution Explorer.

2. Click on **Install Missing npm Modules** on the context menu.


All missing modules will be installed.

To install a single missing module:


1. Right-click on the module's node in Solution Explorer.

2. Click on **Install Missing npm Module** on the context menu.


The missing module will be installed.

In both cases **npm**'s output, including any errors, will be written to the **Output** window. The status bar will show whether or not **npm** ran successfully.


*(Alpha release only)*

In the alpha release there is no **npm install** support for installing missing modules. **This will be available in the beta and, before that, within dev builds.**

There are two options to workaround this:

1. Install missing modules from the command line by executing **npm install** with no arguments in your Node project's directory. To open a command prompt in the correct location, right-click on your **Node** project in **Solution Explorer**, and then click **Open Command Prompt Here** on the context menu.

2. Open up the **npm Package Management** dialog, select the **Local Packages** tab, go to **Specify Package Details** and, for each missing package, enter the package name in the **Package Name** field and click **Install Locally**.


### YAAARGH! I cancelled a package install/uninstall and now I have a partially installed/uninstalled package in my project. What do I do?


If **npm** fails to install a package it will often be able to rollback the operation so that your packages are in their previous state.

If, however, you cancel an **npm** operation you are simply killing the process, so no rollback occurs. This means that you will usually end up in a situation where a package is partially installed or uninstalled.

Fortunately this is not a huge problem. You can fix it by:

- Uninstalling the package, either in the **npm Package Management** dialog, or via the command line.

- Manually deleting the package directory from the filesystem - again, you can do this in *Visual Studio*'s **Solution Explorer**, from the command line, or in *Windows Explorer*.

- Installing the package, either in the **npm Package Management** dialog, or via the command line, which will restore the package and any damaged/partially removed sub-packages.

The easiest way to open a command prompt in the correct location is to right-click your **Node** project in **Solution Explorer**, then click **Open Command Prompt Here** on the context menu.

In a future release of *Node Tools for Visual Studio* we may implement an explicit rollback when you cancel an **npm** operation if enough people request it.

*(New in NTVS 1.0 Beta)*

If you choose to install or uninstall the package using **npm** you can now also do this in Solution Explorer via the right-click context menus on the **npm** node, or the packages that are partially installed/uninstalled.


## Coming Soon


- Install all packages in *package.json* with **npm install** *(New in NTVS 1.0 Beta)*, and install missing packages in **npm Package Management** dialog without having to first uninstall them.

- Caching of **npm** repository catalogue to speed searches on first run of the **npm Package Management** dialog within a session. *(New in NTVS 1.0 Beta)*

- Update dependencies in *package.json* to match modules installed on filesystem.

- Detection of "out of date" packages and update to latest versions with **npm update**.

- Uninstall packages directly from **Solution Explorer** context menu. *(New in NTVS 1.0 Beta)*

- *package.json* editor.

- Install packages from tarballs, folders, and URLs.

- Support for bundled dependencies - very useful for reliable deployment of **Node** apps.

- Publishing to the **npm** repository.
