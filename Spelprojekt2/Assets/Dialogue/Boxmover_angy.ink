// Boxmover angy, active when interacting with boxes
// Boxmover is labelled as "Box Guardian" in dialogue

VAR isAllowedToMoveBoxes = false
VAR secondWarning = false
VAR movedBox = false

->main

===main===
->noBox

= noBox
{secondWarning: ->main.grr|->main.hmpf}

= hmpf
~secondWarning = true
HEY! What do you think you're doing moving those boxes?! #speaker=Box Guardian
    * [Oh... Sorry.] I didn't know they were yours. #speaker=Alea
        They <i>aren't</i>. They're the USER's. It's m... eh forget it; someone like you wouldn't understand anyway. #speaker=Box Guardian
        Just don't touch anymore boxes.
        ->DONE
        
->DONE

= grr
What did I just tell you?! #speaker=Box Guardian
Don't. Touch. The. Boxes.
I distinctly remember telling you that, are you deaf? 
    + [Okay, yes. But why?] I don't see the harm in moving the boxes. # speaker=Alea
    Oh USER, you're slow on the uptake. You wouldn't understand and that's that! #speaker=Box Guardian
        The USER put them there and that's where they will stay, end of story! #speaker=Box Guardian
    ->DONE