# Alex
A Minecraft (Java & Bedrock Edition) client written in C# 

![Build status](https://github.com/kennyvv/Alex/workflows/.NET%20Core/badge.svg)

##### This client requires a paid Minecraft account.

You also need access to the resources from both Java & Bedrock edition of Minecraft.

As of the current state, there are still a few resources required from the Bedrock engine, work is being done on removing this dependency.

About
-----

This is a hobby project i work on from time to time, the end goal being it able to connect to a MC:Bedrock & MC:Java server & be feature compatible with most features required for the typical minigame server.

As of now, it is able to connect to java 1.15.2 & bedrock 1.14.1 servers.

Screenshots
-----------

![Bedrock Lifeboat](https://raw.githubusercontent.com/kennyvv/Alex/master/screenshots/lbsg3.png)


Cloning the Repository
----------------------

As we use submodules to add support for MC:Bedrock, you need to pull the submodules in order to compile Alex.

The easiest method of doing this is to clone the repository using ```git clone --recursive https://github.com/kennyvv/Alex.git```

Requirements
-------------------

Alex requires SDL2 to create it's Window, this is a free library. See [SDL2](https://wiki.libsdl.org/Installation) for instructions on how to install it on windows.

On Ubuntu you may run ```apt install libsdl2-dev``` for easy installation.

On MacOS you can run the following command if you have [Brew](https://brew.sh) installed: ```brew install sdl2```

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
