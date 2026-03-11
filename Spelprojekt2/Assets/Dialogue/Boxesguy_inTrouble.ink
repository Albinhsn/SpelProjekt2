//Boxesguy after box broke

VAR boxMoment = true
VAR talked = false
->main

=== main ===
{boxMoment: ->boxBroke | ->talking}

=boxBroke
~boxMoment = false
Oooh boy now you've done it. # speaker=Boxesguy
->DONE

->END


=== talking ===
{talked: ->stitch2 | ->stitch1}
= stitch1
~talked = true
Well I warned you didn't I? # speaker=Boxesguy
What they said about you outsiders really is true.
You just run through life not caring about anything but yourselves. 
+[...]
... # Alea
->DONE
= stitch2
Hmpf # speaker=Boxesguy
->DONE
->END