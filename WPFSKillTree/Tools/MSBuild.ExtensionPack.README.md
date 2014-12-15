![](https://github.com/mikefourie/MSBuildExtensionPack/raw/master/Images/Logo.png)

[![Build status](https://ci.appveyor.com/api/projects/status/jqebjbmgg9adecyx)](https://ci.appveyor.com/project/mikefourie/msbuildextensionpack)

The **MSBuild Extension Pack** provides a collection of over 480 MSBuild Tasks, MSBuild Loggers and MSBuild TaskFactories.

A high level summary of what the tasks currently cover includes the following:

* System Items: Active Directory, Certificates, COM+, Console, Date and Time, Drives, Environment Variables, Event Logs, Files and Folders, FATP, GAC, Network, Performance Counters, Registry, Services, Sound
* Code: Assemblies, CAB Files, Code Signing, DynamicExecute, File Detokenisation, GUID’s, Mathematics, Strings, Threads, Xml, Zip Files
* Applications: BizTalk 2006 / 2009, Email, IIS6, IIS7, MSBuild, SourceSafe, SQL Server 2005 /2008, StyleCop, Twitter, Team Foundation Server, Visual Basic 6, Windows Virtual PC, WMI

The *MSBuild Extension Pack* is provided in two versions. The 3.5.* version supports the .Net Framework 3.5 and the 4.0.* version supports the .Net Framework 4.0.

The *MSBuild Extension Pack* implements a *TaskAction* based design which aims to improve usability and maintenance whilst reducing the code base, e.g. to start or stop a website, typically two task files would be created to perform each task, whereas the *MSBuild Extension Pack* accomplishes this in a single task file using TaskAction=”Stop” and TaskAction=”Start”. 

Each task is documented and provided with an example in the help file, which is also available online at [www.msbuildextensionpack.com](http://www.msbuildextensionpack.com/). Where applicable, tasks are remote enabled, simply specify a MachineName with optional credentials and the task will target the remote machine.
 
Each task is also provided with an IntelliSense schema file to improve developer productivity in the Visual Studio IDE.

## Feedback
Direct feedback may be emailed to [feedback@msbuildextensionpack.com](mailto:feedback@msbuildextensionpack.com)

You may also wish to follow on [Twitter](http://www.twitter.com/msbep)

If you are a frequent user of MSBuild, take a look at [MSBuild Explorer](http://www.msbuildexplorer.com)
