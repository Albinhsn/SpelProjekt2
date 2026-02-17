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
HEY! What do you think you're doing moving those boxes?! #speaker=Boxmover
    *[oh... Sorry, I didn't know they were yours.]
        They <i>aren't.</i> They're the USER's. It's m... eh forget it; someone like you wouldn't understand anyway. #speaker=Boxmover
        Just don't touch anymore boxes.
        ->DONE
        
->DONE
= grr
What did I just tell you?! Didn't I tell you not to touch the boxes? I distinctly remember telling you not to move any boxes! #speaker=Boxmover
    *[Okay, yes; you've said that but why? I don't see the harm in moving the boxes.]
    Oh USER, I told you. You wouldn't understand and that's that! #speaker=Boxmover
        The USER put them there and that's where they will stay, end of story! #speaker=Boxmover
    //I am very close to start calling you slurs
->DONE

->DONE

