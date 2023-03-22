# Comands

Every Instruction has a Command that describes what it does. \
Commands have their own Priority (considered before instruction, file, or even mod order). They also have an expected Data and Target type. 

#### Command Priorities
||Name|Desc
|-:|:-:|:-
|0|**`Meta`**|For things that affect other instructions
|1|**`RootCreate`**|Independent new file creation
|2|**`RootFrom`**|Dependent new file creation (may chain)
|3|**`General`**|Any other action


#### Command Data and Target Types
|Group|Type|Example|
|:-:|:-:|:-|
|Node|**Array**| `[ "value0", "value1", "value2", ... ]`
|Node|**Object**| `{ "key": "value", "key1": "value1", ... }`
|Value|**Number**| `8` or `2.37`
|Value|**String**| `"some text"` or `'some other text'`
|Value|**Boolean**| `true` or `false`
|Value|**Null**| `null`

\- An `Object`'s keys are unique. \
\- `Array` and `Object` values can be any other type. \
\- `Number` is generally rounded to the nearest value for integers. \
\- `Null` is context-dependent. It may mean "nothing" or "default".

#### `Object` Special Keys
|Key|Usage|
|-|:-|
|`$type`| Directly specifies the underlying type of an object. <br />**Format**: `"Namespace.TypeName, Assembly"` <br /> **Ex:** `"PhantomBrigade.Functions.Equipment.AddHardpoints, Assembly-CSharp"`
|`@TYPE`| "Auto-Types" the underlying type of an object, using the context to guess both the namespace and the assembly if they aren't provided. <br />**Format**: `"Namespace.TypeName, Assembly"` <br /> **Ex:** `"AddHardpoints"`

## Standard Commands
<style type="text/css">
#standard-commands + style + table tbody
tr:is(:nth-child(1),:nth-child(3),:nth-child(5))
{
    vertical-align: top;
}
#standard-commands + style + table tbody
tr:is(:nth-child(1),:nth-child(3),:nth-child(5))
td{
    padding: 0 0;
    font-size: 18px;
    border-left: unset;
    border-right: unset;
    
}
#standard-commands + style + table tbody
tr:is(:nth-child(1),:nth-child(3),:nth-child(5))
mark{
    display: block;
}
#standard-commands + style + table thead,
#standard-commands + style + table tbody tr:last-child{
    font-size: 13px;
    font-style: italic;
    font-weight: 600;
}
#standard-commands + style + table tbody tr:last-child td:first-child{
    text-align: center;
}
</style>
|Command|Data|Target|Description|
|-:|:-:|:-:|:-|
|<mark>RootCreate</mark>|<mark>&nbsp;</mark>|<mark>&nbsp;</mark>|<mark>&nbsp;</mark>
|`New`|`Object`|-|Creates a new game object
|<mark>RootFrom</mark>|<mark>&nbsp;</mark>|<mark>&nbsp;</mark>|<mark>&nbsp;</mark>
|`CopyBase`|`String`<br />As&nbsp;Name|-|Attempts to copy the game object of the file type with the given name
|<mark>General</mark>|<mark>&nbsp;</mark>|<mark>&nbsp;</mark>|<mark>&nbsp;</mark>
|`Overwrite`|`Object`|-|As `New`, but lower priority
|`Copy`|`String`<br />As&nbsp;Name|-|As `CopyBase`, but lower priority
|`Remove`|-|Any|Removes the target property
|`Replace`|Same|Same|Replaces the target property
|`Concat`|`Array`|`Array`|Adds all values in data to the end of the target array
|`ConcatNew`|`Array`|`Array`|As `Concat`, but only adds missing values
|`SpreadPre`|`Array`|in&nbsp;`Array`|Inserts all values in data just before the target element(s)
|`SpreadPost`|`Array`|in&nbsp;`Array`|Inserts all values in data just after the target element(s)
|`Merge`|`Object`|`Object`|Recursively merges all properties, treating any `Array`s encountered the same as `Concat`
|`MergePatch`|`Object`|`Object`|Recursively merges all properties, treating any `Array`s encountered the same as `ConcatNew`
|`MergeHard`|`Object`|`Object`|Recursively merges all properties, treating any `Array`s encountered the same as `Replace`
|Command|Data|Target|Description|

