---
title: Exit
layout: default
parent: Statements
---

# Exit and Exit Status
{: .no_toc }

The `exit` statement is used to terminate the program and classify the input.
{: .fs-6 .fw-300 }

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

## Exit

{: .example }
> Reject the input if the first pixel is white:
>
> ```btml
> if white exit 1
> ```

### Exit - Definition and Usage

The `exit` function terminates the program.

### Exit - Syntax

```ebnf
exit [status]
```

### Exit - Fields

Field | Description
-- | --
status | Optional. An [exit status](#exit-status), default 0.

### Exit - More Examples

{: .example }
> Else is not treated as an exit status:
>
> ```btml
> if black exit else move east
> if white exit 1
> ```

## Exit Status

The exit status classifies the input, see the list below:

### Exit Status - List

Exit Status | Description
-- | --
0 | The input is accepted (the program finished).
1 | The input is rejected.
2 | The input is malformed (the program moved off the canvas).