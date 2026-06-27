
// TODOS
/*
 
  - Light probe blending ? do we need ?
  - Light probe scenario not set at start properly

  - Holding Animations
  
  - Torch flicker
  - Torch vfx
  - Torch fire_sound
  


  == For Build ==
  - [x]  use torch stick gabriel
  - [x?] proper determinitic init order of execution.
  - Sound manager: Ambients sounds follow positions.
  - [TEST] player Step sounds
  - [x?] In Vr Inputs were unreliablty triggering animations. ? maybe press threshold?
  - [x kinda] light baking
  - [x] TEST TELEPORT/WALK aysu prehistoric scene
  - [] no teleport delay


Alright hellope sorry its so late now, wanted to get this done much sooner but here are couple of Notes and toughts:
First of here is another build: https://drive.google.com/file/d/1nnyxMofvRG-Amc1HeTX_9bPsRDawTO9C/view?usp=sharing
If someone can test it tomorrow that would be great then i can maybe still make some changes till monday. 
Unfortunatly I was not able to build the lighting for outside properly yet. I tried, it took 2 hours and was compleatly broken afterwards.
So its just partially baked atm. Need to figure this out.
Im also not sure if the volumetric godrays are working properly i think its the particle ones but for some reason they show much more in the build than in the editor so maybe somthing wrong there.
I also had to disable a bunch of objects there so i could even run it. so most of the foliage is disabled right now.
Regarding post processing i removed the settings asset from the URP config because it seems to overwrite it for the entire project.
Its better to add it to scenes individually and have scenes define the post processing global or in specific areas locally.
I left most things turned On but i disabled chromatic abboration and motion blur. I dont think they'll go well in vr tbh. 
Bloom and Lens flare is still on for now but since those are heavy screen space effects they might slow things down considerably. 
There are some obvious rendering/visual problems for example decals are rendered on top of the controller hands for some reason (my fault prob).
Wind for foliage seems a little strong (differnt during play mode) also the tree trunk is moving.
Other than that i droped in the prehistoric cave as it was in the scene. 
Didn't add any lights so its basically just black. maybe thats cool so we actually need a lightsource there ?  
We can now adjust Audio volume and Head ground offset in the settings menu.
I added my transition shader for the tunnel walls i made on monday but didn't have time to work on it more.

If someone is able to test the build what i would specifically like to know is:
- Are the hand animations responsive when pressing controller buttons ? last time they weren't
- Is the clock too small now? Camera is hopefully not super low anymore so it might look differnt now.
- how is performance ? In editor its crazy slow (sub 30), but in the build it seems fast, but i cant move to really test it.
- during the transition is there noticable stutter/frame lag ? lots of stuff is happening at once and I might have to get creative about it and spread work across multiple frames.


As a side note. I know you guys are not finsihed with the scenes yet. But just for heads up when you are building it further.
Make sure all meshes are actually marked as static and that for each meshrender the 'receive GI' is set to 'Blend Probes' and not lightmaps.
Its best to do this in the prefabs directly.
If you guys could also setup colliders and teleport areas in the respective environments that would also safe me much time when setting up these builds.
You can use the ones from the existing tunnel prefabs and copy paste them.

*/


