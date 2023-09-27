---
title: Statements
layout: default
has_children: true
permalink: /docs/statements
---

# Statements

<span>New (v0.4.0)</span>{: .label .label-green } Here is a visual representation of all possible statements. Click on a word for more information.

<style type="text/css">
    table {
        border-collapse: collapse;
    }

    td {
        font-size: 0.7rem !important;
        min-width: 2.5rem;
        padding: 0.25rem 0.375rem;
        text-align: center;
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
            <td class="td-2" rowspan=6><a href="if-else#if">if</a></td>
            <td class="td-4">0/white</td>
            <td class="td-5" rowspan=2><a href="write">write</a></td>
            <td class="td-6">0/white</td>
            <td class="td-5" rowspan=4><a href="move">move</a></td>
            <td class="td-6">N/north</td>
            <td class="td-5" rowspan=2><a href="exit/exit">exit</a></td>
            <td class="td-6"><a href="exit#exit-status">status</a></td>
            <td class="td-5" rowspan=5><a href="if-else#else">else</a></td>
            <td class="td-8" rowspan=2><a href="write">write</a></td>
            <td class="td-9">0/white</td>
            <td class="td-8" rowspan=4><a href="move">move</a></td>
            <td class="td-9">N/north</td>
            <td class="td-8" rowspan=2><a href="exit/exit">exit</a></td>
            <td class="td-9"><a href="exit#exit-status">status</a></td>
        </tr>
        <tr>
            <td class="td-1">/</td>
            <td class="td-4">1/black</td>
            <td class="td-6">1/black</td>
            <td class="td-6">E/east</td>
            <td class="td-6">/</td>
            <td class="td-9">1/black</td>
            <td class="td-9">E/east</td>
            <td class="td-9">/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-4" colspan=2>/</td>
            <td class="td-6">S/south</td>
            <td class="td-5"><a href="goto#goto">goto</a></td>
            <td class="td-6"><a href="goto#labels">label</a></td>
            <td class="td-7" colspan=2>/</td>
            <td class="td-9">S/south</td>
            <td class="td-8"><a href="goto#goto">goto</a></td>
            <td class="td-9"><a href="goto#labels">label</a></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-6">W/west</td>
            <td class="td-5" colspan=2><a href="goto#repeat">repeat</a></td>
            <td class="td-6"></td>
            <td class="td-6"></td>
            <td class="td-9">W/west</td>
            <td class="td-8" colspan=2><a href="goto#repeat">repeat</a></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-4" colspan=2>/</td>
            <td class="td-4" colspan=2>/</td>
            <td class="td-6"></td>
            <td class="td-6"></td>
            <td class="td-7" colspan=2>/</td>
            <td class="td-7" colspan=2>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-3"></td>
            <td class="td-4" colspan=7>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-1" rowspan=5><a href="if-else#actions">/</a></td>
            <td class="td-3" rowspan=5>/</td>
            <td class="td-4" rowspan=2><a href="write">write</a></td>
            <td class="td-5">0/white</td>
            <td class="td-4" rowspan=4><a href="move">move</a></td>
            <td class="td-5">N/north</td>
            <td class="td-4" rowspan=2><a href="exit/exit">exit</a></td>
            <td class="td-5"><a href="exit#exit-status">status</a></td>
            <td class="td-3" colspan=7 rowspan=5>/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-5">1/black</td>
            <td class="td-5">E/east</td>
            <td class="td-5">/</td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-3" colspan=2>/</td>
            <td class="td-5">S/south</td>
            <td class="td-4"><a href="goto#goto">goto</a></td>
            <td class="td-5"><a href="goto#labels">label</a></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-2"></td>
            <td class="td-2"></td>
            <td class="td-5">W/west</td>
            <td class="td-4" colspan=2><a href="goto#repeat">repeat</a></td>
        </tr>
        <tr>
            <td class="td-0"></td>
            <td class="td-2"></td>
            <td class="td-2"></td>
            <td class="td-3" colspan=2>/</td>
            <td class="td-3" colspan=2>/</td>
        </tr>
    </tbody>
</table>