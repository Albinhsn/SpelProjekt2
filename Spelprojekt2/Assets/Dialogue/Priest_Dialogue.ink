// Priest
VAR first = true
->main

===main===
{first: ->stitch1|->stitch3}

= stitch1
~first = false
<i>The man is mumbling to himself</i>
...can't you just help us? This is your world; why would you just watch it burn?
    + [ehrm... hi? Are you the Priest?]
        Hmm? Oh, yes I am. Who are you? And why are you in my study?
        ++ [I'm Alea and I just sort of stumbled in here]
            Well then; why don't you just sort of stumble out again?
            ->END
        ++ [Someone told me to find you; They said you could explain what's happening here]
            ->stitch2
->DONE

= stitch2
That would be the good-for-nothing Nitwit I presume. They are the only one who would even think to help an outsider. Very well; what do you want to know?
            <i>Dialogue tree follows</i>
            ->DONE
->DONE

= stitch3
Why? I don't understand...
    + [Hi again]
        <i>Sigh</i> Hi, yes; what do you want?
        ++ [Someone told me to find you; They said you could explain what's happening here]
            ->stitch2
->END