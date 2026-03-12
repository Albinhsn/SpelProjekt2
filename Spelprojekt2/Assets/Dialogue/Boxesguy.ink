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
+ [...Okay?]
    I take it I don't want it to disintegrate. # speaker=Alea
        You really hit the nail on the head with that one; # speaker=Boxesguy
        no, you don't want that. That box has been sanctified by the Boxmover,
        and he doesn't take lightly to moving his boxes. 
        ++ [I see]
            Also, you see that ghostly outline of a cube, thats the box's spawner. #camera=1
            Every box has a spawner that respawns it if it disintegrates. 
            +++[Why is this important?]
                Why are you telling me all this? # speaker=Alea #camera=0
                    Well I'm sorry for trying to be helpfull. # speaker=Boxesguy
                    God outsiders are as bad as they say.
                    
->DONE
                    
->END