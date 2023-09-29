---
layout: default
parent: Statements
title: Goto
---

# Goto and Labels
{: .no_toc }

Goto and labels are used to jump through the code.
{: .fs-6 .fw-300 }

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

## Goto

{: .example }
> Skip the next line:
>
> ```btml
> 	if black down else down goto skip
> 	write black exit
> skip:	write white
> ```

### Goto - Definition and Usage

The `goto` function jumps to the specified label.

### Goto - Syntax

```ebnf
goto label
```

### Goto - Fields

Field | Description
-- | --
goto | Required. `goto`.
label | Required. A defined [label](#labels).

### Goto - More Examples

{: .example }
> Jump to the previous line:
>
> ```btml
> back:	if white exit 1 else right
> 	if white exit else right goto back
> ```

## Labels

{: .example }
> Explain a line:
>
> ```btml
> find_white:	while black right
> ```

### Labels - Definition and Usage

A label allows you to jump to a line.

### Labels - Syntax

```ebnf
label:
```

### Labels - Fields

Field | Description
-- | --
label | Required. A unique label.