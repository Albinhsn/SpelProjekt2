//Catwalk polisher

VAR firstInteraction = false

-> main
=== main ===
{firstInteraction: ->stich1 | ->stich2}

= stich1
NO! Don't take another step! I've had it with these catwalks! They are supposed to shine but everyone keeps using them with their dirtcovered boots! I'm stopping this! No one is allowed to use the catwalks anymore. <i>Hmpf</i>
->DONE

= stich2
I said no!!!
    * [I wont use the catwalks I promise. I just wanted to talk]
        You want to talk to me? Oh, ehm... I don't know. We're not really supposed to talk with you.
        ** [Why not?]
            Because you're weird and different. 
            <i>Dialogue tree follows</i>
        ->DONE
    * [Yeah, no. Sure I wont use the catwalks]
        ->END
->DONE


->END