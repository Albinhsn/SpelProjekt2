//Early iteration of Boxmover NPC

VAR isInteractedWith = true
VAR isFirstInteraction = true

->main
=== main ===


{isFirstInteraction: ->main.first|->main.second}

= first
...
    + [...hello?]
        ~ isFirstInteraction = false
        <i>He ignores you.</i>
        ->DONE

->DONE
= second
...
    + [<i>Hellooooo?</i>]
        <i>He ignores you harder.</i>
        ->DONE
->DONE

->END



->END