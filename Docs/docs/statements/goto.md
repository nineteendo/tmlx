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
> 	if black move south else move south goto skip
> 	write Black exit
> skip:	write White
> ```

## Goto - Definition and Usage

The `goto` function jumps to the specified label.

## Goto - Syntax

```ebnf
goto label
```

## Goto - Fields

Field | Description
-- | --
label | Required. A defined [label](#labels).

## Goto - More Examples

{: .example }
> Jump to previous line:
>
> ```btml
> back:	if white exit 1 else move east
> 	if white exit else move east goto back
> ```

## Repeat

{: .example }
> Move east until the first white pixel:
>
> ```btml
> if black move east repeat
> ```

## Repeat - Definition and Usage

The `repeat` function repeats the current line.

### Repeat - Syntax

```ebnf
repeat
```

## Labels

{: .example }
> Explain a line:
>
> ```btml
> find_white:	if black move east repeat
> ```

## Labels - Definition and Usage

A label allows you to jump to a line.

### Labels - Syntax

```ebnf
label:
```

## Labels - Fields

Field | Description
-- | --
label | Required. A unique label.