Thank you for purchasing Explosions Pack.

You can find the demo scene under EXPLOSIONS PACK/Scenes/DemoScene.
You can find the explosion prefabs under EXPLOSIONS PACK/Prefabs.

There's a few included scripts with this pack:

- ExplosionLight animates a point light source over time. Used to fade out an explosion's light source
- TimedObjectDestroy destroys an object after some amount of time. Used to automatically clean up explosion effects when they are finished playing.

There's also a custom particle shader included, HDRAdditiveParticle. This custom shader mimics the built-in Additive shader, with the exception of an HDR Multiplier slider which multiplies the image by some factor between 1 at the low end and 20 at the high end. This makes the particle effects work with tonemapping (stare directly into the flames, and the camera darkens)