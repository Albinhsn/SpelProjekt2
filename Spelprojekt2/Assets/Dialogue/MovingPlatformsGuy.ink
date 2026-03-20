// Moving Platforms Guy

# font=0

VAR isFirstInteractionM = true

-> main
=== main ===

{isFirstInteractionM: ->main.think | ->main.think2}

= think
~isFirstInteractionM = false
Ey, you there lad or lassy. Can't tell. # speaker=Unknown #camera=0
I've been looking at those platforms ova' there # speaker=Disgruntled Citizen #camera=1 #transition=0
and for the darndest long time I have not figured them out. 
They keep going out of sync, if I had a way to maybe stop one or two I coulda scuttled on over.
But no dice! Aye, I'm too old for this. # speaker=Old Disgruntled Citizen #camera=0 #transition=0
-> DONE
= think2
->questionM
=questionM
Whatchu botherin’ an old localite for? # speaker=Old Disgruntled Citizen #camera=0
*[> Inquire: Reason]
    Why do you want to get to the other side? #speaker=Alea
    Well, who wouldn’t? I need to get to my sermon! #speaker=Old Disgruntled Citizen
    The ignorant youth of today doesn’t even understand how important the USER is!
    **[> Rant Incoming - Initiating: Skip]
        He’s created us for USERs sake! But everyone is just … complaining and… did you know… #speaker=Old Disgruntled Citizen
        The Priest is the only one who understands! 
        Only decent localite ‘round here, I say!
        ->questionM
*[> Inquire: Issue]
    How is it that you can’t get across? #speaker=Alea
    Ain't it darn obvious? The blasted platforms are out-of-sync! #speaker=Old Disgruntled Citizen
    **[>Give Suggestion]
        Is there any swi- #speaker=Alea
        AND THERE’S NO SWITCH! #speaker=Old Disgruntled Citizen
        A predicament indeed…
        ***[> Adjusting Auditory Perception]
        ->questionM
*[> Proceed]
    Platforms, platforms and platforms... #speaker=Old Disgruntled Citizen
-> DONE
