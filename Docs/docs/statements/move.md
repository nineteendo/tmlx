---
title: Move
layout: default
parent: Statements
last_modified_date: 2023-09-16 15:38
---

# Move

{: .example }
> Move east until the first white pixel:
>
> ```btml
> if black move east repeat
> ```

## Move - Definition and Usage

The `move` function moves the program.

## Move - Syntax

```ebnf
move direction
```

## Move - Fields

Field | Description
-- | --
direction | Required. `N`/`north`, `E`/`east`, `S`/`south`, `W`/`west`.

## Move - More Examples

{: .example }
> Abbreviated:
>
> ```btml
> if black move E repeat
> ```