//Boxesguy after box broke

VAR boxMoment = true
VAR talked = false
->main

=== main ===
{boxMoment: ->boxBroke | ->talking}

=boxBroke
~boxMoment = false
Oooh boy now you've done it. # speaker=Boxes guy #anim=isTalking:bool=true;
Now we will never get that box back. #anim=isTalking:bool=false;
->DONE

->END

=== talking ===
{talked: ->stitch2 | ->stitch1}
= stitch1
~talked = true
Well I warned you didn't I? # speaker=Boxes guy #anim=isTalking:bool=true;
What they said about you outsiders really is true.
You just run through life not caring about anything but yourselves. 
+[...] # Alea #anim=isTalking:bool=false;
->DONE
= stitch2
Hmpf # speaker=Boxes guy #anim=isTalking:bool=false;
->DONE
->END
