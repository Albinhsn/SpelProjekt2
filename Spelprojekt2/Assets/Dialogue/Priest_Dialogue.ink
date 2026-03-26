// Early Priest iteration
// Must be interacted with fully before progressing
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
... can't you just help us? This is your world; why would you just let it burn? # speaker=Priest #anim=isTalking:bool=true;
    * [> <i>Inquire: Priest</i>] Eh... hi? Are you the Priest? # speaker=Alea #anim=isTalking:bool=false;
        Hmm? Oh yes, I am. Who are you? And why are you in my study? # speaker=Priest #anim=isTalking:bool=true;
        ** [> <i>Initiate introduction</i>] I'm Alea. Someone told me to find you; # speaker=Alea #anim=isTalking:bool=false;
        they said you could help me find a way to the <color="purple">Bearing</color>. 
            ->sacred

->DONE

= stitch2
<i>sigh</i> Very well. What do you want to know? #speaker=Priest #anim=isTalking:bool=true;
            -> choice
            = choice
            * [> <i>Inquire: USER</i>]
                Who is the USER? # speaker=Alea #anim=isTalking:bool=false;
                The USER is our God. # speaker=Priest #anim=isTalking:bool=true;
                He is the creator of our world and has sole authority over it and it's people.
                I am – or rather was – His messenger. 
                I have been spreading His teachings to the other localites. 
            -> choice
            * [> <i>Inquire: People</i>]
            Who are all of you? Why are you here and what is this place? # speaker=Alea #anim=isTalking:bool=false;
                We are the localites and this is a <color="yellow">Construction Site</color>. # speaker=Priest #anim=isTalking:bool=true;
                It is all that remains of this island. 
                It was once a city with busy streets. But now all that's left is the irony of it all.
                ** [> <i>Expound</i>]
                    What do you mean? What happened? # speaker=Alea #anim=isTalking:bool=false;
                        The distortion came. Glitches that destroyed anything they touched. # speaker=Priest #anim=isTalking:bool=true;
                        I guess the USER wasn't happy with us. He let the glitches run rampant across the island, 
                        ripping and tearing, destroying our home and everyone in it. 
                        And then they just stopped. He couldn't even be bothered to finish all of us off.
                        Everyone looked to me for guidance, to tell them why this was happening, but I had nothing to offer. 
                        He abandoned us and left us to be consumed by this distortion so to answer your question: 
                        There is no reason as to why we are here, we obviously were never supposed to, 
                        there is no point to our existence and there never was... 
                        our lives is just one cruel joke.
                            *** [> <i>Express sympathy</i>]
                            Oh...I'm sorry. #speaker=Alea #anim=isTalking:bool=false;
                            You and me both. #speaker=Priest #anim=isTalking:bool=true;
                            ->choice
                ** [> <i>Disengage subject</i>]
                Okay then... #speaker=Alea #anim=isTalking:bool=false;
            -> choice
            + [> <i>Proceed</i>]
            Thank you for your time. #speaker=Alea #anim=isTalking:bool=false;
            ->END
= sacred                
            The sacred ground? What would someone like you need to do there? # speaker=Priest #anim=isTalking:bool=true;
            Someone like me? I don't understand what everybody finds so strange about me. # speaker=Alea #anim=isTalking:bool=false;
            Have you looked at yourself? # speaker=Priest #anim=isTalking:bool=true;
            You're bipedal and your features are so... round, its unnerving. 
            If you had asked before all this happened I probably wouldn't even have helped you, 
            but... <i>sigh</i> I just can't be bothered anymore. 
            If you wish to go to the <color="purple">Bearing</color> then <color="yellow">go to the roof</color>, 
            if you're lucky, and He wants to talk, then you will be able to move forward.
            Thank you. # speaker=Alea #anim=isTalking:bool=false;
            Yeah, yeah, just go already. Oh and... <i>may he guide your path</i>, and all that. # speaker=Priest #anim=isTalking:bool=false;
            + [> <i>Withdraw from interaction</i>] 
            ->DONE
            * [> <i>Further inquiring</i>]
            I was told you might be able to answer some questions. #speaker=Alea #anim=isTalking:bool=false;
            ->stitch2
= stitch3
Why? I don't understand... # speaker=Priest #anim=isTalking:bool=true;
    + [> <i>Offer greeting</i>]
        Hi again. #speaker=Alea #anim=isTalking:bool=false;
        <i>Sigh</i> Hi. What do you want? #speaker=Priest #anim=isTalking:bool=true;
        ++ [> <i>Further inquiring</i>] I was told you might be able to answer some questions. #speaker=Alea #anim=isTalking:bool=false;
            ->stitch2
        ++ [> <i>Withdraw from interaction</i>]
        Goodbye. #speaker=Priest #anim=isTalking:bool=false;
        ->END
->END
