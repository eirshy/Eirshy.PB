# PRESS X TO JSON
**Press X to JSON** is a Toolkit Mod that adds an advanced
 config editing language to Phantom Brigade. This mod changes
 nothing about the game itself- it merely provides a more
 powerful language for mod authors to use.

### Installation - BETA AVAILBALBE -- v0.5a
Simply snag the latest in the 00-RELEASE folder, and extract it and
 enable it like any other mod.
 As we have to hook a number of mod-loading actions, you should
 place it ***first*** in your mod load order.


## I'm a Mod Author and What's This
As you might guess, this adds a JSON-based version of
 ConfigEdits, using JPath to select what you're editing and
 utilizing a number of additional features one might want for
 ease of development or compatibility.

## What are we waiting on?
* Convert all my current "could be published" for examples
* Basic reference docs for Instruction/InstructionFile Schemas

## What the Future Holds?
* "Less-Lazy Loading" mode
  * When init finishes, confirm all that we touch loaded
  * Guarantees syntax errors are found immediately
  * Can clean up all of our data right after as we done
* Partial ownership of types for logging
  * InfoVerbose dumping deserialized round-trips is useful
  * With even just 2 json mods, not so much.
  * Should try to detect which mods modified a given file
    and log under that mod if it's only that one.
  * Maybe also make Orphan look at all levels that modified
    a file and take the most permissive if there's more than
    one?
* Real reference docs
* Proper schema definitions for our internals
* Schema "generator" for game classes.
* More PLINQ usage
* Built-In Instruction Macros
  * "New Mech" for sure
* External Instruction Macros
