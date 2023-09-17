---
title: Goto
layout: default
parent: Statements
last_modified_date: 2023-09-16 15:38
---

# Goto, Repeat and Labels

Goto, repeat and labels are used to jump through the code.

## Goto

{: .example }
> Skip next line:
>
> ```btml
> if Black move Down else move Down goto skip
> write Black exit
> label skip write White
> ```

## Goto - Definition and Usage

The `goto` function goes to the specified label.

## Goto - Syntax

```ebnf
goto label
```

## Goto - Fields

Field | Description
-- | --
label | Required. A defined [label](#label).

## Goto - More Examples

{: .example }
> Go to previous line:
>
> ```btml
> label back if White exit 1 else move Right
> if White exit else move Right goto back
> ```

## Repeat

{: .example }
> Move right until the first white pixel:
>
> ```btml
> if Black move Right repeat
> ```

## Repeat - Definition and Usage

The `repeat` function repeats the current line.

### Repeat - Syntax

```ebnf
repeat
```

## Label

{: .example }
> Explain a line:
>
> ```btml
> label find_white if Black move Right repeat
> ```

## Label - Definition and Usage

The `label` statement gives a line you can jump to.

### Label - Syntax

```ebnf
label label instruction
```

## Label - Fields

Field | Description
-- | --
label | Required. A unique label.
instruction | Required. An [action](if-else#actions) or [if statement](if-else).