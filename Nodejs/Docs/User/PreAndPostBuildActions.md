Pre and Post Build Actions
==========================

Getting Started
---------------
You may find it necessary to run a script or process immediately before or after build.  This is achieved
with minimal effort when using Node.js Tools for Visual Studio.  A small bit of xml code needs to be added to the Node.js 
project file (.njsproj) and your custom action will be called on build.  Place this code just before 
`<Import Project="$(VSToolsPath)\Node.js Tools\Microsoft.NodejsTools.targets" />` in the project file.

```
<PropertyGroup>
  <PreBuildEvent>
      PRE BUILD ACTION
  </PreBuildEvent>
</PropertyGroup>

<PropertyGroup>
  <PostBuildEvent>
      POST BUILD ACTION
  </PostBuildEvent>
</PropertyGroup>
```

These actions are run as if they were run manually through cmd.exe.  You could use this to do just about any custom 
action before or after build.

Example Using Grunt After Build
-------------------------------

A common scenario for many node developers is a desire to use grunt along with Node.js Tools for Visual Studio.  In this
example we will use grunt to mangle our js files to make them have less characters.

First, we should make sure we have grunt installed.  We should install grunt-cli, the command line interface, to the global node 
install.  We should also add the grunt package to this project and the grunt-contrib-uglify package we will use to mangle
our source code.

```
npm install -g grunt-cli
npm install grunt (or use the npm integration within Visual Studio)
npm install grunt-contrib-uglify (or use the npm integration within Visual Studio)
```

We also need to add a basic gruntfile.js file to the project.  Your actual file will likely be much more interesting.  
This file will produce a minimized/mangled version of the app.js file to a dest directory.
 
```javascript
module.exports = function (grunt) {
    /// Project configuration.
    grunt.initConfig({
        uglify: {
            options: {
                banner: '/* <%= grunt.template.today("yyyy-mm-dd") %> */',
            },
            my_target: {
                files: {
                    'dest/app.min.js': ['app.js']
                }
            }
        }
    });
    
    // Load the plugin that provides the "uglify" task.
    grunt.loadNpmTasks('grunt-contrib-uglify');
    
    // Default task(s).
    grunt.registerTask('default', ['uglify']);
};
```

Now that grunt is installed and we have a basic gruntfile, we can open up the .njsproj file for your project and add 
the code that will result in a PostBuildEvent.  This code needs to be inserted under the `<Project>` node and before
`<Import Project="$(VSToolsPath)\Node.js Tools\Microsoft.NodejsTools.targets" />`.

```
<PropertyGroup>
  <PostBuildEvent>
      grunt
  </PostBuildEvent>
</PropertyGroup>
```

Now all you need to do is request a build in Visual Studio and your action will be run.

NOTE: If you want to make sure grunt runs call rebuild.  If you call build and a build is not currently needed, before 
and after build events will not be run.  Requesting a rebuild in Visual Studio ensures that the build action is 
run as expected.