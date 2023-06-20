# A Platform for Subjective Quality Assessment in Mixed Reality Environments

## System Requirements

This platform was developed on a system with an i9-13900K CPU, 64 GB of DDR5-4800 MHz memory and an NVIDIA RTX 4070 Ti. These are all high-end specs (as of mid-2023), but the only absolute requirement here is the memory[^1]. At least 32 GB of memory is recommended. Having a GPU with >=8GB of memory will also be beneficial.

## Software Pre-requisites

The project has been tested with Unity version 2021.3.19f1. Please only use the same, as using newer versions can introduce bugs.

The project uses MRTK2 to work with the HoloLens 2. Check the tools needed to use MRTK2 [here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools?tabs=unity) and install them.

## Preparing the Data
The point clouds PLY files need to be in binary little-endian format. This platform uses ["Pcx - Point Cloud Importer/Renderer for Unity"](https://github.com/keijiro/Pcx), and it only works with that format.

The meshes need to be generated offline. Ensure the meshes have the correct materials associated with them in Unity! The platform was developed and tested with OBJ files and works well with them.

One PLY or OBJ file per frame is utilised to animate the object on the screen.

## Set up the Project
1. Download this repository and extract it.
2. Place the prepared point cloud files in the `Assets\Resources\PointClouds\<Name>\q#\PointClouds` folder[^2].
3. Similarly, place the prepared mesh files in the `Assets\Resources\PointClouds\<Name>\q#\Mesh` folder[^2].
4. Select to `PointClouds->Update Point Clouds From Assets` in the menu bar to let the project configure itself using the added objects.
5. MRTK should already be set up correctly but double-check the XR settings according to the MRTK documentation.
6. Enable/disable [Holographic Remoting](https://learn.microsoft.com/en-gb/windows/mixed-reality/mrtk-unity/mrtk2/features/tools/holographic-remoting?view=mrtkunity-2022-05) as you desire.

## Point Clouds Loader
For both functionalities of this platform, there will be a `Manager` object in their respective scene. It contains a `Point Clouds Loader (Script)` component. It is responsible for loading the point clouds/meshes into memory.

Add new elements to the `Pc Objects` array in this component and provide information regarding the object name and quality levels present in the project. Only the objects and qualities mentioned here will be loaded into memory and are the only ones that can be used/interacted with.

The `Load Meshes` toggle controls whether the mesh files for the specified objects will be loaded.

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

A `Start` button will be displayed before the start of each task. After every sequence, the test participant will be asked to give feedback between 1 and 10 using an immersive slider. The feedback is stored in the `Assets\CSV\ratings.csv` file using [CSVHelper]https://joshclose.github.io/CsvHelper/).

[^1]: When it starts, the project loads the point clouds and mesh files directly into memory. Solutions are being explored to reduce the memory requirement.

[^2]: <Name> is the name you want to associate with the point cloud object. q# signifies the quality level, for e.g. `q1` or `q2`. Each quality level will have its own folder.

## Citation

```
@inproceedings{Vats_A_Platform_for_2023,
  author = {Vats, Shivi and Nguyen, Minh and Van Damme, Sam and van der Hooft, Jeroen and Torres Vega, Maria and Wauters, Tim and Timmerer, Christian and Hellwagner, Hermann},
  series = {QoMEX 2023},
  title = {{A Platform for Subjective Quality Assessment in Mixed Reality Environments}},
  year = {2023}
}
```
