# Lethal Company Employee Evaluation mod

A mod to track your own statistics, as well as your team's. Have you ever wanted to know how many Tattered Metal Sheets you collected so far or could you host a hell of a party with all the pickles you collected? Now you can!

## Features
- Tracking collected scrap (how much it's worth, how much of each item, etc.)
- Tracking scrap sales (because let's face it, all of you lost some valuable items ;))
- (TESTING) Tracking character deaths & their sources (let's see what is the most lethal cause! Pun intended)
- (IN PROGRESS) Tracking bought items
- Tracking creature kills (friendly fire too :))
- Tracking the moon expeditions (how many times you went to e.g. Titan and what the weather was like)
- Summary of items collected on a moon expedition (uses the same window as when you sell the scrap and it tells you how much you earned)
- Access to your current stats through the [terminal](access-terminal)
- (PLANNED) Tracking the same things the game currently does (steps, etc.) but cumulative across saves
- (PLANNED) Backing up the files after each game

** Please note all the stats are saved locally and also track your teammates.**

## <a name="access-terminal"></a>Accessing stats in-game
You can access your statistics by using the terminal.
When you type in the `help` command, you'll see that there is a new category called `STATS`.
`stats` - displays available options. After using this command, you will be able to use the other commands.
`scrap` - shows how much of each item was collected, how much it was worth in total, most valuable items, etc.
`deaths` - shows causes of death and how many times employees were killed by them.
`kills` - shows your eradication effort on the Company moons.
`sales` - displays how many items were sold to the Company and how much money it gave you.
`shopping` - shows information about store purchases.
`moons` - shows your expedition log

## Stats file location
Saved statistics can be found at: 
*%APPDATA%/LocalLow/ZeekerssRBLX/Lethal Company/player_stats_data.json*

##Dependencies
`TerminalAPI` - for new terminal commands

### Notes
This mod is still heavily in development, but should at least be usable now. If you find any bug, please do not hesitate to create an issue on Github.
Should be compatible with some of the mods that prevent losing loot on death when solo.

### Known issues
- There might be a problem with the mod sometimes not counting the beehive when players take it into the ship after departure, but it's purely visual and it is still saved, just doesn't display in the summary window
- There might be problems with tracking some of the deaths (known ones are listed below)
    - Masked
    - Drowning
    - Circuit Bees
    - Nutcracker
    - Lightning (but due to the nature of how it is coded it's probably not possible to fix it)
