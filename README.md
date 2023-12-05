# Development Technical Scenario

My first impressions of this problem were that it was relatively simple, although upon further reading I found the phrasing of the specification to be a somewhat confusing.
To this end, I first went after the low hanging fruit - making sure the compiler can generalize types, validation option types, formatting, reusable components - as
I think these should be a given anyway.

I picked F# rather than C# (despite being stronger in C#) for two reasons. Firstly, as discussed in the interview, Illy is migrating to F# from C#.
Secondly, I've been practicing (read as: learning from scratch) in the past few days, and wanted to challenge myself.

I interpreted the specification to mean that the consumer of the library was implementing the validations on their custom types themselves, with parts from the library, hence
why the solution is split into two components - `Library` and `Program`. The library includes a number of pre-written validation components which are as general as possible, except
for the max length component where I failed to find a `Countable`, `Sized`, `Length`, etc. interface (I don't know if F# has something analogous to C++ overloading, but if so I
haven't come across it yet), and the date time ones where it's likely going to be a DateTime passed in regardless.

The consumer validation function is implemented by using the pipe operator (`|>`) on the validation functions, chaining them together, and using Result.bind to lazily evaluate only
if the previous tests return Ok. This could also possibly be implemented using the composition operator (`>>`), although that's a bit above my skill level for the time being.

I think I may have missed the mark a bit with the brief as I feel like the problem would have better suited a system leveraging dependency injection, such that you could
add the name or dob component at runtime then pass literally everything into the same validator function, using the dependency injection system to only run the validators on
types that exist within the type passed into the validator. That said, a dependency injection system would take a fair bit longer to implement.
Additionally, I haven't implemented a validator for a different example type such as `Appointment`, `Prescription`, etc. as it would have taken me over 2 hours. 

As a last quick note, I originally intended to use MSTest for testing the solution but adding the packages threw up a strange error about the entry point not being the final
translation unit in the file. As much as I could probably have fixed it, I decided it was quicker to just write a console application instead.