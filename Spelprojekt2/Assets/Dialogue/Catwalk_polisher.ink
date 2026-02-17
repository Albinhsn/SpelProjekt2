//Catwalk polisher

VAR firstInteraction = false

-> main
=== main ===
{firstInteraction: ->stich2 | ->stich1}

= stich1
~firstInteraction = true
NO! Don't take another step! I've had it with these catwalks! #speaker=Catwalk Lady
They are supposed to shine but everyone keeps using them with their dirtcovered boots!
I'm stopping this! No one is allowed to use the catwalks anymore. <i>Hmpf</i>
->DONE

= stich2
I said no!!! #speaker=Catwalk Lady
    * [I wont use the catwalks, I promise. I just wanted to talk.]
        You want to talk to me? Oh, ehm... I don't know. We're not really supposed to talk with you. #speaker=Catwalk Lady
        ** [Why not?]
            Because you're weird and different. #speaker=Catwalk Lady
            //<i>Dialogue tree follows</i>
        ->DONE
    * [Yeah, no. Sure I wont use the catwalks]
        Good! Good. Right, great we can clear that up. 
        ...
        You can leave now.
        -> DONE
->DONE


->END