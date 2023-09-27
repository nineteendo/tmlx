---
title: If Else
layout: default
parent: Statements
---

# If, Else and Actions
{: .no_toc }

Conditional statements are used to perform different actions for white and black pixels.
{: .fs-6 .fw-300 }

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

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
if else action
```

### Else - Fields

Field | Description
-- | --
if | Required. An [if statement](#if).
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
write | Optional. A [write statement](write), default write read color.
move | Optional. A [move statement](move), default precompute action.
exit, goto or repeat | Optional. An [exit](exit#exit), [goto](goto#goto) or [repeat statement](goto#repeat), default go to next line.

{: .note }
> An action consists out of at least one statement.