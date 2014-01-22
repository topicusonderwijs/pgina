---
title: pGina Login Mask
layout: default
pageid: Documentation
---

# pGina Login Mask

## Fork diff

* during the system boot a message box (max. 1min) is shown to prevent a user from login

* if pGina is unusable for a user an exclamation mark logo will be shown

* the logo is primarily used to inform a user of the current pGina state

* a user receives a clear message if he tries to login without a running pGina service

* in case of an error the username and password field is cleared

* during a login another message box is shown to prevent any user interaction with the CP

* a login can't be canceled

## Boot
<img src="{{ site.url }}/images/documentation/login_start.png" style="float:right" alt="pGina boot">
Instead of showing a login mask as soon as possible,
this fork will wait for the pGina service to came up until presenting a login screen.
The reason is, without a running pGina service only local administrators are able to login!
With that in mind I decided to show a message box until the pgina service is up and running,
at least for one minute.
<br style="clear:both">

<img src="{{ site.url }}/images/documentation/login_start_failed.png" style="float:left" alt="pGina service failed">
If a minute has past and the pGina service is still down the login mask is shown, but with an exclamation mark logo.
There can be a lot of reasons, like a slow machine, AV scanner, ....
<br style="clear:both">

<img src="{{ site.url }}/images/documentation/login_start_ok.png" style="float:right" alt="pGina boot">
As soon as the pGina service is up the logo is shown, and thats the normal case.
<br style="clear:both">

## Login
The login process is the same as always, but this fork will show another message box
saying that the login is in process.
![user login]({{ site.url }}/images/documentation/login_running.png)

<img src="{{ site.url }}/images/documentation/login_start_failed.png" style="float:right" alt="pGina service failed">
If you try to login while the pGina service is down
you are limited to local administrators!
<br style="clear:both">
If a user tries to login from such a stage he will receive an error message like

![user login failed]({{ site.url }}/images/documentation/login_failed.png)

## Facts
While the system is booting

* a message box is shown until the pgina service is up and running

* if a minute has passed the service state disconnected status is shown

If the service state changes from

* connected to disconnected

  an exclamation mark logo will be shown

  the username and password field will be cleared

* disconnected to connected

  your logo will be shown

  the username and password field will be cleared

If the login failed

* the username and password field will be cleared
