//Boxes guy
VAR talked = false

->main
=== main ===

{talked: ->stitch1 | ->stitch2}

=stitch1
Good that you're staying away from that box.
->DONE

=stitch2
~talked = true
Hey, if you're thinking of moving that box; I wouldn't. # speaker=Unknown #camera=0
It's very close to that forcefield and if they collide that box is gonna disintegrate. 
+ [> <i>Acknowledge</i>]
    ...Okay? I take it I <i>don't</i> want the box to disintegrate. # speaker=Alea
        You really hit the nail on the head with that one! # speaker=Boxesguy
        No, you don't want that. That box has been sanctified by the Boxmover,
        and he doesn't take it lightly to someone moving his boxes. 
        But don't worry about yourself being disintegrated, 
        The forcefield is only prejudiced against boxes.
        ++ [> <i>Acknowledge</i>]
            I see. # speaker=Alea
            Also, you see that ghostly outline of a cube? That's the box's spawner. # speaker=Boxesguy #camera=1
            Every box has a spawner that respawns it if it disintegrates. 
            +++[> <i>Inquire: Importance</i>]
                Why are you telling me all this? # speaker=Alea #camera=0
                    Well I'm sorry for trying to be helpful. # speaker=Boxesguy
                    God outsiders are as bad as they say.

->DONE      
->END
