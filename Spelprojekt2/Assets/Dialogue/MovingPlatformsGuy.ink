// Moving Platforms Guy

# font=0

VAR isFirstInteractionM = true

-> main
=== main ===

{isFirstInteractionM: ->main.think | ->main.think2}

= think
~isFirstInteractionM = false
Ey, you there lad or lassy. Can't tell. # speaker=Unknown #camera=0 #anim=isTalking:bool=true;
I've been looking at those platforms ova' there # speaker=Disgruntled Citizen #camera=1 #transition=0
and for the darndest long time I have not figured them out. 
They keep going out of sync, if I had a way to maybe stop one or two I coulda scuttled on over.
But no dice! Aye, I'm too old for this. # speaker=Old Disgruntled Citizen #camera=0 #transition=0 #anim=isTalking:bool=false;
-> DONE
= think2
->questionM
=questionM
Whatchu botherin’ an old localite for? # speaker=Old Disgruntled Citizen #camera=0 #anim=isTalking:bool=true;
*[> Inquire: Reason]
    Why do you want to get to the other side? #speaker=Alea #anim=isTalking:bool=false;
    Well, who wouldn’t? I need to get to my sermon! #speaker=Old Disgruntled Citizen #anim=isTalking:bool=true;
    The ignorant youth of today doesn’t even understand how important the USER is!
    **[> Rant Incoming - Initiating: Skip]
        He’s created us for USERs sake! But everyone is just … complaining and… did you know… #speaker=Old Disgruntled Citizen #anim=isTalking:bool=true;
        The Priest is the only one who understands! 
        Only decent localite ‘round here, I say!
        ->questionM
*[> Inquire: Issue]
    How is it that you can’t get across? #speaker=Alea #anim=isTalking:bool=false;
    Ain't it darn obvious? The blasted platforms are out-of-sync! #speaker=Old Disgruntled Citizen #anim=isTalking:bool=true;
    **[>Give Suggestion]
        Is there any swi- #speaker=Alea #anim=isTalking:bool=false;
        AND THERE’S NO SWITCH! #speaker=Old Disgruntled Citizen #anim=isTalking:bool=true;
        A predicament indeed…
        ***[> Adjusting Auditory Perception]
        ->questionM
*[> Proceed]
    Platforms, platforms and platforms... #speaker=Old Disgruntled Citizen #anim=isTalking:bool=false;
   
-> DONE
