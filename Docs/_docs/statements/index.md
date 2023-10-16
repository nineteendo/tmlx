---
has_children: true
layout: default
permalink: /docs/statements/
title: Statements
---

# Statements

{: .example }
> Write a black pixel and move right:
>
> ```btml
> write black right
> ```

## Statements - Overview

Here is a visual representation of all possible statements. Click on a word for more information.

<style type="text/css">
    table {
        border-collapse: collapse;
        min-width: 100%;
    }

    td {
        font-size: 0.75rem !important;
        min-width: 3.18rem;
        padding: 0.159rem 0.318rem;
        text-align: center;
    }

    tbody tr:last-of-type td {
        padding-bottom: 0;
    }

    .td-0 {
        background-color: #ffffff
    }

    .td-1 {
        background-color: #fafafa
    }

    .td-2 {
        background-color: #f5f5f5
    }

    .td-3 {
        background-color: #f0f0f0
    }

    .td-4 {
        background-color: #ebebeb
    }

    .td-5 {
        background-color: #e6e6e6
    }

    .td-6 {
        background-color: #e1e1e1
    }

    .td-7 {
        background-color: #dcdcdc
    }

    .td-8 {
        background-color: #d7d7d7
    }

    .td-9 {
        background-color: #d2d2d2
    }
</style>
<table>
    <tbody>
        <tr>
            <td class="td-2"><a href="goto#labels">label:</a></td>
            <td class="td-2" rowspan=10><a href="if-while-else#if">if</a>/<a href="if-while-else#while">while</a></td>
            <td class="td-4">0/white</td>
            <td class="td-5" rowspan=8><a href="write">write</a></td>
            <td class="td-6">0/white</td>
            <td class="td-5">up</td>
            <td class="td-5" rowspan=2><a href="exit#exit">exit</a></td>
            <td class="td-6"><a href="exit#exit-status">status</a></td>
            <td class="td-5" rowspan=9><a href="if-while-else#else">else</a></td>
            <td class="td-8" rowspan=8><a href="write">write</a></td>
            <td class="td-9">0/white</td>
            <td class="td-8">up</td>
            <td class="td-8" rowspan=2><a href="exit#exit">exit</a></td>
            <td class="td-9"><a href="exit#exit-status">status</a></td>
        </tr>
        <tr>
            <td class="td-1">/</td>
            <td class="td-4">1/color</td>
            <td class="td-6">1/black</td>
            <td class="td-5">down</td>
            <td class="td-6">/</td>
            <td class="td-9">1/black</td>
            <td class="td-8">down</td>
            <td class="td-9">/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-6">blue</td>
            <td class="td-5">left</td>
            <td class="td-5"><a href="goto#goto">goto</a></td>
            <td class="td-6"><a href="goto#labels">label</a></td>
            <td class="td-9">blue</td>
            <td class="td-8">left</td>
            <td class="td-8"><a href="goto#goto">goto</a></td>
            <td class="td-9"><a href="goto#labels">label</a></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-6">green</td>
            <td class="td-5">right</td>
            <td class="td-4" colspan=2>/</td>
            <td class="td-9">green</td>
            <td class="td-8">right</td>
            <td class="td-7" colspan=2>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-6">cyan</td>
            <td class="td-4">/</td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-9">cyan</td>
            <td class="td-7">/</td>
            <td class="td-6"></td>
            <td class="td-6"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-6">red</td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-9">red</td>
            <td class="td-6"></td>
            <td class="td-6"></td>
            <td class="td-6"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-6">magenta</td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-9">magenta</td>
            <td class="td-6"></td>
            <td class="td-6"></td>
            <td class="td-6"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-6">yellow</td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-9">yellow</td>
            <td class="td-6"></td>
            <td class="td-6"></td>
            <td class="td-6"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-4" colspan=2>/</td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-7" colspan=2>/</td>
            <td class="td-6"></td>
            <td class="td-6"></td>
            <td class="td-6"></td>
        </tr>
         <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-4" colspan=6>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-1" rowspan=9><a href="if-while-else#actions">/</a></td>
            <td class="td-3" rowspan=9>/</td>
            <td class="td-4" rowspan=8><a href="write">write</a></td>
            <td class="td-5">0/white</td>
            <td class="td-4">up</td>
            <td class="td-4" rowspan=2><a href="exit#exit">exit</a></td>
            <td class="td-5"><a href="exit#exit-status">status</a></td>
            <td class="td-3" colspan=6 rowspan=9>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">1/black</td>
            <td class="td-4">down</td>
            <td class="td-5">/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">blue</td>
            <td class="td-4">left</td>
            <td class="td-4"><a href="goto#goto">goto</a></td>
            <td class="td-5"><a href="goto#labels">label</a></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">cyan</td>
            <td class="td-4">right</td>
            <td class="td-3" colspan=2>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">green</td>
            <td class="td-3">/</td>
            <td class="td-2"></td>
            <td class="td-2"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">red</td>
            <td class="td-2"></td>
            <td class="td-2"></td>
            <td class="td-2"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">magenta</td>
            <td class="td-2"></td>
            <td class="td-2"></td>
            <td class="td-2"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">yellow</td>
            <td class="td-2"></td>
            <td class="td-2"></td>
            <td class="td-2"></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3" colspan=2>/</td>
            <td class="td-2"></td>
            <td class="td-2"></td>
            <td class="td-2"></td>
        </tr>
    </tbody>
</table>

## Statements - More Examples

{: .example }
> Statements can be conditional:
>
> ```btml
> if color right else exit 1
> ```