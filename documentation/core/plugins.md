---
layout: default
title: pGina Plugin Development
pageid: Documentation
---

# Developing pGina Plugins

## Tutorial: Hello pGina

To learn how to create a pGina plugins, we'll start with a tutorial that 
demonstrates the implementation of a simple authentication plugin.
Along the way, you'll be introduced to the primary concepts and tools
behind pGina plugin development.

### Tools

Minimally, you'll need the following:

* Visual Studio 2010 Professional Trial [iso](http://download.microsoft.com/download/4/0/E/40EFE5F6-C7A5-48F7-8402-F3497FABF888/X16-42555VS2010ProTrial1.iso) or [webinstaller](http://download.microsoft.com/download/D/B/C/DBC11267-9597-46FF-8377-E194A73970D6/vs_proweb.exe)
* [.NET 4.0](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
* [Visual Studio 2010 Sp1](http://download.microsoft.com/download/E/B/A/EBA0A152-F426-47E6-9E3F-EFB686E3CA20/VS2010SP1dvd1.iso)

### Download the pGina source code

The first step is to download the pGina source code.

You can do so by downloading a zip archive from [source](download.html), or cloning the [repository](https://github.com/MutonUfoAI/pgina/).

### Setting up Visual Studio
**First you need to compile the plugin SDK**

* by running `pGina\src\pGina-3.x-vs2010.sln`,

* than select in the Menubar\Build\Configuration Manager... choose configuration:Release

* now select in the Menubar\Build\Configuration Manager... choose configuration:Release and platform:x86

* and compile

* do the same for platform x64


**Now you can create your plugin**

* Open Visual Studio and create a new project.
* In the new project dialog, select the "Class Library" template under "Visual C#" -> "Windows".

  Make sure to select ".NET Framework 4" in the drop-down list at the top.
* Select the `Plugins` directory of the pGina distribution as the location, and make sure to select "Create new solution."

  The name and solution name should be a short version of the name of your plugin without any spaces.

  For this example, we'll use the name `HelloPlugin`.

This will create a solution with a single project and a simple C# file.
Before we jump into the code, let's configure the build settings.
Select "Project" -> "HelloPlugin Properties...".
It makes things easiest if you set your build directory to a common location for all plugins.
Currently, all contributed plugins build to `Plugins\bin`.
To update your build settings so that the output directory is set to this directory, click on the "Build" tab,
select "All Configurations" from the "Configuration" list, and set "Output path" to `..\..\bin`.

We like to have all plugins use a consistent naming scheme for the output file names.
This is someting like the following: `pGina.Plugin.PluginName`.
Select the "Application" tab, and set the "assembly name" to `pGina.Plugin.HelloPlugin`.
We should also use an isolated namespace for our plugin, so under "default  namespace", use `pGina.Plugin.HelloPlugin`.

Save and do a quick build ("Build"->"Build Solution").
Verify that your plugin's `dll` appears in the `Plugins\bin` directory.
You should see `pGina.Plugin.HelloPlugin.dll`.
Next, we need to add references to the pGina SDK dll's and the log4net dll.
Select "Project" -> "Add reference...".
Select the "Browse" tab, and browse to `..\..\..\pGina\src\bin\`, and select `pGina.Shared.dll`, `Abstractions.dll`, and `log4net.dll` (`pGina.Core.dll` is not necessary for plugin development).

You're now ready to start developing your plugin!

### Implementing the Plugin

In this example, we'll create an authentication plugin.  This plugin will successfully
authenticate any user that has "hello" in the username, and "pGina" in the 
password.

To create an authentication plugin, we need to implement the interface 
`IPluginAuthentication`.  Let's create a class in the default namespace 
for this plugin:

```csharp
namespace pGina.Plugin.HelloPlugin
{
    public class PluginImpl : pGina.Shared.Interfaces.IPluginAuthentication
    {

    }
}
```

You'll probably want to change the name of the file to `PluginImpl.cs` to
match this class name.  

Next, we'll implement the required interface members, starting with `Name`.
This property should provide a human readable name for the plugin.

```csharp
public string Name
{
    get { return "Hello"; }
}
```

The `Description` property should provide a short (one sentence) description
of the plugin.

```csharp
public string Description
{
    get { return "Authenticates all users with 'hello' in the username and 'pGina' in the password"; }
}
```

The `Uuid` property must return a unique ID for this plugin.  You can generate
a new Guid using Visual Studio ( select "Tools" -> "Create GUID" ).

```csharp
private static readonly Guid m_uuid = new Guid("CED8D126-9121-4CD2-86DE-3D84E4A2625E"); 

public Guid Uuid
{
    get { return m_uuid; }
}
```

Note that the above is just an example.  You should generate a GUID and replace
the string above with that GUID.

The `Version` property should return the version number for your plugin.  The
best way to do this is to query for it using reflection.  For example:

```csharp
public string Version
{
    get
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
```

To change the version number, modify `Properties\AssemblyInfo.cs`.

Next, we implement the `Starting` and `Stopping` methods.  These are executed
at startup/shutdown of the pGina service.  They're intended for 
initialization/cleanup tasks for things that are service related.  Note that 
they are **not** intended
as intialization/cleanup for each logon.  They are not called during simulation
so you should not do anything in these methods that is needed for logon 
processing.  Most plugins don't need to do anything within these methods.
For our example plugin, we leave them empty.

```csharp
public void Starting() { }

public void Stopping() { }
```

Finally, we get to the meat of our plugin, the `AuthenticateUser` method.  This
is called by the pGina service at the appropriate time during the authentication
stage of a login.  The parameter, a `SessionProperties` object contains information
about the user including the username and password.  For our plugin, we need to
simply verify that the username contains the word `hello` and that the password
contains `pGina`.  If that is the case, we return a successful result, if not we
return failure.  We return the result in a `BooleanResult` object.

```csharp
public BooleanResult AuthenticateUser(SessionProperties properties)
{
    UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

    if (userInfo.Username.Contains("hello") && userInfo.Password.Contains("pGina"))
    {
        // Successful authentication
        return new BooleanResult() { Success = true };
    }
    // Authentication failure
    return new BooleanResult() { Success = false, Message = "Incorrect username or password." };
}
```

The `BooleanResult` object contains two properties: `Success` and `Message`.
You do not always need to set the `Message` property, but you always want to
set the `Success` property.   We recommend that you always set the `Message` 
property when the authentication fails.

That's it!  You've implemented a simple pGina plugin.

### Testing your plugin

Execute the pGina configuration utility, under the "Plugin Configuration" tab, 
make sure to add the plugin build directory in the pGina distribution (`Plugins\bin`),
and enable the plugin by checking the checkbox for the authentication stage.
Then, under the "Simulation" tab, test your plugin by trying out a few logins.

There's much more to learn about plugins, but this should give you a starting
point.  In the sections below, we'll dive into some more advanced plugin concepts.

## Adding Logging to Your Plugin

Your plugin should log information about its progress and activites.  Logging
support is provided via [Apache log4net][log4net]. Adding logging to a plugin
is simple.  The first step is to create a logger object.  You can do this in the
`Starting` method, or the constructor.  We recommend that you do not statically
initialize this object.  For example, to initialize your logger in the constructor,
use the following code:

```csharp
private ILog m_logger;

public PluginImpl()
{
    m_logger = LogManager.GetLogger("pGina.Plugin.HelloPlugin");
}
```

To log messages using the logger, you can use any of the standard [log4net][log4net]
logging functions.  For example:

```csharp
if (userInfo.Username.Contains("hello") && userInfo.Password.Contains("pGina"))
{
    // Successful authentication
    m_logger.InfoFormat("Successfully authenticated {0}", userInfo.Username);
    return new BooleanResult() { Success = true };
}
// Authentication failure
m_logger.ErrorFormat("Authentication failed for {0}", userInfo.Username);
return new BooleanResult() { Success = false, Message = "Incorrect username or password." };
```
        
For more about log4net, vist the [log4net web site][log4net].

## Storing Plugin Settings in the Registry

If your plugin requires configurable settings, they should be stored in the
registry, as a sub-key of the main pGina registry key.  Support for this is
provided via the `pGinaDynamicSettings` class.  This class utilizes the C#
`DynamicObject` class and the `dynamic` type.  Settings can be queried and
set as if they are properties of the object.  

To use `pGinaDynamicSettings` we recommend that you instantiate the object and
immediately set the defaults for all of your settings.  It makes sense to do
this in a static initializer.  For example:

```csharp
private static dynamic m_settings;
internal static dynamic Settings { get { return m_settings; } }

static PluginImpl()
{
    m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(m_uuid);

    m_settings.SetDefault("Foo", "Bar");
    m_settings.SetDefault("DoSomething", true);
    m_settings.SetDefault("ListOfStuff", new string[] { "a", "b", "c" });
    m_settings.SetDefault("Size", 1);
}
```

The `SetDefault` method will initalize a setting in the registry if it does not
already exist.  If the registry setting exists, the method has no effect.  Be
sure to instantiate the `pGinaDynamicSettings` object using the `Guid` of your
plugin.  This will ensure that your settings are stored in the approprate 
registry key (usually: `HKLM\SOFTWARE\pGina3\Plugins\{guid}` where `{guid}` is 
replaced with the Guid of your plugin).

The supported data types for settings are `string`, `bool`, `string[]`, and
`int`.  It is highly recommended that you set defaults as soon as your object
is created.  This will avoid runtime exceptions when trying to access a 
non-existent registry value.

To set/read the settings, you simply treat them as properties of the object.
For example:

```csharp
bool okToGoAhead = Settings.DoSomething;
if (okToGoAhead)
{
    Settings.Foo = "Baz";
}
```

Note that when reading a property it must be able to determine the data type
at run-time.  If you try to read the setting in a context that is ambiguous,
you may recieve a run-time exception.  Your best bet is to assign the setting
to a local variable with the appropriate data type (as shown above).

## Creating a Plugin Configuration Dialog

To provide a dialog to the user for configuration of your plugin via the 
pGina configuration UI, you implement the `IPluginConfiguration` interface.

```csharp
public class PluginImpl : pGina.Shared.Interfaces.IPluginAuthentication,
    pGina.Shared.Interfaces.IPluginConfiguration
```

This requires you to implement the method `Configure`.  This method should
initalize and display your dialog, and will be called by the pGina configuration
UI when the user requests to configure your plugin.  

Create a Windows form (use "Project" -> "Add windows form..."), and set up
your dialog.  Then make sure to invoke your windows form within the `Configure`
method.  For example, if my Windows form was called `Configuration`, I'd use
the following code:

```csharp
public void Configure()
{
    Configuration myDialog = new Configuration();
    myDialog.ShowDialog();
}
```

## Authorization and Gateway Plugins

To have your plugin support the authorization stage, implement the
`IPluginAuthorization` interface.  This requires the implementation of the following
method:

```csharp
BooleanResult AuthorizeUser(SessionProperties properties) { ... }
```

This method should return a `BooleanResult` with `Success` set to `true` if
the user is authorized by this plugin, otherwise `Success` should be set to
`false` and an appropriate message provided in the `Message` property.

To support the gateway stage, implement the `IPluginAuthenticationGateway`
interface.  This requires you to implement the following method:

```csharp
BooleanResult AuthenticatedUserGateway(SessionProperties properties) { ... }
```

The gateway stage is intended for any last minute post-authorization actions that
may be necessary.  For example, a user's group membership might be modified
(e.g. LDAP plugin), or
the user's username might be modified (e.g. the single user plugin).  Generally,
this stage should not fail, except under exceptional circumstances.  You
should almost always return a `BooleanResult` with `Success` set to `true` unless
for some reason the login should be denied.  Usually in the gateway stage, the
login should not be denied.  The only situation that might warrant a failure for
this stage is if an error of some kind occurs, however, even in that
situation, it often makes sense to log the error and return a successful result.

## `Session Properties`

The `SessionProperties` object is provided as a parameter to each of the three
methods corresponding to the three stages (authentication, authorization, and
gateway).  The most obvious use of this is to query the user information (by
retrieving the `UserInformation` object), such
as username, password, and group membership.  However, it is actually a general
purpose storage object, and can be used to store
any information that a plugin may need to persist across stages.  In fact, it
is recommended that if you have any persistent state that needs to be passed
between stages, you should use this object rather than using instance fields.

You can store objects in the `SessionProperties` object using the provided 
methods listed below.

```csharp
public void AddTracked<T>(string name, T val) { ... }
public void AddTrackedSingle<T>(T val) { ... }

public T GetTracked<T>(string name) { ... }
public T GetTrackedSingle<T>() { ... }
```

You can store an object associated with a key (a `string`) using `AddTracked`, or
you can store a single instance of a class using `AddTrackedSingle`.  Of course,
your plugin should not add a tracked single of the class `UserInformation`, because
that would clobber the `UserInformation` object that is provided by pGina core.

A unique `SessionProperties` object will be provided for each login.  If your plugin
is only involved in a single stage, then there is no need to store anything in
this object.  However, if your plugin is involved in multiple stages, then it
makes sense to store any persistent state related to a given login within this
object.  

Note that if you need to make a connection to a remote data source and you'd like
that connection to persiste between stages, you should make use of the `SessionProperties`
object along with the `IStatefulPlugin` interface (see below).

### Getting Information about Plugin Activity

It is often the case that a plugin needs to know what other plugins have executed
previously in the login chain, and the result of those plugins.  This information
is stored in the `SessionProperties` object and is in a tracked single of type
`PluginActivityInformation`.   You can query for the result of a given plugin
via the methods `GetAuthenticationResult`, `GetAuthorizationResult`, or 
`GetGatewayResult`.   However, use caution because if a plugin has not executed 
yet, these will throw an exeception.  To be safe, you should first use 
`GetAuthenticationPlugins`, `GetAuthorizationPlugins`, or `GetGatewayPlugins`
to get a list of plugins that have executed and iterate through the list.

For example, to count the number of successful authentications in the 
authentication stage so far, you could use the following code:

```csharp
PluginActivityInformation pluginInfo = sessionProps.GetTrackedSingle<PluginActivityInformation>();

int nSuccess = 0;
foreach( Guid pluginId in pluginInfo.GetAuthenticationPlugins() )
{
    BooleanResult result = pluginInfo.GetAuthenticationResult( pluginId );
    if( result.Success )
        nSuccess++;
}

// nSuccess has the number of plugins that have registered success in the authentication
// stage so far.
```


## The `IStatefulPlugin` Interface

If your plugin has state that needs to persist between stages of a
login, and/or makes connections to resources that need to be relased at the
end of a login chain (such as making a connection to a remote data source), you 
should implement the `IStatefulPlugin` interface.  This interface requires
two methods:

```csharp
void BeginChain(SessionProperties props) { ... }
void EndChain(SessionProperties props) { ... }
```

`BeginChain` is called prior to the authentication stage and should be used 
for initialization and set-up.  You should store any state in the provided
`SessionProperties` object (see above).  For example, one might initialize a connection
to a remote data source here and store a reference to the connection within
the `SessionProperties` object.

`EndChain` is called at the end of a login chain regardless of the success or
failure of the login.  This should be used to clean up any resources that 
are held by the plugin.  For example, one might terminate the connection with
a remote data source here.

## Notification Plugins

To implement a notification plugin, you should implement the `IPluginEventNotifications`
interface.  This interface requires the following method:

```csharp
void SessionChange(SessionChangeDescription changeDescription, SessionProperties properties) { ... }
```

This method is called when any of the standard Windows terminal events occurs.  The
first parameter provides a description of the event.  This class is 
[documented in the MSDN documentation][sessionChangeMsdn], and provides two
main properties: `Reason` and `SessionId`.  Of primary importance is the `Reason`
property.  The [MSDN documentation][changeReasonMsdn] describes the possible
values for this property.  Based on the value of the `Reason` property, you can
take the action that is appropriate for your plugin.

[pgina-github]: https://github.com/pgina/pgina "pGina repo on GitHub"
[example-code]: ExamplePluginImpl.cs "Example plugin source code."
[log4net]: http://logging.apache.org/log4net/ "Log4Net web site"
[sessionChangeMsdn]: http://msdn.microsoft.com/en-us/library/system.serviceprocess.sessionchangedescription.aspx "MSDN OnSessionChange"
[changeReasonMsdn]: http://msdn.microsoft.com/en-us/library/system.serviceprocess.sessionchangedescription.reason.aspx "MSDN SessionChangeReason"

### sub Notification `IPluginLogoffRequestAddTime` interface
<span class="greenbg">
Since fork 3.1.6.2<br>
This is a sub-interface of the `IPluginEventNotifications` interface.
<br>
The purpose of this interface is, to let the plugin know if the system is trying to shut down.
<br>
A plugin using this interface is able to delay a shutdown.
</span>

 This interface requires the following:

a directory containing all useres with active running tasks
```csharp
private Dictionary<string, Boolean> RunningTasks = new Dictionary<string, Boolean>();
```
<br><br>
a locker for the directory
```csharp
private ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
```
<br><br>
a bool to be set from the pgina service. True means the system is shutting down
```csharp
public static Boolean IsShuttingDown = false;
```
<br><br>
`LogoffRequestAddTime` is called in a loop from the pgina service while the system is shutting down

be sure to clean the Dictionary from any logged of user
```csharp
public Boolean LogoffRequestAddTime()
{
    IsShuttingDown = true;
    try
    {
        Locker.TryEnterReadLock(-1);
        if (RunningTasks.Values.Contains(true))
            return true;
    }
    catch (Exception ex)
    {
        m_logger.InfoFormat("LogoffRequestAddTime() error {0}", ex.Message);
    }
    finally
    {
        Locker.ExitReadLock();
    }

    return false;
}
```
<br><br>
`LoginUserRequest` is called from the pgina service during a login attempt

its possible that a user that has logged of retries to relogin while a cleanup task is running

(yes the logoff can and should be non blocking)

To prevent a messed up profile we check the Dictionary
```csharp
public Boolean LoginUserRequest(string username)
{
    try
    {
        Locker.TryEnterReadLock(-1);
        if (RunningTasks.Keys.Contains(username.ToLower()))
        {
            m_logger.InfoFormat("LoginUserRequest() logoff in process for {0}", username);
            return true;
        }
        else
        {
            m_logger.InfoFormat("LoginUserRequest() {0} free to login", username);
            return false;
        }
    }
    catch (Exception ex)
    {
        m_logger.InfoFormat("LoginUserRequest() {0} error {1}", username, ex.Message);
    }
    finally
    {
        Locker.ExitReadLock();
    }

    return false;
}
```
<br><br>
this requires IPluginEventNotifications interface to work
```csharp
public void SessionChange(SessionChangeDescription changeDescription, SessionProperties properties)
{
    UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

    if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
    {
        try
        {
            Locker.TryEnterWriteLock(-1);
            RunningTasks.Add(userInfo.Username, true);
        }
        finally
        {
            Locker.ExitWriteLock();
        }

        // dont block others, run your stuff in its own thread
        Thread rem_smb = new Thread(() => cleanup(userInfo, changeDescription.SessionId));
        rem_smb.Start();
    }
}

private void cleanup(UserInformation userInfo, int sessionID)
{
    // do your stuff
    try
    {
        Locker.TryEnterWriteLock(-1);
        RunningTasks.Remove(userInfo.Username);
    }
    finally
    {
        Locker.ExitWriteLock();
    }
}
```

## Change Password Plugins
To implement a change password plugin, you should implement the IPluginChangePassword interface. This interface requires the following method:

```csharp
public BooleanResult ChangePassword(SessionProperties properties,
                                    ChangePasswordPluginActivityInfo pluginInfo)
{
    UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
    if (userInfo.HasSID)
    {
        string username = userInfo.Username;
        string password_old = userInfo.oldPassword;
        string password = userInfo.Password;

        if (password.Length >= 5 && password.Length <= 8)
            if (Regex.IsMatch(password, @"\d"))
                if (Regex.IsMatch(password, @"(?i)[a-z]"))
                    if (Regex.IsMatch(password, "(?i)[^a-z0-9]"))
                        pass_ok = "special character ar'nt allowed";
                    else
                        pass_ok = null;
                else
                    pass_ok = "The password does'nt contain letters";
            else
                pass_ok = "The password does'nt contain numbers";
        else
            pass_ok = "The password length must be between 5 and 8";

        if (!String.IsNullOrEmpty(pass_ok))
            return new BooleanResult()
            {
              Success = false,
              Message = String.Format("{0}\n\nYour password does'nt fit the password policy\n\n{1}",
              pass_ok, "the policy ...")
            };
    }

    return new BooleanResult() { Success = true };
}
```
This method is called when a user changed his password using CTRL+ALT+DEL

<span class="greenbg">
Since fork 3.2.0.0<br>
Instead of using the ChangePasswordInfo class from pGina this fork uses the SessionProperties class.
You are than able to get all UserInformation instead of a few
<br>
Related to the order in which plugins are called is also different to the one from pGina.
This fork will call the IPluginChangePassword interface from a plugin and stops calling any other plugin if a false BooleanResult is returned.
The error message of this plugin is than presented to the user.
</span>