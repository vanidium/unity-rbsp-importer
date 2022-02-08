# unity-rbsp-importer
Just a small weekend project to import compiled BSP files (Raven-variant).
It's super incomplete, but maybe one day it will be usable. It has most things needed - at least in a "ready to start with" state.

### The List
- [x] Loading RBSP files
- [x] Polygon surfaces
- [x] Mesh surfaces
- [x] Bezier surfaces
- [ ] Billboard surfaces
- [x] Loading and using textures
- [x] Parsing .shaders (for the most part)
- [ ] Using .shaders (there is SOME support, but it needs much, much more with a specialized Unity Shader) 
- [ ] Using baked lightmaps (Unity can do a better job at this, probably)
- [ ] Using surface flags

So, it is currently able to load an (r)bsp file, display the faces with mostly the correct textures (there are issues with .shader files, it tries it's best to grab the texture from the associated shader, but it's guaranteed, especially if the texture relies on blendFunc and rgbGen a lot).

Tested with Jedi Academy and Jedi Outcast maps with all their pk3's extracted to a folder. 

![Jedi Academy - mp/ffa3](https://github.com/Vanidium/unity-rbsp-importer/blob/main/Screenshots/ja-ffa3.png?raw=true)
Jedi Academy - mp/ffa3

![Jedi Outcast - ffa_bespin](https://github.com/Vanidium/unity-rbsp-importer/blob/main/Screenshots/jo-ffa-bespin.png?raw=true)
Jedi Outcast - ffa_bespin

I do not condone using this to "remake" games, or "reuse maps and assets" from released games that use this format.
