// Nitwit Dialogue

VAR FirstIneractionN = false
VAR PickItUp = false
VAR NitInteract = false

-> main
=== main ===

{FirstIneractionN: ->Calm | ->Surprise}  // Calm = true and Surprise = false

= Surprise
~FirstIneractionN = true
OH JEEZ! Uh... hello? #speaker=Unknown #camera=0 #anim=isTalking:bool=true;
    * [> <i>Inquiring reply</i>]
        … Hello? # speaker=Alea #anim=isTalking:bool=false;
        WOAH, you talk like us! That's cool, I mean. #speaker=Unknown #anim=isTalking:bool=true;
        Who are you stranger? What are you doing here in the localites land? #speaker=Unknown
        ** [> <i>Initiate introduction</i>]
            I'm Alea, my directive is to go to the <color="purple">Bearing</color>. #speaker=Alea #anim=isTalking:bool=false;
            Huh, I didn't expect that. Must be important if you need to get to the <color="purple">Bearing</color>. #speaker=Unknown #anim=isTalking:bool=true;
            Welcome to the <color="yellow">Construction Site</color>! We... build here! Yes. #speaker=Unknown #anim=isTalking:bool=true;
            I'm usually called "useless" or "go away", but you can call me Nit!
            Also if you really want to get to the <color="purple">Bearing</color> you should go on ahead. #speaker=Nit #anim=isTalking:bool=true;
            The Priest should know more, he's usually in his study. #speaker=Nit #camera=2 #anim=isTalking:bool=true;
            Also pick up that thing over there, I think you'd like it. #camera=1
            Well, good luck! I will just be standing here... #speaker=Nit #camera=0 #anim=isTalking:bool=true;
            without anyone to talk to. Alone... #anim=isTalking:bool=false;
            -> DONE
    -> DONE

= Calm
    You will most likely be hurrying on ahead. #speaker=Nit #camera=1 #anim=isTalking:bool=true;
    But before then... If you have any questions then shoot! 
    I have literally <i>nothing</i> better to do!
            -> choice
            =choice
            * [> <i>Inquire: Localites</i>]
            Who are the Localites? # speaker=Alea #anim=isTalking:bool=false;
            It's the name of our people. The USER gave it to us. #speaker=Nit #anim=isTalking:bool=true;
            Also be wary, we're not really accepting of outsiders.
            -> choice
            * [> <i>Inquire: Construction Site</i>]
            What is this place? #speaker=Alea #anim=isTalking:bool=false;
            A pair of office buildings, it's been a work in progress since... #speaker=Nit #anim=isTalking:bool=true;
            Oh. I don't even remember.
            -> choice
            * [> <i>Inquire: USER</i>]
            I keep hearing of the USER, who is he? #speaker=Alea #anim=isTalking:bool=false;
            The USER is our creator and god, He has sole authority over our world. #speaker=Nit #anim=isTalking:bool=true;
            No one's really spoken with Him though, besides the Priest I suppose.
            -> choice
            * [> <i>Inquire: Nit</i>]
            What about you? What's your deal? #speaker=Alea #anim=isTalking:bool=false;
            You want to know more about... me? HAH HA! #speaker=Nit #anim=isTalking:bool=true;
            <i>Awkward cough</i> I mean... yeah, sure! I'm an open book.
                ->nit
            * [> <i>Proceed</i>]
            If you haven't picked up the thingy yet, you should. #speaker=Nit #camera=1 #anim=isTalking:bool=true;
            It's <i>pretty</i> important!
            Bye for now! #anim=isTalking:bool=false;
                -> DONE
                =nit
                ~NitInteract = false
                ** [> Inquire: Name]
                    How'd you get Nit from useless? #speaker=Alea #anim=isTalking:bool=false;
                    ~NitInteract = true
                    Well, I created my name. They typically reflect our purpose. #speaker=Nit #anim=isTalking:bool=true;
                    I don't really have a purpose. 
                    So I shortened the name the Priest gave me, I think it ended up being pretty nice.
                ->nit
                ** [> Inquire: Relationships]
                    How are you getting along with the other Localites? #speaker=Alea #anim=isTalking:bool=false;
                    ~NitInteract = true
                    Well, I don't have a special someone yet. #speaker=Nit #anim=isTalking:bool=true;
                    But I do have many friends, even here at the <color="yellow">Construction Site</color>.
                    Maybe not the Priest though, he doesn't seem to keen on knowing me.
                ->nit
                ** [> <i>Proceed</i>]
                    Cool! Nice talk. #speaker=Nit #anim=isTalking:bool=false;
                    ->choice
    -> DONE



