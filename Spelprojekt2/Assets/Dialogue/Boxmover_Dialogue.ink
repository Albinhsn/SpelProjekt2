//Early iteration of Boxmover NPC

VAR isInteractedWith = true
VAR isFirstInteraction = true
VAR isAllowedToMoveBoxes = false

{isInteractedWith: ->main | ->movedBox}
=== main ===


{isFirstInteraction: ->main.first|->main.second}

= first
...
    * [...hi?]
        ~ isFirstInteraction = false
        <i>He ignores you</i>
        ->DONE

->DONE
= second
...
    * [HelloOOoo?]
        <i>He ignores you harder</i>
        ->DONE
->DONE

->END


=== movedBox ===
{isAllowedToMoveBoxes: ->movedBox.yesBox |->movedBox.noBox}
= noBox
HEY! What do you think you're doing moving those boxes?!
    *[oh...Sorry I... I didn't know they were yours]
        They <i>aren't.</i> They're the USER's. It's m... eh forget it; someone like you wouldn't udnerstand anyway. Just don't touch anymore boxes.
   ->DONE
->DONE

= yesBox

->DONE



->END