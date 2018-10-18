# Build Your Business - Submitting Art
*Updated for game version 0.1.22b. Game version number can be seen from this repo's README*

This document is for DKV only.

All assets, from the moment this document is uploaded, must be submitted through GitHub. Therefore, pay attention to the following:

## Important Folders
Please put the assets in its respective folder:

- Animation: **Animations**
- Model file (FBX): **Models**
- Textures (if separated from the FBX): **Textures**
- 2D Assets: **Sprites**

Always create a new folder inside these folders with the model name!

## Importing to Unity
While the GAT may be able to import all assets for you, it's best if you import it yourself to Unity. To import, just drag-and-drop the file to Unity's Project pane. Make sure that what you see in Unity is what you expected before committing the asset to GitHub

If you need to download Unity, download version **2018.1.1**. Do **NOT** use any other version.

## Models Checklist
1. Always use **Standard** material, with **Bitmap** maps. Any other maps would need to be rendered into Texture.
2. Only assign Diffuse and Bump from modelling software. Any other would need to be done in Unity.
3. Keep poly count low. Optimize model in the software, not Unity.
4. Keep models **centered** on the grid in the software. **Any non-centered models will have to be retried** due to it messing with the Customer's look direction
5. Unity is Y-up. Rotate accordingly.
6. Combine non-functional detail (glass on the table, keyboard on the table) into a single object to reduce clutter. You can ignore this at your own peril.
7. Combine all animations into one FBX to reduce clutter. However, you could separate the animations over multiple FBX **as long as** the following is fulfilled: 
    - There is a base model, without any animations, named **modelName.fbx**
    - Export all animations for this model *preferably without its Mesh* with name **modelName@animationName.fbx**
    - Import to Unity, then check **Preserve Hierarchy** in the import settings for all animations. 
    - DUe to the possible amount of animations, you set the import settings in Unity on your own. **I will not be doing it**, and if the animations didn't work I'll ask you to retry it.