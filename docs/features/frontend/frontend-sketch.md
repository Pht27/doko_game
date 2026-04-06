# Frontend

I want to start building a sketch for the frontend for a game.

## Develop Frontend

The goal is to first have a frontend that I can open in the browser, where I can control all 4 players for development purposes. This allows me to test the frontend without needing four devices / sessions. Later on, we can connect that frontend to the hub and play with four real players.

To accomplish that, this special instance of the frontend needs a way to switch between what players 1-4 see. Then, I can play one card with one player and switch to the next player and so on...

I am not yet sure, if we can do that through our current API layer, that is something you need to tell me. If what I want is too different entirely, we should just create a different layer akin to the Doko.Console that we can boot up for testing purposes.

## Frontend Specifications

The frontend should be written in React. It should stay as lightweight as possible without losing any of its necessary functionality. Please think of a sensible component structure and where to use contexts etc.

The frontend is primarily designed to be used on mobile in horizontal view. This is what it should be optimized to, i.e. it should be touch based and aspect ratio will always be a mobile one.

### Hand Display and Playing Cards
On the bottom, the player hand should be displayed. The cards can be stacked a bit to save space but there should be more than half of the card visible at all time. When a card is clicked, it should be played (given that it is the players turn). If the card is not eligible to play there should be a small popup that vanishes after short time that displays the error (why it cannot be played). We already have the legal cards to play, so maybe we can somehow make the illegal ones a bit darker to indicate the unplayability.

### Player Display

Each of the other three players should be indicated by a label. Later, we can think about displaying their hands or similar things, but for now, their "name" suffices. Also, their announcements should be displayed below their labels. The labels should be positioned on the left, top and right of the screen respectively.

### Trick Display

In the center of the screen the current trick should be displayed. Each of the cards played by each player should be oriented such that it is easy to see, who played them (using the positions of the players' labels). When a trick is full, there should be a short animation, which card wins the trick (maybe a little wiggle of the card). Then, the cards get removed when the new trick begins.

### Reservations

At the start of each round, in order the players should be displayed their options for reservations. Once the first player has decided on reservations, the second player gets to announce their reservation and so on...

The game mode should be displayed at the top left of the screen with the current trick number.

### Announcements

The players should not be asked everytime, if they want to announce something. Instead, there should be a small button in the center left part of the screen that when pressed, fires the next announcement. This could cause problems, because I think the backend / application expects a yes or no each turn where announcements are possible. We need to research that and if so, always send a "no announcements" with a card played, if the button was not pressed.

### Card Display

I have put .svgs for each card in /home/philipp/programming/doko/claude_website/resources/cards. They are named by the schema "<suit><rank>" where suit is (kr, p, h, k) and rank is (9, 10, A, B, D, K) (using german names kreuz, pik, herz, karo and Bube Dame König Ass). This is what we will use for display for now.