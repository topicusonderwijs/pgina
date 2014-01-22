---
title: pGina pgSMB Plugin Documentation
layout: default
pageid: Documentation
---

# pGina pgSMB Plugin Documentation

* **Plugin Name:** pgSMB
* **Plugin Type:** Gateway, Notification
* **Version:** 3.2.0

## How it Works

The pgSMB plugin is a clone of the pGina 1.x pgFTP plugin.
It's purpose is to un/load compressed roaming profiles.
Instead of using the windows internal roaming profile function and upload a profile file by file,
this plugin will compress the whole profile before uploading it and decompress it before loading it.

*What is this god for?*

If your users dont login on only one station, rather than moving from pc to pc
and you want them to be able to use there profile on all station,
than you need a reliable way to upload there profiles after logging out.
The windows internal roaming profile upload is'nt reliable, because each file is uploaded one by one.
That means, if a file can't be uploaded (of whatever reason) windows will give up after the 3rd try and stops the whole upload process.
The result is a jumbled roaming profile.

*Whats going on?*

Because of the desing of pGina 3 im not able to load a profile directly (for the experts LoadUserProfile) like in pgina 1 or 2.
This plugin still uses the windows internal roaming profile but stores it locally.
This way an admin is able to manage the roaming profile trough a local GPO.

The plugin btw. will inform an admin in case of an error, like a failed profile upload.

### Gateway Stage

In the gateway stage the plugin will create the user on the local system
than extract the compressed profile from the SMB share,
adapt the ACL to fit the new user SID and let windows do the rest.
If the user doesn't pass the gateway stage he is still able to login but will receive a temporary profile.
No, not a windows temp profile. He is getting a normal user profile but without his data.
You can detect such a profile by calling

*net user %username% | find /I "pGina created pgSMB tmp" && @echo I'm a temp user*

Such a user recieves "pGina created pgSMB tmp" as description instead of "pGina created pgSMB"
and this tmp marked profile is excluded from the profile upload procedure.

Is there a problem during the gateway stage the plugin will retry as often as you specified in the configuration,
if the procedure failed an email is generated designated to the mail addresses you've entered.

**You still need the Local Machine plugin to do the group membership!**

**The plugin gateway order need to be pgSMB before Local Machine.**

**Its important that you've created the appropriate folder structure to store the profile.**

C:\Users\Public does fit well, but if you prefer a more secure structure

cacls output
```cacls
c:\roaming NT AUTHORITY\SYSTEM:(OI)(CI)F
           BUILTIN\Administrators:(OI)(CI)F
           NT AUTHORITY\INTERACTIVE:(special access:)
                                   READ_CONTROL
                                   SYNCHRONIZE
                                   FILE_APPEND_DATA
                                   FILE_READ_EA
                                   FILE_READ_ATTRIBUTES

           Creator Owner:(OI)(CI)(IO)C
           Creator Owner:(OI)(CI)(IO)(special access:)
                                          SYNCHRONIZE
                                          FILE_DELETE_CHILD
```

powershell output
```powershell
Path   : Microsoft.PowerShell.Core\FileSystem::C:\roaming
Owner  : BUILTIN\Administrators
Group  : NT AUTHORITY\SYSTEM
Access : Creator Owner Allow  DeleteSubdirectoriesAndFiles, Modify, Synchronize
         NT AUTHORITY\INTERACTIVE Allow  AppendData, ReadExtendedAttributes, ReadAttributes,
         ReadPermissions, Synchronize
         NT AUTHORITY\SYSTEM Allow  FullControl
         BUILTIN\Administrators Allow  FullControl
Audit  :
Sddl   : O:BAG:SYD:PAI(A;OICIIO;0x1301ff;;;CO)(A;;0x12008c;;;IU)(A;OICI;FA;;;SY)(A;OICI;FA;;;BA)
```

### Notification

The login script is triggered by the login event received from the pGina service,
also the max profile space value is applied in this stage.

The logoff procedure is triggered by a logoff event.
First a new thread is created than this thread will wait until the user has logged off.
If so the profile will be compressed and pushed into an SMB share.
Is there a problem compressing or pushing the plugin will retry as often as you specified in the configuration,
if the procedure failed an email is generated designated to the mail addresses you've entered.

## Configuration

![pgSMB configuration]({{ site.url }}/images/documentation/plugins/pgsmb.png)

### Roaming Profile
* *value in parenthesis* -- represent the regkey value

* *%?* -- is a macro variable for this value

* **The SMB share to connect** -- The path to the share which the plugin will try to connect to

  to be overridden by [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) pgSMB_SMBshare
* **Where to store the compressed Profile** -- This is the place where the compressed profile will be stored

  to be overridden by [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) usri4_profile
* **The name and extension of the Profile** -- How do yoy want to name the compressed profile

  to be overridden by [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) pgSMB_Filename
* **Where to extract the Profile** -- The remote compressed file will be extracted to this location
* **Try n times to connect/extract/compress** -- How often shall the plugin try to get a task done
* **The Program to un-compress the Profile** -- What program to use for (de)compression
* **The command to uncompress the Profile** -- The command line for the decompress call
* **The command to compress the Profile** -- The command line for the compress call

### User
* **The user HomeDir** -- The path to the home share

  to be overridden by the [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) usri4_home_dir
* **The user HomedirDrive** -- The drive name of this share

  to be overridden by [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) usri4_home_dir_drive
* **Script Path** -- The script to run in the user context during a login

  to be overridden by [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) LoginScript
* **The user max storage space in kbytes** -- GPO setting to limit the profile size

  to be overridden by [Attribute converter]({{ site.url }}/documentation/plugins/ldap.html#attribute_convertter) usri4_max_storage
  
### Administrative
* **space seperated ntp FQDN servers** -- a space seperated list of ntp servers
* **space seperated emails** -- a space seperated list of emails to receive error messages
* **space seperated smtp FQDN servers** -- a space seperated list of smtp servers

  the logged in username and password will be used
