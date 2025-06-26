
# <ins> The basics </ins>

This project is a chess engine. Its not professional or high quality or even that good, but it is what it is.

I decided to make a chess engine because of my love for the game and after seeing **Sebastian Lague's** videos on the topic.
I used C# because its a good language imo and doesnt have me shoot my foot off with manual memory management. (And its faster than python or js, and it **IS BETTER** than java).

Enough with that, lets get onto how this shit works.

## Basics of a chess engine

Humans play chess with thier mind and make moves based on intuition, pattern recognintion and principals they have been taught.
We rely on things like memory of past games and make plands and attacks and such.

Of course, a computer can't do this becuase it doesn't have a *"mind"* (unless, neural networks and stuff, which are not something one would make just for a chess engine).
So, instead of making a computer "think", we exploit its speed. We, let say, look into the future of a current position. That is, we look at all the possible moves in a position and determine which one leads to the best **evaluation**, which is a score given to a positon, which indicated which side is better (positive means white is better, negitive means black is, and 0 means its roughly equal).

Its much more than that. But that is why I have an entire seperate folder called docs isnt it.
