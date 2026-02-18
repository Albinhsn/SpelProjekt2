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
    * [I wont use the catwalks, I promise.] I just wanted to talk. # speaker=Alea
        You want to talk to me? Oh, eh... I'm not sure. #speaker=Catwalk Lady
        We're not really supposed to talk with people like you. 
        ** [Why not?]
            Because you're weird and different. #speaker=Catwalk Lady
            Can't really do anything about it...
        -> DONE
    * [Yeah, sure.] I wont use the catwalks. # speaker=Alea
        Good! Good. Right, great that we could clear that up. # speaker=Catwalk Lady
        ...
        Please leave, now.
        -> DONE

= stich2
    I said no! #speaker=Catwalk Lady
    <i>Leave</i>.
-> DONE