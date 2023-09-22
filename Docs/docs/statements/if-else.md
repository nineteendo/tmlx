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
> Move east until the first white pixel:
>
> ```btml
> if black move east repeat
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
color | Required. `0`/`white` or `1`/`black`.
action | Required. An [action](#actions).

### If - More Examples

{: .example }
> Emphasize binary:
>
> ```btml
> if 1 move east repeat
> ```

## Else

{: .example }
> Move east until the last white pixel:
>
> ```btml
> if white move east repeat else move west
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
color | Required. `0`/`white` or `1`/`black`.
action | Required. An [action](#actions).

## Actions

{: .example }
> Write a black pixel and move east:
>
> ```btml
> write Black move east
> ```

### Actions - Syntax

```ebnf
[write] [move] [exit | goto | repeat]
```

### Actions - Fields

Field | Description
-- | --
write | Optional. A [write](write) statement, default write read color.
move | Optional. A [move](move) statement, default precompute action.
exit, goto or repeat | Optional. An [exit](exit#exit), [goto](goto#goto) or [repeat](goto#repeat) statement, default go to next line.

{: .note }
> An action consists out of at least one statement.