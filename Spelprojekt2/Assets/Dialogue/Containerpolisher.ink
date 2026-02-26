// Iteration upon Catwalk Polisher, Early Container Polisher

VAR isFirstInteractionC = true

-> main
===  main ===

{isFirstInteractionC: ->polish1  | ->polish2}

=polish1
~isFirstInteractionC = false
This spot here and another here. It never ends... # speaker=Unknown # camera=0
* [Hello?] Excuse me, can I ask- # speaker=Alea
NO! Hold on! You can't just walk up to these containers! they can't get smudged by your... # speaker=Container Cleaner # camera=1 # anim=Turn:bool=true;
Something, the Priest has said to watch out if someone weird comes up.
I do not intend for whatever <em>bodily liquids</em> stored inside you to be splattered all over these containers.
** [Why?] Would something happen if they did get dirty? # speaker=Alea
I'm not saying that I'm planning on going to the bathroom anytime soon. I don't think.
You...? For something I've never seen, you know about bathrooms?
*** [Why wouldn't I??] That's pretty normal? # speaker=Alea
Weird, anyway keep away! # speaker=Container Cleaner
**** [I need to get through.] It's about the Priest you mentioned. # speaker=Alea
Ugggghhhh. OKAY! FINE! I'm not the one who put these stup- I mean, nice containers here anyway. # speaker=Container Cleaner
Thank youuu <em>USER</em>! Now shoo, I need to get back to cleaning as soon as you pass through. # camera=0 # anim=Turn:bool=false;
->DONE
=polish2
<em>She seems to be whispering to herself as she cleans the container.</em> # camera=0
Why did I do start doing this? Well, no going back. It's what I am now. # speaker=Container Cleaner
->DONE