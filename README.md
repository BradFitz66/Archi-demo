# ARCHI DEMO

Archi is a 3D tilemap tool. The purpose of this demo is to test the intuitiveness of the tool. Although possible, it is not recommended you use this for current projects as this tool is still very buggy and in active development and has not been tested for bugs inside actual builds. Inside this demo you'll find two examples of different tilesets, a very basic one created by me in Blender, and a more detailed and complex tileset made by Kenney over at https://opengameart.org/content/castle-kit


# Creating your own tileset

To create your own tileset, you should have a bunch of prefabs with the "Tile" component attached. With this tileset you should have a "Master" tile if you want to have autotiling. This should be a duplicate of one of your tiles (it doesn't matter which) and should contain all of the rules for autotiling. 

To start adding rules, select your master tile. First, set a default tile. This will be the tile that will be chose by default when a rule cannot be found. **THIS SHOULD NOT BE YOUR MASTER TILE** Click the plus button near Rotations List. This should add a new element to the list, in which you will find 4 things:

Tile - The tile prefab that will be created if the rules are met

Rotation Diff - The rotational difference of the tile (this is useful for corner pieces so you don't have to create 4 different prefabs for each rotation of the tile. For example, if you put 90 here, the tile will be rotated 90 degrees when placed)

Z Rotation - This is useful for when a model hasn't been imported properly and the Z axis is where the Y axis is, which would mess up rotations. 

Tile Mask - This is the actual rule that must be met. If you want to create a corner piece, you should select either these combinations:
Right + Top -  If there are tiles to the right and top of this one
Right + Bottom - If there are tiles to the right and bottom of this one
Left + Top - If there are tiles to the left and top of this one
Left + Bottom - If there are tiles to the left and bottom of this one

Also, for any tiles placed, you might be able to see a number on them. This refers to the current rule they're fulfilling (note this rule might not exist, in which case the tile will be the default tile). Here's a list of the rules associated to each number:

Top: 1
Right: 2
Bottom: 4
Left: 8

adding these together will give you combinations (eg: left + bottom = 10) and is useful for debugging.
Also, sometimes it might be confusing to determine what the "left" of a tile is. To get a correct view, use the view gimbal in the top right of the unity scene view and click the green Y arrow. This should give you a top down view of the tilemap. From here left, right, top, and down should be where you expect them to be.

To assign a tile to the Archi component, you must select the object with the Archi component added to it and drag the tile prefab from the project file viewer to the inspector. If you want to have autotiling, this tile MUST be the master tile. You only need to add the master tile to have autotiling.

If you're still confused, look at the example tilesets to see how they've been set up.