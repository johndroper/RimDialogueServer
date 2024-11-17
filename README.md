# RimDialogueClient 

RimDialogue is a fork of Jaxe’s excellent Interaction Bubbles mod, but instead of simply displaying the system-generated interaction text, it takes these interactions and uses a language model to transform them into real dialogue. This dialogue is then displayed in-game above each character’s head in a speech bubble.

For example, an interaction like, "Huynh and Nila chatted about crazy eels," could turn into something like:

Huynh: "I've heard the crazy eels in this jungle are as unpredictable as Bowman's sense of humor, Nila."
Nila: "Tell me about it, Huynh. I've seen them leap out of the water and attack anyone who gets too close."

Alongside the interaction text, the language model receives nearly 100 different data points about each interaction—from the weather and each pawn's health status to the history between the two pawns, and much more. The model uses this data to influence the tone, word choice, and references in each piece of dialogue, creating a dynamic, personalized conversational experience.

But this mod can do even more. By adding “Additional Instructions” in the mod settings, you can influence the entire culture of your colony.

For instance, try adding instructions like:
"Everyone in the colony is obsessed with David Hasselhoff." Now, many conversations will include references to “the Hoff” and Baywatch.
"Bob speaks like Albert Einstein." This will cause only the pawn named “Bob” to talk about astrophysics.
"All the men in the colony speak French." Now, only the male pawns will speak exclusively in French.

Want to recreate the entire cast of Friends? Go for it. The possibilities are endless.

This project is a fork of https://github.com/Jaxe-Dev/Bubbles

This mod also expands the range of conversation topics, adding a few hundred new ones to the interaction system.
This mod uses server resources and needs an internet connection.

The following base methods are patched with Harmony 2.3.3:
```
Postfix : RimWorld.PlaySettings.DoPlaySettingsGlobalControls
Postfix : RimWorld.MapInterface.MapInterfaceOnGUI_BeforeMainTabs
Postfix : Verse.PlayLog.Add
Prefix  : Verse.Profile.MemoryUtility.ClearAllMapsAndWorld
```

