Brand Allred
8/4/2019

This project represents my attempt at the message design pattern. The factory pattern is also utilized in this project.

Sending, recieving, and handling messages are all handled in the SpaceShip Abstract class.

The Board handles where the ships are, and what ships there are. And the ships knowing who eachother are.

I used DTO objects to serialize and send messages between threads. This allows for a simple, agreed upon way for each SpaceShip concrete class to interact with eachother.
The DTOs also allow for new types of ships to be made, and even ships that are not even a part of the same program. Or even written in the same language. This is because 
the DTOs are serialized into JSON.

*******************IMPORTANT**************************
Package info:
	Newtonsoft.json version 12.0.2
	C# version 4.6.1