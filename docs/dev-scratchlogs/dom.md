# Dominic's running Scratch Log for Project Seed

Role(s): Programming, Design, Writing

--------------------

# Tech

## Multiplayer

Considerations: 

1. Cost
2. Documentation
3. API's Ease of use
4. Dev Support

A good overview of current network solutions: https://forum.unity.com/threads/what-are-the-pros-and-cons-of-available-network-solutions-assets.609088/

Choices that I have looked into so far:

- UNET
	- Deprecated by Unity, buggy sometimes. (Dev Support gone)
	- Documentation is OK.
	- Ease of use is OK.
	- Cost is virtually 0 if we use Steam backend & matchmaking services (or Epic's when its counterparts come up https://dev.epicgames.com/en-US/services)
- Photon PUN2
	- Very good documentation.
	- Very good dev support.
	- Tried and True solution.
	- Up to 100 CCU for $95 for 60 months, or 20 CCU for free. 
	- Uses client as a host BUT requires the relay servers to be used
		- Cannot leverage Steam's/Epic's NAT punchthrough to the fullest.
- Mirror (UNET Fork with fixes and improvements)
	- Absolutely Free for the networking stuff.
		- Some separate optional assets which are also very cheap ($1 patreon)
	- Basically UNET, which as far as I can tell is easy to use and understand.
	- Can utilize Steam's backend.
	- Makes UNET completely obsolete  
- New Unity Networking (w/ ECS, Job System, all that jazz)
	- Too new, documentation lacking, also I'm unfamiliar with ECS.
	- Might consider for future projects once the framework's more mature.
- MLAPI
	- NO example projects
	- Absolutely free
	- A spinoff of UNET, so slight differences.
	- Documentation is very sparse.
	- Only one developer working on it (albeit a very talented one!)

I'm settling on __Mirror__ for now

Also looking through Steamworks API and examples to ensure that what I want to do is viable: https://partner.steamgames.com/doc/sdk/api/example

What I've gathered from the C++ example is that, the IP address is more or less not used directly through the APIs (handled by lower level code), and things are mostly done through the servers' steam ID. The only exception to this is the server itself having the ip address of the server for us to use (so i think we can set the unet/mirror server ip maybe?).

The above is confirmed to be doable with Mirror (and also UNET): https://github.com/Raystorms/FizzySteamyMirror  
We just need the `steam64id` in order to connect to the host.  

An alternative way is to not focus on multiplayer code for now, and instead just work on the design, and write code that helps us to run simulations, do rapid design iterations and all that.  
I think this sounds like an OK plan, though I haven't tried it before, like an actual pre-production phase to really nail down the direction before working on the product proper.  
By the time we're done with design/art direction, the multiplayer frameworks might have more development (such as the new Unity Networking framework) and may push me to using it.
