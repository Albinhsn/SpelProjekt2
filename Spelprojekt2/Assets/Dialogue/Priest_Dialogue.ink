// Early Priest iteration
// Must be interacted with fully before progressing

# font=0

VAR first = true
VAR info = false

->main

===main===
{first: ->stitch1|->stitch3}

= stitch1
~first = false
<i>The man is mumbling to himself. Unaware of your presence.</i>
... can't you just help us? This is your world; why would you just let it burn? # speaker=Priest?
    * [Eh... Hi?] Are you the Priest? # speaker=Alea
        Hmm? Oh yes, I am. Who are you? And why are you in my study? # speaker=Priest
        ** [I sort of stumbled in here.] #speaker=Alea
            Well then, Alea; why don't you just sort of, stumble right on out again? #speaker=Priest
            ->END
        ** [I'm Alea. Someone told me to find you;] They said you could explain what's happening around here. # speaker=Alea
            ->stitch2
->DONE

= stitch2
That would be the good-for-nothing Nitwit I presume. She's the only one who would even consider helping an outsider. #speaker=Priest
Very well. What do you want to know?
            -> choice
            = choice
            + [USER.]
            Who is the USER? # speaker=Alea
            You're truly not one of ours. The USER is our one and only God. # speaker=Priest
            He is the creator and sole authority of our world and it's people.
            I am His messenger. I have been spreading His teachings to the localites.
            -> choice
            * [Move on.]
            I need to know above all else: # speaker=Alea
            How do I get to the <color="purple">Bearing?</color>
            You would go to our sacred ground? A fool's journey for someone like you. # speaker=Priest
            Why is that? I don't really understand what's so strange about me. # speaker=Alea
            Have you looked at yourself? But I suppose it's not something you have considered. # speaker=Priest
            But I shouldn't be pointing out the obvious, outsider.
            If you wish to go to the <color="purple">Bearing</color> then so be it.
            Go to the roof, if you're a chosen then you will be able to move forward, through the door.
            Thank you. # speaker=Alea
            Don't, I wont accept it. # speaker=Priest
            ->DONE
= stitch3
Why? I don't understand...
    + [Hi again.]
        <i>Sigh</i> Hi. What do you want?
        ++ [I'm Alea. Someone told me to find you;] They said you could explain what's happening around here. # speaker=Alea
            ->stitch2
        ++ [Nothing.] It's nothing. # speaker=Alea
        Nothing. Then stop wasting my time. # speaker=Priest
->END