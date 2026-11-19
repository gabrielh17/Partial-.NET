# DECISIONS

Answer **yourself**, in your own words, in 3–5 sentences per question.
Refer to concrete files and classes from your own repo (for example `Features/Bookings/CreateBooking/...`).
An unanswered question gives 0 points.

## 1. A rule you moved
Pick one business rule (R1–R8). In which file did it live in the starter and in which file does it live now? Why there?

_R1 was in Services/RoomService.cs, now it is in Features/Rooms/CreateRoom/CreateRoomEndpoint.cs_

## 2. Result instead of an exception
Why is `Result` a better choice than an exception for "the room is occupied"? What happens in your code when an `Error` is returned?

_exceptions are made for things that shouldn't happen, result is for business failures (they are not a crash)_

## 3. Who knows about EF Core?
Which classes in your project know about EF Core and which do not? What do you gain and what do you pay with Repository + Unit of Work?

_everything from infrastructure_

## 4. The flow of a request
Draw in text (ASCII arrows) the path of a `POST /api/rooms/{roomId}/bookings` request through your project, with the **real** names of files and classes, from the endpoint to the database.

_(your answer here)_

## 5. What you rejected from what the AI gave you
Give a concrete example of code or an idea proposed by the AI that you corrected or rejected, and say why.

_(your answer here)_

## 6. If you had 30 more minutes
What would you improve and what risk did you leave in the project?

_better time management, creating skills_
