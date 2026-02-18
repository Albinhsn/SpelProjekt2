// Early Catwalk polisher

# font=0

VAR firstInteraction = true

-> main
=== main ===
{firstInteraction: ->stich1 | ->stich2}

= stich1
~firstInteraction = false
NO! Don't you <i>dare</i> take another step! I've had it with these catwalks! #speaker=Unknown
They are supposed to shine! But everyone keeps using them with their nasty dirtcovered- Ugh! # speaker=Catwalk Lady
No one, not a singular <i>person</i> is allowed to use the catwalks anymore, period.
    * [But I need to use the catwalks, I need to talk to the priest]
        ... alright then. But please... no more after that. I can't take it anymore.
        ->DONE

= stich2
    JUST GO ALREADY! #speaker=Catwalk Lady
-> DONE