---
layout: default
parent: Statements
title: If While Else
---

# If, While, Else and Actions
{: .no_toc }

Conditional statements are used to perform different actions for white and black pixels.
{: .fs-6 .fw-300 }

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

## If

{: .example }
> Move right if the first pixel is black:
>
> ```btml
> if black right
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
{: .d-inline-block }

New (v0.4.0)
{: .label .label-green }

{: .example }
> Emphasize binary:
>
> ```btml
> if 1 right
> ```

## While
{: .d-inline-block }

New (v0.4.0)
{: .label .label-green }

{: .example }
> Move right until the first white pixel:
>
> ```btml
> while black right
> ```

### While - Definition and Usage

Use the `while` statement to specify a single action to be performed as long as the read pixel has the specified color.

### While - Syntax

```ebnf
while color action
```

### While - Fields

Field | Description
-- | --
while| Required. `while`.
color | Required. `0`/`white` or `1`/`black`.
action | Required. An [action](#actions).

### While - More Examples

{: .example }
> Emphasize binary:
>
> ```btml
> while 1 right
> ```

## Else

{: .example }
> Move right if the first pixel is black else reject the input:
>
> ```btml
> if black right else exit 1
> ```

### Else - Definition and Usage

Use the `else` statement to specify a single action to be performed if the read pixel does not have the specified color.

### Else - Syntax

```ebnf
(if | while) else action
```

### Else - Fields

Field | Description
-- | --
if or while | Required. An [if](#if)- or [while statement](#while).
else | Required. `else`.
action | Required. An [action](#actions).

### Else - More Examples
{: .d-inline-block }

New (v0.4.0)
{: .label .label-green }

{: .example }
> Move right until the last black pixel:
>
> ```btml
> while black right else left
> ```

## Actions

{: .example }
> Write a black pixel and move right:
>
> ```btml
> write black right
> ```

### Actions - Syntax

```ebnf
[write] [direction] [exit | goto]
```

### Actions - Fields

Field | Description
-- | --
write | Optional. A [write statement](write), default write read color.
direction | Optional. `up`, `down`, `left` or `right`, default precompute action.
exit or goto | Optional. An [exit](exit#exit)- or [goto statement](goto#goto), default go to next line.

{: .note }
> An action consists out of at least one statement.