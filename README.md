# Deer God Game
This is our project for The University of Texas at Dallas CS 4361 Computer Graphics with Dr. Kumar Fall 2022.

## Contributors
- Jordan Frimpter
- Henry Kim
- Brandon Runyon

## How to Run
Clone the repository and make sure to have Git LFS installed.
Open the folder GithubGraphics2 in Unity 2020.3.26f1 (although it should be compatible with other versions of Unity 2020). To run the game in the editor, open the MainScene in Assets/_Project/Scenes. To compile into an executable, open the Build Settings menu, choose the proper platform (PC, Mac & Linux Standalone) and select Build button. Make sure the MainScene is included in Scenes In Build in the build menu.

IT IS IMPORTANT for Git LFS to be installed and configured correctly when forking or cloning, otherwise the models may not download with the rest of the project.

## How To Play
Controls:
- WASD to move
- SPACE to jump
- WASD + LSHIFT to dash
- WASD + LCTRL to hyperdash
- Left Click plants to remove
- Left Click ground to add plants
- Right Mouse + Drag to move camera
- Right Mouse + Mouse Scroll to change camera zoom

## Attributions
We made all the assets used in this game ourselves, except for the music, which was downloaded from musopen.org. Much of the code used is adapted from tutorials for specific processes published on YouTube; many of these are linked in the code they inspired.


## Procedural Generation
The default seeds are
- 42 for Flora
- -50 for terrain

These seeds can be changed in the Unity Editor on the Mesh Generator Script of the Mesh Generator object to procedurally generate new shapes and distributions of flora.
The actual probabilities for different biomes can be adjusted with the transition matricies of the Mesh Generation object.
