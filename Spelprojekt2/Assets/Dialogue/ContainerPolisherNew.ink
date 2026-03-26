// Container polisher

VAR firstInteraction = true
VAR helped = false
-> main
=== main ===
{firstInteraction: ->stitch1 | ->stitch2}

= stitch1
~firstInteraction = false
<i>The woman is softly banging her head into the container</i>
It never ends. It never ends. It never ends. It never ends... #speaker=Unknown
+ [> Intervene]
    A..are you alright? # speaker=Alea
        <i>The banging stops</i> #anim=isIdle:bool=true; # speaker=
        No... # disallowSkip # delay=0.1 # speaker=Unknown 
        I'm Alea, whats your name? # delay=default # speaker=Alea
        I'm... the polisher. I polish the containers. #speaker=Polisher #anim=isTalking:bool=true;
        It is my duty to keep the containers shining, an assignment I've been doing ever since the start... # speaker=Polisher
        <i>The woman trails off</i> # speaker= #anim=isTalking:bool=false;
        ++ [> Inquire]
            Do you like polishing the containers? # speaker=Alea
                NO I DON'T LIKE IT! I HATE IT! It's too much... #anim=isTalking:bool=true; # speaker=Polisher
                <i>The woman starts sobbing</i> #anim=isTalking:bool=false; # speaker=
                It never ends, the dust in the air keeps undoing all my work. #speaker=Polisher #anim=isTalking:bool=true;
                I've been working on this container for the last month,
                and I will keep working on it until I throw myself off the edge.
                +++ [> Express Confusion]
                    Why don't you just stop? # speaker=Alea #anim=isTalking:bool=false;
                    Oh... you're an outsider... I'm sorry, I'm not supposed to talk to you. Please leave. # speaker=Polisher #anim=isTalking:bool=true;
                    <i>The woman starts banging her head angainst the container again</i> #anim=isIdle:bool=false; #anim=isTalking:bool=false; # speaker=
                    ++++[> Insist]
                        Please, I want to help you. Why do you keep polishing the containers? # speaker=Alea
                            <i>The banging stops.</i> #anim=isIdle:bool=true; # speaker=
                            Because it's my job. My assignment. # speaker=Polisher #anim=isTalking:bool=true;
                            We all have an assignment we do. 
                            A divine duty from the USER. If I stop, the others will notice and they will judge.
                            I will be ostracised. 
                            But you wouldn't have to work yourself to death. You might be happy.# speaker=Alea #anim=isTalking:bool=false;
                            I guess...maybe... # speaker=Polisher #anim=isTalking:bool=true;
                            Could you carry on with whatever you're doing. I think I need to be alone for a while... #anim=isTalking:bool=false;
                                ~helped = true
                            
                    ->DONE
                    ++++[> Leave her be]
                    ->DONE
        
    ->DONE
+ [> Leave her be]
->DONE
= stitch2
{helped: ->thankfull| ->stitch1}

== thankfull
Ehm... thank you but I need to be alone for a while. # speaker=Polisher
->DONE
->DONE
->END