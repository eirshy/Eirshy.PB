# PRESS X TO JSON
**Press X to JSON** is a Toolkit Mod that adds an advanced
 config editing language to Phantom Brigade. This mod changes
 nothing about the game itself- it merely provides a more
 powerful language for mod authors to use.

### Installation - BETA AVAILBALBE
Simply snag the latest in the 00-RELEASE folder, and extract it and
 enable it like any other mod.
 As we have to hook a number of mod-loading actions, you should
 place it ***first*** in your mod load order.


## I'm a Mod Author and What's This
As you might guess, this adds a JSON-based version of
 ConfigEdits, using JPath to select what you're editing and
 utilizing a number of additional features one might want for
 ease of development or compatibility.

### Documentation Where!
Mostly my head, but I've moved some of it to markdown files in the
 `00-DOCS` directory. I'll (probably) eventually move it to the
 snazzy "wiki" feature that Github provides, but for now that's all
 there is.

## What are we waiting on? (pre-1.0)
* Convert all my current "could be published" to be examples
* Basic reference docs for Instruction/InstructionFile Schemas

## What the Future Holds? (post-1.0 or earlier)
* "Less-Lazy Loading" mode
  * When init finishes, confirm all that we touch loaded
  * Guarantees syntax errors are found immediately
  * Can clean up all of our data right after as we done
* Real reference docs
* Proper schema definitions for our internals
* Schema "generator" for game classes.
  * Or at least a few manual ones, like for Subsystems
* Built-In Instruction Macros
  * "New Mech" for sure
* External Instruction Macros
