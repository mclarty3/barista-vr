# BaristaVR
An in-the-works Barista VR arcade game, in which the player is challenged to complete multiple customer orders using realistic barista equipment including a bean grinder, espresso machine/steam wand, and more.

This game will in theory ultimately play somewhat like <a href="https://store.steampowered.com/app/530120/VR_The_Diner_Duo/">VR The Diner Duo</a> (probably minus the co-op). Customers will come up, place a procedurally generated order (medium vanilla latte, large iced coffee with cream, etc.), and the player will have to use the equipment at their disposal to quickly and correctly craft the beverages. 

# Where the game stands now
Currently, the game is little more than a tech demo. I've been working on and off for a long time on perfecting the basic mechanics (interacting with the machines, pouring liquids from one container to another, and getting scaling and interactivity feeling smooth and natural). So far, the following features are implemented with varying levels of quality:

- Decent placeholder models for all of:
  - Milk carton
  - Espresso machine
  - Bean grinder
  - Milk pitcher
  - Portafilter
  - Espresso pitcher
  - Syrup bottle
  - Paper cup
- Pour liquids: There are a variety of containers set up to store liquids, using the LiquidVolume asset.
    - These containers can also pour liquid if turned over, and receive poured liquids from other containers.
- Grinding beans: Place the portafilter into the bean grinder and press the button. 
    - The portafilter will fill with ground espresso
- Pouring espresso: Place a portafilter with (non-burnt) espresso grinds into the espresso machine and press the button. 
    - Espresso will pour out of the portafilter.
- Steaming milk: Place the steam wand inside a (filled) milk pitcher and twist the valve on the machine. 
    - The steam wand will turn on and begin to steam the milk, raising the temperature (as displayed on the screen).
- Pump syrup: Syrup bottles with pumps can be pressed to pump out syrup.
- An ingredient system, allowing the drinks to track what ingredients they contain (how much milk, espresso, syrup, etc.)
- A drink system, with thresholds for appropriate amounts and ratios of ingredients for different drinks
- A new and improved coffee shop environment! Now with a primitive outside visible through the storefront windows and open door

# Immediate roadmap

Coming up next, I'm working on polishing up the fundamentals and creating a basic gameplay loop for the first alpha release! It's actually coming quite a bit more quickly than I expected (certainly not due to many nights staying up far past my bedtime), so I'm expecting to release it probably the first week of October.

I'm realizing I also likely have quite a bit of figuring out to do to get this to work on Quest... The whole VR interaction system is based on the SteamVR Unity asset, and I fear the port will not be trivial. Fingers crossed!

# Further down the line
As far as goals for how the game will play once the foundation is in place:

- A thorough and realistic NPC behaviour system
    - NPCs will enter the shop, place an order, potentially have a seat in the coffee shop, and leave.
    - NPCs may get mad if their orders are wrong or take too long
- A variety of increasingly nice coffee shops, with different sizes, layouts, customers, and drinks
- An upgrade and decoration system for the coffee shop, allowing the player to customize their current shop to their liking with furniture, floor/wall decorations, and more
- An improvement to how liquid is rendered
    - Currently, it's just a lot of tiny sphere prefabs acting as 'drops' of liquid. I'd like to maybe use marching cubes for a more detailed liquid, though that's quite a bit beyond me as of now.
- Other goods for customers to order, like pastries, blended drinks, and teas
- **Make it an actual game!**
    - I'll of course be implementing menu systems, VR quality of life settings (scaling the play area, etc.), and other systems and features to turn it from a tech demo to a playable game.
        - This will probably end up being 80% of the work, right?

---

This game has been "in development" for quite a while now, meaning I had the idea back in 2018 and have been slowly building up and tearing down iterations, with several month breaks in between. Hopefully laying out and finally version controlling the project will be a good incentive to keep up progress.
