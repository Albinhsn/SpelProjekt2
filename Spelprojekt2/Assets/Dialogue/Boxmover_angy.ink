// Boxmover angy 

VAR isAllowedToMoveBoxes = false
VAR secondWarning = false
->main

===main===
->noBox

= noBox
{secondWarning: ->main.grr|->main.hmpf}

= hmpf
~secondWarning = true
HEY! What do you think you're doing moving those boxes?!
    *[oh...Sorry I... I didn't know they were yours]
        They <i>aren't.</i> They're the USER's. It's m... eh forget it; someone like you wouldn't udnerstand anyway. Just don't touch anymore boxes
        ->DONE
        
->DONE
= grr
What did I just tell you?! Didn't I tell you no to touch the boxes? I destinctly remember telling you not to move any boxes!
    *[Okay, yes; you've said that but why? I don't see the harm in moving the boxes]
    I am very close to start calling you slurs
->DONE

->DONE

