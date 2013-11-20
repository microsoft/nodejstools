## npm


Npm is Node's package manager and allows you to install packages for use in your Node apps. You can install both packages from the public npm repository, as well as packages you've authored yourself.


## Managing installed packages with npm


### 1. The npm Package Management dialog


To use npm in your _Node Tools for Visual Studio_ project, right-click on the **npm** node in Solution Explorer, then click **Manage npm Modules**.


![](Images/npm-context-menu.png)


This will bring up the **npm Package Management** dialog. From here you can manage both local packages that will be available to your application, and global packages that are available to all Node applications. You can also easily search the public npm repository which, as of this writing, contains over 48,000 packages!


![](Images/npm-package-manager.png)


The **npm Package Management** dialog is divided into two main areas. On the left you can see packages that are installed, with separate tabs for local and global packages, whilst on the right you can see tabs that provide functionality for installing packages.


### 2. Local and global packages


If you created a blank Node project the **Local Packages** tab will be empty but, if you created your project using, for example, the express template you should see that some packages have automatically been included in your project. Local packages are available only to your application, and each application you create will have its own copies of whatever local packages you install. Thus it's perfectly fine to have different applications depend upon different versions of a given package.

As its name suggests, the **Global Packages** tab contains Node packages that have been installed globally and are therefore available to all applications. Depending upon whether or not you already have some Node applications installed that have installed global packages this list may or may not be empty. You should be careful about making changes to global packages because these changes can impact other applications and may stop them working properly, or at all. The **npm Package Management** dialog will warn you about this when the **Global Packages** tab is selected, as shown below.


![](Images/npm-global-warning.png)


You should also beware that some packages do not work well when installed globally, whereas for some a global install is preferred. If you are unsure refer to the package documentation to find out whether it should be installed globally or locally. We recommend that in general you install packages locally first and only install them globally where absolutely required.


### 3. Installing packages by name (and version)


If you already know the name of the package you want to install the easiest way is to do this by name.

1. In the **npm Package Management** dialog, click on either **Local Packages** or **Global Packages**, depending upon whether you want to install a local or global package.

2. Click on **Specify Package Details**. You'll see a pane, as follows, that allows you to enter the details of your package:


![](Images/npm-specify-package-details.png)


3. Enter the package name in the **Package Name** field.

4. Optionally, enter the version or tag that you'd like to install. If you leave this blank the latest version of the package will be installed. Note that you can enter a version range here, as described at [https://npmjs.org/doc/install.html], but that **npm install** will not write this range to your _package.json_ file - it will only write the version that it actually installs. If you wish to specify a version range in _package.json_ you should edit the file directly.

5. Click **Install Locally** or **Install Globally** to install the package.

6. A window will appear showing npm's progress and log, including any errors that occur during installation.


![](Images/npm-install-from-details.png)

![](Images/npm-install-from-details-log.png)


Note that whilst you can cancel the installation this is _not_ a transactional operation and will generally leave you with a partially installed package hierarchy. Fortunately this is easy to fix: please see **Troubleshooting** below.

7. Click **Close** on the progress window.


### 4. Using npm search to install packages


Searching npm's package repository can often be the best way to find and install packages, since many of them have similar names, especially those that are related.

1. In the **npm Package Management** dialog, click on either **Local Packages** or **Global Packages**, depending upon whether you want to install a local or global package.

2. Click on Search npm Repository. The search pane will appear. If this is the first time you've opened the **npm Package Management** dialog during your session you'll see that the repository catalog is being downloaded. In future the catalog will be cached between sessions to allow for quicker searching straight away. There will be an option to manually refresh the catalog from the npm repository whenever you need to.


![](Images/npm-catalog-retrieval.png)


3. Enter your search term in the **Find** field. Once the module catalog list has populated it will filter automatically as you type.


![](Images/npm-search-filtering.png)


4. Click on the module in the list that you would like to install to select it.

5. Click **Install Locally** or **Install Globally** to install the package.

6. A window will appear showing npm's progress and log, including any errors that occur during installation.


![](Images/npm-install-from-search.png)

![](Images/npm-install-from-search-log.png)


Note that whilst you can cancel the installation this is _not_ a transactional operation and will generally leave you with a partially installed package hierarchy. Fortunately this is easy to fix: please see **Troubleshooting** below.

7. Click **Close** on the progress window.


### 5. Working with dev and optional dependencies


Mostly the packages you install will appear in the **dependencies** collection in your project's package.json file. These are the dependencies that _must_ be installed for your app to work.

This will generally be what you want, but npm also supports two special types of dependency:

- **Dev dependencies**: These are packages that are only use for developing your app. A good example would be a package for unit testing.

- **Optional dependencies**: These are packages that allow your app to provide additional functionality but, without which, it will still work with gracefully degraded function. **Note that it is YOUR responsibility to check whether or not optional packages are installed in code and gracefully handle the situation where they are not.**

To install dev and optional dependencies use the **Install As** combo at the bottom of the **npm Package Management** dialog. (Note that this is only available when **Local Packages** is selected, since it makes no sense for global packages.)


![](Images/npm-install-as-combo.png)


This combo has three values:

- **Standard Dependency**: packages are required to run your app and will be added to the **dependencies** collection in _package.json_.
- **Dev Dependency**: packages are required only for development and will be added to the **devDependencies** collection in _package.json_. Packages should _not_ be required to run your app.
- **Optional Dependency**: packages are not required to run your app, but may enable richer functionality if installed, and will be added to the **optionalDependencies** collection in _package.json_. Packages should _not_ be required to run your app.

To install a package as a dev or optional dependency just select the appropriate value in the combo before you click **Install Locally**.

Dev and optional dependencies are listed with a different icon from normal dependencies in both the **npm Package Management** dialog and **Solution Explorer**.


![](Images/npm-dev-opt-management.png)


![](Images/npm-dev-opt-explorer.png)


### 6. Uninstalling packages


To uninstall a package from your project:


1. In the **npm Package Management** dialog, click on either **Local Packages** or **Global Packages**, depending upon whether you want to uninstall a local or global package.

2. In the list of installed packages, select the package you'd like to uninstall.


![](Images/npm-uninstall.png)


3. Click **Uninstall**.

4. A window will appear showing npm's progress and log, including any errors that occur during uninstall. Generally the log is much less extensive than for an install.


![](Images/npm-uninstall-local-log.png)


Note that whilst you can cancel the uninstall this is _not_ a transactional operation and will generally leave you with a partially uninstalled package hierarchy. Fortunately this is easy to fix: please see **Troubleshooting** below.

5. Click **Close** on the progress window.


### 7. The npm node in Solution Explorer


The **npm** node in solution explorer provides a high-level view of the packages installed in your Node app. You can also drill further into these packages to see which other packages they depend upon.


![](Images/npm-solution-explorer.png)


Various different icons are used to indicate the type and status of the packages you have installed.


- ![](Images/Dependency_16.png) - installed package that is listed in _package.json_.

- ![](Images/DependencyBundled_16.png) - installed package that is listed in _package.json_ and marked as a bundled dependency.

- ![](Images/DependencyDev_16.png) - installed package listed as a dev dependency in _package.json_.

- ![](Images/DependencyOptional_16.png) - installed package listed as an optional dependency in _package.json_.

- ![](Images/DependencyExtraneous_16.png) - installed package not listed as a dependency of any type in _package.json_.

- ![](Images/DependencyExtraneousBundled_16.png) - installed package not listed in **dependencies**, **devDependencies**, or **optionalDependencies** in _package.json_ but nevertheless marked as a bundled dependency.

- ![](Images/DependencyMissing_16.png) - dependency listed in _package.json_ but corresponding package not installed.

- ![](Images/DependencyBundledMissing_16.png) - bundled dependency listed in _package.json_ but corresponding package not installed.

- ![](Images/DependencyDevMissing_16.png) - dev dependency listed in _package.json_ but corresponding package not installed.

- ![](Images/DependencyOptionalMissing_16.png) - optional dependency listed in _package.json_ but corresponding package not installed.


Notes:

- Missing dev and optional dependencies of sub-packages are not shown. This is to avoid polluting Solution Explorer with large numbers of irrelevant nodes since many packages in the repository have both dev and optional dependencies that will not be downloaded by npm when they are installed.

- You can obviously install and uninstall packages both from within Visual Studio using the **npm Package Management** dialog, and from the command-line. The **npm** node will track your changes regardless of how you make them, but you may sometimes notice a short delay before the tree updates.


## 8. Viewing the npm log


TODO!!!


## Troubleshooting


### I see a message that says npm cannot be found


If you see this it means that either Node.js is installed in a non-default location on your system, and isn't on your PATH, or that it's not installed at all.

If you need to install Node.js you can download it from [http://nodejs.org/].

If you already have it installed you just need to tell NTVS where to find it. To do this:

1. Right-click on the project node for your Node.js project in **Solution Explorer**.

2. Click **Properties**.

3. Specify the location of Node.exe by browsing or typing the location into the **Node.exe path** field.

4. Right-click the **npm** node in Solution Explorer.

5. Click **Manage npm Modules** to reopen the **npm Package Management Dialog**.

You should now be able to install and uninstall packages. NTVS uses the location of _Node.exe_ to work out where to find _npm.cmd_. Node has included npm since version 0.6.3. If you are using an earlier version of node you should consider upgrading! If you have to install npm manually you should ensure that _npm.cmd_ is installed in the same directory as _node.exe_ otherwise NTVS won't be able to find it.


### How do I install missing modules?


In this release there is no **npm install** support for installing missing modules. **This will be available in the beta and, before that, will appear in dev builds within the next week or two.**

There are two options to workaround this:

1. Install missing modules from the commandline by executing **npm install** with no arguments in your Node project's directory.

2. Open up the **npm Package Management** dialog, select the **Local Packages** tab, go to **Specify Package Details** and, for each missing package, enter the package name in the **Package Name** field and click **Install Locally**.


### YAAARGH! I cancelled a package install/uninstall and now I have a partially installed/uninstalled package in my project. What do I do?


TODO - improve this

If this happens you can fix it by:

- Uninstalling the package
- Manually deleting the package directory from the filesystem
- Installing the package, if you unintentionally cancelled installation



## Coming Soon


- Install all packages in _package.json_ with **npm install**, and install missing packages in **npm Package Management** dialog without having to first uninstall them
- Caching of npm repository catalogue to speed searches on first run of the **npm Package Management** dialog within a session
- Update dependencies in _package.json_ to match modules installed on filesystem
- Detection of "out of date" packages and update to latest versions with **npm update**
- Uninstall packages directly from **Solution Explorer** context menu
- _package.json_ editor
- Install packages from tarballs, folders, and URLs
- Support for bundled dependencies - very useful for reliable deployment of Node apps
- Publishing
