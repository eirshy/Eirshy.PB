# PRESS X TO JSON
**Press X to JSON** is a Toolkit Mod that adds an advanced
 config editing language to Phantom Brigade. This mod changes
 nothing about the game itself- it merely provides a more
 powerful language for mod authors to use.

### Installation - NOT ACTUALLY READY YET
Once it is:

Simply download the "Release.zip", and extract it in the standard
 mod folder. Then, enable it through the in-game
 menu. As we have to hook a number of mod-loading actions, you should
 place it ***first*** in your mod load order.


## I'm a Mod Author and What's This
As you might guess, this adds a JSON-based version of
 ConfigEdits, using JPath to select what you're editing and
 utilizing a number of additional features one might want for
 ease of development or compatibility.

## What are we waiting on?
* Log buffers need a flush point still lol
* "Semi-Lazy Loading" mode
  * Detect when init load is done, confirm all that we touch loaded
  * Gives us something to hook for our first dump of logs
  * Guarantees syntax errors are found immediately
  * Can clean up all of our data right after as nothing can change
* Convert all my current "could be published" for examples
* Basic reference docs for Commands
* Basic reference docs for Instruction/InstructionFile Schemas

## What the Future Holds?
* Real reference docs
* Proper schema definitions for our internals
* Schema "generator" for game classes.
* More PLINQ usage
* Built-In Instruction Macros
  * "New Mech" for sure
* External Instruction Macros
