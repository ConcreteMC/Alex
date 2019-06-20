# Alex
A Minecraft (Java & Bedrock Edition) client written in C# 

[![Build status](https://img.shields.io/appveyor/ci/kennyvv/Alex/master.svg?label=build%20%7C%20master&logo=appveyor&style=flat-square)](https://ci.appveyor.com/project/kennyvv/alex/branch/master/artifacts)

##### This client requires a paid Minecraft account.

You also need access to the resources from both Java & Bedrock edition of Minecraft.

As of the current state, there are still a few resources required from the Bedrock engine, work is being done on removing this dependency.

About
-----

This is a hobby project i work on from time to time, the end goal being it able to connect to a MC:Bedrock & MC:Java server & be feature compatible with most features required for the typical minigame server.

It can currently connect to any java 18w50a server, bedrock server support is coming as soon as the network implementation is done. I do not recommened trying the client out on a server you care about tho, seeing as any slightly decent anti-cheat will detect you as our movement physics do not currently match up with Vanilla. Some world interactions like building and destroying are implemented but only recommened to be used on private servers & in creative mode.

Model rendering isn't yet quite perfect, but most models can be rendered.

Cloning the Repository
----------------------

As we use submodules to add support for MC:Bedrock, you need to pull the submodules in order to compile Alex.

The easiest method of doing this is to clone the repository using ```git clone --recursive https://github.com/kennyvv/Alex.git```

Building on Windows
-------------------

To build on windows, you must install Gtk. See [GTK Project](https://www.gtk.org/download/windows.php) for info. Or else use these commands in CMD.
```
git clone https://github.com/Microsoft/vcpkg
cd vcpkg
.\bootstrap-vcpkg.bat
vcpkg install gtk:x64-windows
```

Contributing
------------

I'm looking for people that want to help me continue development on Alex.  C# experience required, i'd obviously help you setup the client to get started and be there to answer any questions.

* [Discord](https://discord.gg/txaahdU) - Join us on Discord!

Awesome repositories
---------------------

These repositories really helped me create Alex, thanks!

* [MiNET](https://github.com/NiclasOlofsson/MiNET) - A Minecraft Bedrock Edition server written in C#
* [TrueCraft](https://github.com/SirCmpwn/TrueCraft) - A Minecraft beta 1.7.3 client written in C#

Other resources
---------------

* [Wiki.vg](https://wiki.vg/Main_Page) - A website with documentation on various Minecraft things such as the protocol.
