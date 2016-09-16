# Contributing

The original code was taken from [here](https://groups.google.com/forum/#!searchin/getseq/enhance%2420exception/getseq/rsAL4u3JpLM/PrszbPbtEb0J) and then improved for better performance and to support more exception types without using reflection by [Muhammad Rehan Saeed](http://rehansaeed.com).

If you have created any custom destructurers, please contribute them even if they are for third party libraries. If they are for exceptions from a third party library e.g. EntityFramework, please contribute a Serilog.Exceptions.EntityFramework project containing all the destructurers for the all the exceptions in EntityFramework.

## Coding Guidelines

Serilog.Exceptions uses StyleCop to produce style warnings. Please fix all warnings in any code you submit. Also, please attempt to write unit tests for any code written.