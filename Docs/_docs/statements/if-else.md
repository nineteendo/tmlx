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
> Move right until the first white pixel:
>
> ```btml
> if black right repeat
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
if | Required. `if`.
color | Required. `0`/`white` or `1`/`black`.
action | Required. An [action](#actions).

### If - More Examples

{: .example }
> Emphasize binary:
>
> ```btml
> if 1 right repeat
> ```

## Else

{: .example }
> Move right until the last white pixel:
>
> ```btml
> if white right repeat else left
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
else | Required. `else`.
action | Required. An [action](#actions).

## Actions

{: .example }
> Write a black pixel and move right:
>
> ```btml
> write black right
> ```

### Actions - Syntax

```ebnf
[write] [direction] [exit | goto | repeat]
```

### Actions - Fields

Field | Description
-- | --
write | Optional. A [write statement](write), default write read color.
direction | Optional. `nowhere`, `up`, `down`, `left` or `right`, default precompute action.
exit, goto or repeat | Optional. An [exit](exit#exit)-, [goto](goto#goto)- or [repeat statement](goto#repeat), default go to next line.

{: .note }
> An action consists out of at least one statement.