// Nitwit Dialogue

VAR FirstIneractionN = false
VAR PickItUp = false
VAR NitInteract = false

-> main
=== main ===

{FirstIneractionN: ->Calm | ->Surprise}  // Calm = true and Surprise = false

= Surprise
~FirstIneractionN = true
OH JEEZ! Uh... hello? #speaker=Unknown #camera=0
    * [> <i>Inquiring reply</i>]
        … Hello? # speaker=Alea
        WOAH, you talk like us! That's cool, I mean. #speaker=Unknown 
        Who are you stranger? What are you doing here in the localites land? #speaker=Unknown
        ** [> <i>Initiate introduction</i>]
            I'm Alea, my directive is to go to the <color="purple">Bearing</color>. #speaker=Alea
            Huh, I didn't expect that. Must be important if you need to get to the <color="purple">Bearing</color>. #speaker=Unknown
            Welcome to the <color="yellow">Construction Site</color>! We... build here! Yes. #speaker=Unknown
            I'm usually called "useless" or "go away", but you can call me Nit!
            Also if you really want to get to the <color="purple">Bearing</color> you should go on ahead. #speaker=Nit
            The Priest should know more, he's usually in his study. #speaker=Nit #camera=2
            Also pick up that thing over there, I think you'd like it. #camera=1
            Well, good luck! I will just be standing here... #speaker=Nit #camera=0
            without anyone to talk to. Alone...
            -> DONE
    -> DONE

= Calm
    You will most likely be hurrying on ahead. #speaker=Nit #camera=1
    But before then... If you have any questions then shoot! 
    I have literally <i>nothing</i> better to do!
            -> choice
            =choice
            * [> <i>Inquire: Localites</i>]
            It's the name of our people. The USER gave it to us. #speaker=Nit
            Also be wary, we're not really accepting of outsiders.
            -> choice
            * [> <i>Inquire: Construction Site</i>]
            A pair of office buildings, it's been a work in progress since... #speaker=Nit
            Oh. I don't even remember.
            -> choice
            * [> <i>Inquire: USER</i>]
            The USER is our creator and god, He has sole authority over our world. #speaker=Nit
            No one's really spoken with Him though, besides the Priest I suppose.
            -> choice
            * [> <i>Inquire: Nit</i>]
            You want to know more about... me? HAH HA! #speaker=Nit
            <i>Awkward cough</i> I mean... yeah, sure! I'm an open book.
                ->nit
            * [> <i>Proceed</i>]
            If you haven't picked up the thingy yet, you should. #speaker=Nit #camera=1
            It's <i>pretty</i> important!
            Bye for now!
                -> DONE
                =nit
                ~NitInteract = false
                ** [> Inquire: Name]
                    ~NitInteract = true
                    Well, I created my name. They typically reflect our purpose.
                    I don't really have a purpose. 
                    So I shortened the name the Priest gave me, I think it ended up being pretty nice.
                ->nit
                ** [> Inquire: Relationships]
                    ~NitInteract = true
                    Well, I don't have a special someone yet. 
                    But I do have many friends, even here at the <color="yellow">Construction Site</color>.
                    Maybe not the Priest though, he doesn't seem to keen on knowing me.
                ->nit
                ** [> <i>Proceed</i>]
                    Cool! Nice talk. #speaker=Nit
                    ->choice
    -> DONE



