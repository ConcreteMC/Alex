# Alex
A Minecraft (Java & Bedrock Edition) client written in C#

##### This client does NOT allow you to play without an official minecraft account.  
You also need access to the resources from both Java & Bedrock edition of Minecraft.

About
-----

This is a hobby project i work on from time to time, the end goal being it able to connect to a MC:Bedrock & MC:Java server & be feature compatible with most features required for the typical minigame server.

It can currently connect to any java 18w50a server, bedrock server support is coming as soon as the network implementation is done. I do not recommened trying the client out on a server you care about tho, seeing as any slightly decent anti-cheat will detect you as our movement physics do not currently match up with Vanilla. Some world interactions like building and destroying are implemented but only recommened to be used on private servers & in creative mode.

Model rendering isn't yet quite perfect, but most models can be rendered.

Awesome repositories
---------------------

These repositories really helped me create Alex, thanks!

* [MiNET](https://github.com/NiclasOlofsson/MiNET) - A Minecraft Bedrock Edition server written in C#
* [TrueCraft](https://github.com/SirCmpwn/TrueCraft) - A Minecraft beta 1.7.3 client written in C#

Other resources
---------------

* [Wiki.vg](https://wiki.vg/Main_Page) - A website with documentation on various Minecraft things such as the protocol.
