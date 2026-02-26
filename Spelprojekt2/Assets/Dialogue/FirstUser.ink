#font=0
...
<i>Suddenly, a light moves in front of the door.</i> #camera=0 # anim=move:bool=true;
What are you? #camera=0 #speaker=You
I am the USER and your creator. #speaker=USER # anim=idle:bool=true;
It can be disorienting in the beginning, but you will get used to the feeling, Alea.
What do you mean? #speaker=Alea
I'm giving you a directive, your goal is: #speaker=USER
Get to the <color="purple">Bearing</color>. You will get further instructions there. #camera=1
Wait! Can't you... Nevermind. #speaker=Alea #camera=0 # anim=disappear:bool=true;
-> choice
=== choice ===
 + [The <color="purple">Bearing</color>.]
 The island looks different in comparison to the other ones.
 -> choice
 + [The USER.]
 A strange thing or person, a guiding light maybe?
 -> choice
 * [Move on.]
 There's a door ahead of me, must be the way forward.
    -> END
