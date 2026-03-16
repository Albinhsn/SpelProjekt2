//Early iteration of Boxmover NPC

# font=0

VAR isFirstInteraction = true

->main
=== main ===

{isFirstInteraction: ->main.first|->main.second}

= first
... #Speaker=Unknown
    * [Hello?]
        ~ isFirstInteraction = false
        <i>He ignores you.</i> #speaker=Unknown
        -> DONE
= second
...
    + [Hellooooo?]
        <i>He ignores you harder.</i> #speaker=Unknown
        -> DONE