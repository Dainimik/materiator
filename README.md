This software, called `Materiator`, was developed to address performance issues related to game rendering on mobile devices in [Unity software](https://unity.com/) using the [Universal Rendering Pipeline (URP)](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@16.0/manual/index.html).

## Problem
The game required numerous meshes with different material variations, which posed a challenge. When multiple meshes shared the same material, they could be rendered together in a single batch by the GPU. However, encountering a mesh with a different material prevented batching, leading to the termination of the current batch and creation of a new batch specifically for that mesh and its material. The remaining meshes with the original material would then be placed in a separate batch. As a result, even a single different material would split the batch into three resulting in three separate draw calls that the CPU had to prepare and send to the GPU. Since the game needed to support not just two but several dozen materials, this resulted in extremely small batches which led to suboptimal performance on mobile devices, causing excessive battery consumption and generating significant heat.

## Solution
The solution leverages the fact that the URP batches draw calls of meshes which share the same shader variant. Most numerical material values can differ from one mesh to another and the meshes still will be batched as long as they share the same shader variant. Therefore, the solution is to encode shader values the mesh needs as RGB values and store than in an atlas texture. This atlas texture can be sampled during runtime to retrieve the necessary information, apply it to the material, and render the mesh accordingly. This results in batching being preserved.

### Steps

- First, export meshes from a 3D mesh modeling software in FBX format with a custom property assigned. This property should map the UV island coordinates to specific names, allowing identification of the corresponding UV islands within the software. For instance, a human model may have UV islands representing boots, a hat, skin, etc.
- Within the Unity software, there is an `AssetPostprocessor` class responsible for processing assets during import. `Materiator` extends this class and focuses on processing meshes. Its purpose is to extract the custom property that associates UV islands with their respective names.
- After extracting the data, `Materiator` creates an instance of `MateriaSetter` and stores this information within it. In the user interface, developers can then assign `Tags` to these slots. These `Tags` serve as indicators for mapping specific UV islands to corresponding sections of the atlas texture.
- Developer adds a list of `Tags` and the corresponding `Materia` they want each `Tag` to have. This information is added to the `Atlas`.
- With the necessary data in place, the atlas texture is generated from the `Atlas` asset.
- Finally, the generated atlas texture is assigned to a `MateriaSetter` instance, which applies the material to the mesh renderer.

Specific assets called `ScriptableObject` is used in this software:

- `ShaderData` — This asset specifies a shader to be used and stores its property names. Each property name is mapped to the R, G, B, and A channels of the texture. Furthermore, the purpose of each channel can be defined (e.g., whether it holds metallic value, glossiness, fresnel strength, or any other float value defined in the shader).

- `MaterialData` — This asset stores references to a `ShaderData` and to a Unity's material.

- `Tag` — This asset acts as a bridge, facilitating the mapping of UV islands on the `MateriaSetter` to specific `Materias` on the `Atlas`.

- `Materia` — By referencing `MaterialData`, this asset generates a user interface based on the shader properties defined within `MaterialData`. It allows developers to modify the corresponding material values.

- `Atlas` — This asset maps a `Tag` to each `Materia` and holds the responsibility of generating the 2D atlas texture. During the atlas texture generation process, the `Atlas` asset collects all meshes with a `MateriaSetter` instance, adjusts their UV coordinates to fit within the 0-1 UV space of the atlas texture, and assigns the values from the `Materias` to the corresponding atlas texture sections occupied by the UV islands, based on their associated `Tags`. Additionally, the Atlas object holds a reference to the material to which it needs to apply the generated texture.

# Result

All meshes sharing this material will be batched into a single draw call while all having different shading values. This greatly increases the rendering performance, especially when there are many meshes being rendered at the same time.

## Images

- ShaderData

![ShaderData asset UI](/images/shader-data.png)

- MaterialData

![MaterialData asset UI](/images/material-data.png)

- Materia

![Materia asset UI](/images/materia.png)

- Atlas

![Atlas asset UI](/images/materia-atlas.png)

- MateriaSetter

![MateriaSetter UI](/images/materia-setter.png)

- Material with  atlas textures applied

![Material](/images/material.png)
