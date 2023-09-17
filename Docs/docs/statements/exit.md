---
title: Exit
layout: default
parent: Statements
last_modified_date: 2023-09-16 16:58
---

# Exit and exit status

The `exit` statement is used to terminate the program and classify the input.

## Exit

{: .example }
> Reject the input if the first pixel is white:
>
> ```btml
> if White exit 1
> ```

### Exit - Definition and Usage

The `exit` function terminates the program.

### Exit - Syntax

```ebnf
exit [ status ]
```

### Exit - Fields

Field | Description
-- | --
status | Optional. A status number used as [exit status](#exit-status), default 0.

### Exit - More Examples

{: .example }
> Else is not treated as an exit status:
>
> ```btml
> if Black exit else move Right
> if White exit 1
> ```

## Exit Status

The exit status classifies the input, see the list below:

### Exit Status - List

Exit Status | Description
-- | --
0 | The input is accepted (the program finished).
1 | The input is rejected.
2 | The input is malformed (the program moved off the canvas).