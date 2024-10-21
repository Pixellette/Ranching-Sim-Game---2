# Peaceful Plains: A Ranching Sim

## Overview

**Peaceful Plains: A Ranching Sim** was created to explore more dynamic and interesting animal behavior in video games. In many games, animals are often portrayed with simplistic, static behaviors, whether they are central to the gameplay or simply part of the environment. *Peaceful Plains* aims to improve upon this by simulating more realistic behaviors, with a focus on flocking and individual animal personalities.
At the core of the project is the use of **Boid behavior** to simulate the flocking dynamics of farm animals like sheep and cows, providing a richer, emergent behavior model that makes animals feel more lifelike.

## Features
**Flocking Mechanics:** Uses Boid principles to implement natural flocking of animals, with specific behaviors for separation, alignment, and cohesion.
**Build Mode:** Allows players to build fences and gates to create paddocks and manage the animals effectively.
**Player Interaction:** Players can use a Shout command to influence animal movement and engage with the environment.
**Varied Animal Behaviors:** Animals have different hunger levels, wander and seek food independently, and exhibit realistic group dynamics.

## Installation and Setup

### Cloning the Repository
1. Clone the repository from GitHub by running:

### Installing Unity Hub and Unity Engine
1. **Download and install Unity Hub** if you do not have it installed.
2. Through Unity Hub, install **Unity Engine version 2022.3.19f1.**
3. Use Unity Hub to open the cloned repository as a Unity project.

### Building and Running the Game
1. Once the project is open in the Unity Editor, navigate to **File > Build and Run.**
2. Set your target platform and build settings if needed.
3. **Build and Run** will create an executable version of the game for you to play.

### Running the Game in Unity Editor
1. To run the game for testing, open the Unity Editor.
2. Load the appropriate scene from the **Hierarchy** tab.
3. Use the **Scene View** to move and position game objects as needed.
4. Press the **Play** button at the top of the Unity Editor to test in the **Game View.**
5. Use the **Inspector** to adjust object properties during runtime.

## User Guide

### Gameplay Overview
In *Peaceful Plains*, you manage herds of animals, build paddocks, and explore a dynamic environment where animals exhibit natural flocking behavior. You can use **Play Mode** for regular gameplay or enter **Build Mode** to design paddocks and manage the area effectively.

### Controls

#### Play Mode
- **WASD:** Move the player (W/S for forward/back, A/D for strafe left/right).
- **Mouse:** Look around.
- **Shift:** Run.
- **Spacebar:** Jump.
- **TAB:** Open the pause menu.
- **Right Mouse Button:** Use the shout command to influence animals.
- **B Key:** Enter Build Mode.

#### Build Mode
- **WASD:** Move the camera (up/down/left/right).
- **R/F:** Zoom the camera in/out.
- **Q/E:** Rotate the selected object.
- **UI Buttons:** Switch between placing fences or gates, and toggle between place and delete modes.
    - **Note:** There is a known bug where switching directly from delete mode to placing a gate or fence can cause the current build mode to break.
- **TAB:** Open the pause menu.

### Important Notes
- There is **no save functionality** currently implemented. However, the game includes a Main Menu to quit or start a new game and an in-game pause menu.
- **Warning:** If the player is determined to climb the mountains surrounding the play space and falls off the terrain, the game will break, and there is no way to fix it during play. The player will need to quit and reload the game.

## Possible Future Improvements
- A save game feature may be added for players wanting to return to their progress.
- Adding invisible walls or similar constraints to prevent players from leaving the playable area.

## License
This project is for educational purposes and is a part of a university capstone project.

## Contributions
Contributions are not currently being accepted as this is a completed capstone project.

---

Feel free to explore, test, and enjoy the simulation of dynamic animal behaviors!