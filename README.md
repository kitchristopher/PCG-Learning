# PCG Learning
This project is a study on various Procedural Content Generators for games.

## Installation
This project contains the entire project for Unity, so to install just drag the folder into your liking. That said, really only the scripts are needed. The level and prefabs can be created easily.

## Learning Outcome
To further my knowledge on these techniques, develop better programming practices and object oriented principles, and find an ideal algorithim to further my focus in. Furthermore, these algorithims were designed to function decoupled from Unity as well, so for instance they could be used to generate classic ascii art in a console.

## Usage
If you are not using the existing project then you will need to set the scripts up again as follows: 
* Inside the unity editor, drag the Generate_Dungeon componant onto an object. 
* Next, assign prefabs of your liking for the three tile types.
* Lastly, set the sizes for the dungeon width, height, minimum room area, and minimum corridor area.
  * These values can be any positive number as long as the area of the dungeon is greater than the minimum room area and minimum corridor area.

# The Binary Space Partition Algorithim
This algorithim creates a dungeon by partitioning space out, then creating rooms in each of these spaces, then lastly connecting the rooms to one another with corridors. 

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
 
 # Agent-Based Algorithim
This algorithim randomly carves out corridors and rooms. It is quite random and more difficult to gurantee a good dungeon. However, this unpredicatiblity can in theory create some interesting dungeons.

## Findings
1. A grid based system to store the cells is quite convienient and constrains the size of the dungeon to the maximum size provided by the user. Morover, adding cells only when nessesary works well, as opposed to creating them all at once as empty cells.

2. Earlier in this algorithm, the rooms were built around a center point from the corridor
This had the awesome effect of creating sub-rooms within the room, but alas two problems presented themselves:
   * First, corridors were not designed to be rooms. It would be strange to see a hallway as a room. In order to maximize
this, a seprate room class would have been needed and would be a good future implementation.
   * Second, since the corridors could be in the middle of the room, sometimes the room would get cut in half, making
half of it inaccessable.

So, because of these faults (mainly #2), the algorithim was changed so the room would absorb the corridors as the room
was built overtop of it.

## Future Ideas
The Logic of the BSP algorithim seems far more controllable, but can limit the shape of the dungeons. Thus, to combine the best of both worlds, a hybrid method could be ideal. Following the logic of the BSP, after creating the rooms as rectangles, variation could be added by randomly adding onto the rooms from the outside, or carving them out from the inside to create sub-rooms.

# Cellular Automata
This algorithm creates a cave system based on cellular automata, that is the caves are iterativly generated based on the neighbors that each cell has. This creates very organic looking shapes, however it is more difficult to control.

## Findings
1. It would be good to create more flexibility for neighborhood creation, such as depth.
2. A more advanced transition rule instead of only a rock threshold would improve the look of the cave.
3. A better system of connecting rooms should be researched.
4. Converting the entire cave system into a mesh would likely be more efficent. Since The rock cells are instansiated, there are a vast number of meshes that are created but not really used, in contrast to the other algorithims that leave empty space in untraversable areas.
