// Early Nitwit Dialogue

VAR FirstIneractionN = false
VAR PickItUp = false

-> main
=== main ===

{FirstIneractionN: ->Calm | ->Surprise}

= Surprise
~FirstIneractionN = true
OH JEEZ! UH... HELLO? #speaker=Unknown
    * [Hello?]
        WOAH, you talk like us! That's cool. I mean. #speaker=Unknown 
        Who are you stranger? What are you doing here in the localites sacred land? #speaker=Unknown
        ** [I'm Alea, my directive is to go to the <color="purple">Bearing</color>.]
            Huh, I didn't expect that. Must be important if you need to get to the </color="purple">Bearing</color>. 
            But hello Alea, welcome to the <color="yellow">Construction Site!</color> We build... here. #speaker=Unknown
            If you have any questions then shoot! I have literally <i>nothing</i> better to do! #speaker=Nitwit
            -> choice
            =choice
            + [Localites?]
            It's the name of our people. The USER gave it to us. #speaker=Nitwit
            Also be wary, we're typically not really accepting of outsiders. #speaker=Nitwit
            -> choice
            + [The Construction Site.]
            It's been a work in progress since... #speaker=Nitwit
            Oh. I don't even remember what it's meant to be. #speaker=Nitwit
            -> choice
            + [The USER.]
            The USER is our creator and god, He has sole authority over our world.#speaker=Nitwit
            No one's really spoken with Him though, besides the Priest I suppose. #speaker=Nitwit
            -> choice
            * [Move on.]
            Great! Well, you should pick up that thing over there. Seems mysterious. #speaker=Nitwit
            Also if you really want to get to the <color="purple">Bearing</color> then you need to get rid of those glitches over there. #speaker=Nitwit #camera=1
            The Priest should know more, he's on the roof of the second building there. #speaker=Nitwit #camera=2
            Well, good luck! #speaker=Nitwit #camera=0
            -> DONE
    -> DONE

= Calm
    If you haven't picked up the thingy yet, you should. #speaker=Nitwit #camera=0
    I'm kinda blind right now so I can't tell if you did. #speaker=Nitwit
    -> DONE
