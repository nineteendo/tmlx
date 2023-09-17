---
title: If Else
layout: default
parent: Statements
last_modified_date: 2023-09-16 15:30
---

# If, else and actions

Conditional statements are used to perform different actions for white and black pixels.

## If

{: .example }
> Move right until the first white pixel:
>
> ```btml
> if Black move Right repeat
> ```

### If - Definition and Usage

Use the `if` statement to specify a single action to be performed if the read pixel has the specified color.

### If - Syntax

```ebnf
if color action
```

### If - Fields

Field | Description
-- | --
color | Required. `Black` or `White`.
action | Required. An [action](#actions).

## Else

{: .example }
> Move right until the last white pixel:
> 
> ```btml
> if White move Right repeat else move Left
> ```

### Else - Definition and Usage

Use the `else` statement to specify a single action to be performed if the read pixel does not have the specified color.

### Else - Syntax

```ebnf
if color action else action
```

### Else - Fields

Field | Description
-- | --
color | Required. `Black` or `White`.
action | Required. An [action](#actions).

## Actions

{: .example }
> Write a black pixel and move Right:
> 
> ```btml
> write Black move Right
> ```

### Actions - Definition and Usage

An action consists out of at least one statement.

### Actions - Syntax

```ebnf
[write] [move] [exit | goto | repeat]
```

### Actions - Fields

Field | Description
-- | --
write | Optional. A [write statement](write), default write read color.
move | Optional. A [move statement](move), default precompute action.
exit | Optional. An [exit statement](exit).
goto | Optional. A [goto statement](goto#goto), default go to next line.
repeat | Optional. A [repeat statement](goto#repeat).