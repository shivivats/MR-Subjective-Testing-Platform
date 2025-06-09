# STEP-MR: A Subjective Testing and Eye-Tracking Platform for Dynamic Point Clouds in Mixed Reality

## System Requirements

This platform was tested on a laptop with an i7-12700H, 32 GB of DDR5-4800 MHz memory and an NVIDIA RTX A2000 8GB GPU. These are all high-end specs (as of mid-202), but the only absolute requirement here is the memory[^1]. At least 32 GB of memory is recommended. Having a GPU with >=8GB of memory will also be beneficial.

## Software Pre-requisites

The project has been tested with Unity version 2021.3.19f1. Please only use the same, as using newer versions can introduce bugs.

The project uses MRTK2 to work with the HoloLens 2. Check the tools needed to use MRTK2 [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools?tabs=unity) and install them.

## Preparing the Data
The point clouds PLY files need to be in binary little-endian format. This platform uses ["Pcx - Point Cloud Importer/Renderer for Unity"](https://github.com/keijiro/Pcx), and it only works with that format.

One PLY file per frame is utilised to animate the object on the screen.

## Set up the Project
1. Download this repository and extract it.
2. Place the prepared point cloud files in the `Assets\Resources\PointClouds\<Name>\q#\PointClouds` folder[^2].
3. Select to `PointClouds->Update Point Clouds From Assets` in the menu bar to let the project configure itself using the added objects.
4. MRTK should already be set up correctly but double-check the XR settings according to the MRTK documentation.
5. Enable/disable [Holographic Remoting](https://learn.microsoft.com/en-gb/windows/mixed-reality/mrtk-unity/mrtk2/features/tools/holographic-remoting?view=mrtkunity-2022-05) as you desire.

## Point Clouds Loader
For all functionalities of this platform, there will be a `Manager` object in their respective scene. It contains a `Point Clouds Loader (Script)` component. It is responsible for loading the point clouds/meshes into memory.

Add new elements to the `Pc Objects` array in this component and provide information regarding the object name and quality levels present in the project. Only the objects and qualities mentioned here will be loaded into memory and are the only ones that can be used/interacted with.

Other values in this script should not be changed.

## Point Cloud Preview
The Point Cloud Preview scene can be found in `Assets\_ConfigurationScene\ConfigurationScene.unity`.

1. Configure the `Point Clouds Loader` as desired.
2. The distance slider min and max values can be changed by updating the associated values in the `Distance Changer (Script)` component.
3. Run the scene.

The user can see and control the objects appearing on the screen. Up to 4 objects can be displayed and configured individually. The animation and interaction can be toggled. The objects can also be picked up and moved using the [Bounds Control](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/ux-building-blocks/bounds-control?view=mrtkunity-2022-05) feature of MRTK2.

## Subjective Testing
The Subjective Testing scene can be found in `Assets\_SubjectiveTesting\SubjectiveTestingScene.unity`.

Configure the `Point Clouds Loader` as desired.

### Configure the Test
The `SubjectiveTest` object in the scene has an `ST Manager (Script)` component. It is used to configure the tasks.

Open the `Tasks` array in the component and add task elements. Select the desired point clouds, representations, distances and qualities for each task. Refer to the paper for more details.

The `Randomise Tasks` toggle controls whether the tasks are randomised. The sequences within the tasks are always randomised.

The `Use Fixed Y Offset For Distance` will add a height offset equal to `Y Offset` (in meters) to all objects displayed. You can use this to make your objects appear as if they are standing on the ground.

### Running the test

A `Start` button will be displayed before the start of each task. After every sequence, the test participant will be asked to give feedback between 1 and 10 using an immersive slider. The feedback is stored in the `Assets\CSV\ratings.csv` file using [CSVHelper](https://joshclose.github.io/CsvHelper/).

[^1]: The project does not use that much memory now, as it only loads the next 5 point cloud sequences needed. However, it has not been tested with a system having less than 32GB of memory, so nothing can be promised if less memory is available.

[^2]: <Name> is the name you want to associate with the point cloud object. q# signifies the quality level, for e.g. `q1` or `q2`. Each quality level will have its own folder.

## Eye-Tracking
Eye tracking functionality is new in this version of the platform.

The point cloud loading is the same as described above. The sequences will be played twice, and the user's head position and gaze direction will be recorded every frame into a CSV file.

## Citation

```
TBD
```
