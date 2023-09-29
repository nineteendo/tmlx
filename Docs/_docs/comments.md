---
layout: default
title: Comments
---

# Comments
{: .no_toc }

Comments can be used to explain code, and to make it more readable.
{: .fs-6 .fw-300 }

## Table of contents
{: .no_toc .text-delta }

1. TOC
{:toc}

## Single Line Comments

{: .example }
> ```btml
> if white exit 1 // Reject the input if the first pixel is white
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
> if white exit 1
> ```

## Multi-line Comments

{: .example }
> ```btml
> /*
> Reject the input if the first pixel is white
> */
> if white exit 1
> ```

### Multi-line Comments - Syntax

```ebnf
/* {text \v} text */
```

{: .note }
> You can also add documentation to your program by using `/**`:
>
> ```btml
> /**
> Reject the input if the first pixel is white
> */
> if white exit 1
> ```