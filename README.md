# Enhanced ScriptableObject GameEvent System

A ScriptableObject based GameEvent system for Unity.


## Features
* Serialized delegates. Allowing for a method to have any number of parameters.
* Call stacktraces for each GameEvent to show where it was raised from in code, for easy debugging, and editing.

## Why use this system?
This system has been designed to allow for easy debugging. To this end, in each GameEvent's Inspector you can see a stack trace of each time the event was raised, what line it was raised from, and selecting one will take you to the line in code.
To allow for flexability, the system can be fully used from code, and doesn't require use of the editor. This is thanks to teh use of Delegates, aposed to UnityEvents. This allows it to be used for procedural content.

## Requirements
* Odin Serializer

