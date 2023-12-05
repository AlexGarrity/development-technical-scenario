namespace IllyValidationTest

open System

module Library =
    type ValidationFunction<'A, 'B, 'Err> = 'A -> Result<'B, 'Err>

    /// <summary>
    /// The max (exclusive) length of a value being checked by `MaxLength`
    /// </summary>
    [<Literal>]
    let MAX_LENGTH = 32

    /// <summary>Validates whether a value has been entered or not</summary>
    /// <returns>true if there is an input</returns>
    let MustBeEntered (i: 'A option) : bool =
        match i with
        | Some _ -> false
        | None -> true

    /// <summary>   
    /// Validates if a list-type object is over a max length
    /// </summary>
    /// <returns>true if the input is greater than length 32</returns>
    let MaxLength (i: string option) : bool =
        match i with
        | None -> false
        | Some v ->
            v.Length > MAX_LENGTH
            
    let MaxLength (i: 'A list option) : bool =
        match i with
        | None -> false
        | Some v ->
            v.Length > MAX_LENGTH

    /// <summary>
    /// Validates if a DateTime is in the future
    /// </summary>
    /// <returns>true if the input is in the future (as of the time of calling)</returns>
    let FutureDate (i: DateTime option) : bool =
        match i with
        | None -> true
        | Some d ->
            d > DateTime.Now
        
    let PastDate (i: DateTime option) : bool =
        match i with
        | None -> true
        | Some d ->
            d < DateTime.Now

    /// <summary>
    /// Validates if a DateTime is before 1905
    /// </summary>
    /// <returns>true if the input is before 1905-01-01</returns>
    let Before1905 (i: DateTime option) : bool =
        match i with
        | None -> true
        | Some d ->
            d < DateTime(1905, 1, 1)


module Program =

    // The input type to the validation
    type PersonInput =
        { Name: string option
          DOB: DateTime option
          Borough: int option }

    // The output type from the validation
    type ValidPerson =
        { Name: string
          DOB: DateTime
          Borough: int option }

    // Strong types for the validations. You could use string though.
    type NameValidations =
        | MustBeEntered
        | MaxLength

    type DOBValidations =
        | MustBeEntered
        | FutureDate
        | Before1905

    type PersonValidations =
        | Name of NameValidations
        | DOB of DOBValidations

    /// <summary>
    /// Validates a person's name against A) Having a value; B) Not being too long
    /// </summary>
    /// <returns>An Ok containing the PersonInput if validation succeeds, or an Error with a NameValidations type if not</returns>
    let validateName (i: PersonInput) =
        match Library.MustBeEntered i.Name with
                 | false ->
                     match Library.MaxLength i.Name with
                     | false -> Ok i
                     | true -> Error(PersonValidations.Name(NameValidations.MaxLength))
                 | true -> Error(PersonValidations.Name(NameValidations.MustBeEntered))
                 
    /// <summary>
    /// Validates a person's DoB against A) Having a value; B) Not being in the future; C) Not being prior to 1905
    /// </summary>
    /// <returns>An Ok containing the PersonInput if validation succeeds, or an Error with a DOBValidations type if not</returns>
    let validateDoB (i: PersonInput) =
        match Library.MustBeEntered i.DOB with
                 | false ->
                     if Library.FutureDate i.DOB then
                         Error(PersonValidations.DOB(DOBValidations.FutureDate))
                     elif Library.Before1905 i.DOB then
                         Error(PersonValidations.DOB(DOBValidations.Before1905))
                     else
                         Ok i
                 | true -> Error(PersonValidations.DOB(DOBValidations.MustBeEntered))
    
    let unwrapPerson (i: PersonInput) : Result<ValidPerson, PersonValidations> =
        Ok({
            Name = i.Name.Value
            DOB = i.DOB.Value
            Borough = i.Borough
        })
    
    /// <summary>
    /// Validates a person against the name and DoB requirements
    /// </summary>
    /// <returns>An Ok containing a ValidPerson if validation succeeds, or an Error with a PersonValidations type if not</returns>
    let personValidator input =
            Ok(input)
            |> Result.bind validateName
            |> Result.bind validateDoB
            |> Result.bind unwrapPerson

    [<EntryPoint>]
    let main _ =
        // An input with no name
        let input1: PersonInput =
            { Name = None
              DOB = Some(DateTime.Now)
              Borough = Some(5) }

        // An input with a date prior to 1905
        let input2: PersonInput =
            { Name = Some("Dave")
              DOB = Some(DateTime(1904, 11, 4))
              Borough = Some(2) }

        // An input with a date in the future and a name that's too long
        let input3: PersonInput =
            { Name = Some("ReallyLongNameToTestTheMaxCharacterLengthRatherThanJustReducingIt")
              DOB = Some(DateTime.MaxValue)
              Borough = None }

        // An input with valid values
        let input4: PersonInput =
            { Name = Some("Steven")
              DOB = Some(DateTime(1986, 4, 17))
              Borough = Some(3) }

        
        match personValidator input1 with
        | Ok _ -> printfn "Input 1 was valid"
        | Error errors -> eprintfn $"Input 1 was invalid with the following errors: %A{errors}"

        match personValidator input2 with
        | Ok _ -> printfn "Input 2 was valid"
        | Error errors -> eprintfn $"Input 2 was invalid with the following errors: %A{errors}"

        match personValidator input3 with
        | Ok _ -> printfn "Input 3 was valid"
        | Error errors -> eprintfn $"Input 3 was invalid with the following errors: %A{errors}"

        match personValidator input4 with
        | Ok _ -> printfn "Input 4 was valid"
        | Error errors -> eprintfn $"Input 4 was invalid with the following errors: %A{errors}"

        0


(*

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

*)