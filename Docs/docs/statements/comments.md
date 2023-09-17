---
title: Comments
layout: default
last_modified_date: 2023-09-16 17:26
---

# Comments

Comments can be used to explain code, and to make it more readable.

## Single Line Comments

{: .example }
> ```btml
> if White exit 1 // Reject the input if the first pixel is white
> ```

### Single Line Comments - Syntax

```ebnf
// text [\n | \v]
```

{: .note }
> You can also add documentation to your program by using `///`:
>
> ```btml
> /// Reject the input if the first pixel is white
> if White exit 1
> ```

## Multi-line Comments

{: .example }
> ```btml
> /*
> Reject the input if the first pixel is white
> */
> if White exit 1
> ```

### Multi-line Comments - Syntax

```ebnf
/* {text \n} text */
```

{: .note }
> You can also add documentation to your program by using `/**`:
>
> ```btml
> /**
> Reject the input if the first pixel is white
> */
> if White exit 1
> ```