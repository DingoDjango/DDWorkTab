# Adding Textures for Modded RimWorld Jobs to DD WorkTab

What you'll need*:
- The job's **\<defName>** tag. More on this later.
- A full colour texture (as found in ***DD WorkTab\Textures\Work***).
- An outline texture (as found in ***DD WorkTab\Textures\Work_Disabled***).
- A greyscale texture (as found in ***DD WorkTab\Textures\Work_Greyscale***).

\*Technically, you can get away with making one texture and copying it twice. However, you won't be able to tell if the job is enabled in-game.

## Getting the Job's defName

1) Go into the mod's folder. I'll use vanilla's **Core** "mod" in this example.
2) Find the XML file which contains WorkTypeDef elements. In this example it's ***Core\Defs\WorkTypeDefs\WorkTypes.xml***.
3) Find the job type you want to texture. Let's take Firefight as an example.
4) Inside the job type's XML def (wrapped in \<WorkTypeDef> tags) you'll find a tag called \<defName>. In this case, Firefight's defName is ***Firefighter***.

Now that we have our defName, we can make textures.

## Making the Textures

I'm just kidding. I have no idea how to make these. You're on your own for this one!

## Naming the Textures

So now that I've thoroughly explained how to make textures, you'll need to place your textures in the appropriate folder. Each texture has to be named exactly the same as the defName we found above. They also need to be PNG files.

So, in our example, we have 3 PNG textures all named ***Firefighter.PNG***.

- Place the full-colour, normal texture in the **DD WorkTab\Textures\Work** folder.
- Place the outline texture in the **DD WorkTab\Textures\Work_Disabled** folder.
- Place the greyscale texture in the **DD WorkTab\Textures\Work_Greyscale** folder.

## Testing & Publishing

All you have to do now is go in-game and see if your textures loaded correctly.

If you made some nice textures and want to share them with the world, you can make a pull request on GitHub, a forum post, a Steam comment with a link to download them etc. etc. When I have some spare time I promise to update the mod with all user-made textures.
