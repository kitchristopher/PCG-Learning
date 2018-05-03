# PCG Learning
This project is a study on various Procedural Content Generators for games.

## Installation
This project contains the entire project for Unity, so to install just drag the folder into your liking. That said, really only the scripts are needed. The level and prefabs can be created easily.

## Learning Outcome
To further my knowledge on these techniques, develop better programming practices and object oriented principles, and find an ideal algorithim to further my focus in.

# The Binary Space Partition Algorithim
This algorithim creates a dungeon by partitioning space out, then creating rooms in each of these spaces, then lastly connecting the rooms to one another with corridors. Furthermore, this algorithim was designed to function decoupled from Unity as well, so for instance it could be used to generate classic ascii art in a console.

## Usage
If you are not using the existing project then you will need to set the scripts up again as follows: 
* Inside the unity editor, drag the Generate_Dungeon componant onto an object. 
* Next, assign prefabs of your liking for the three tile types.
* Lastly, set the sizes for the dungeon width, height, minimum room area, and minimum corridor area.
  * These values can be any positive number as long as the area of the dungeon is greater than the minimum room area and minimum corridor area.

## Final Thoughts 
* Overall, the dungeons created with this algorithim look interesting. 
* By creating corridors by connecting two random points in a room, the algorithim creates rooms with varying shapes, sometimes looping corridors, and corridors that can twist around. 
  * However, this does create an issue in figuring out which room the corridors are connected to one another as they can weave through each other.

## Improvememts
* The algorithim could be made more efficent by partitioning only empty array space and just storing the position that each partition was created in. Then when creating rooms and connecting corridors cells would be created from the null space rather than changing the cell's type.
  * This would improve the speed when partitioning since the cells wouldn't need to be copied over each time.
  * This would also decreases memory requirements since unused cells wouldn't be created.
  * However, this could create some issues later on if for whatever reason you needed to still have the unused cells in the game, perhaps as dirt or for something interactive.
 * The recursive functions could be made somewhat cleaner to better keep the *Single Responsibility Principle*.
