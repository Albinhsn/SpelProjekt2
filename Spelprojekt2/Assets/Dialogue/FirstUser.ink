#font=0
<i>You wander out into the open.</i> #camera=0
<i>Suddenly, a light moves in front of the door.</i> # anim=move:bool=true;
What are you? #speaker=You
I am the USER and your creator. #speaker=USER # anim=idle:bool=true;
It can be disorienting in the beginning, but you will get used to the feeling, Alea.
What do you mean? #speaker=Alea
I'm giving you a directive, your goal is: #speaker=USER
Get to the <color="purple">Bearing</color>. You will get further instructions there. #camera=1
Wait! Can't you... Nevermind. #speaker=Alea #camera=0 # anim=disappear:bool=true;
-> choice
=== choice ===
 * [> <i>Contemplate: The <color="purple">Bearing</color></i>]
 The island looks as if it's upside down.
 -> choice
 * [> <i>Contemplate: The USER</i>]
 A strange thing or person, a guiding light maybe?
 -> choice
 * [> <i>Proceed</i>]
 There's a door ahead of me, it must be the way forward.
    -> END
