// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open Aptiture.Puzzle.Solution.FSharp.Solution

open System.IO

[<EntryPoint>]
let main argv = 
    
        let subjects = [ { Name="Java Programming"; SubjectType=Programming ;  MaxSubjectSize=20 }
                         { Name="C# Programming"; SubjectType=Programming ;  MaxSubjectSize=20 }
                         { Name="PHP Programming"; SubjectType=Programming ;  MaxSubjectSize=20 }
                         { Name="Graphic Design"; SubjectType=Programming ;  MaxSubjectSize=20 }
                         { Name="Web Design"; SubjectType=Design ;  MaxSubjectSize=20 }
                         { Name="3D Design"; SubjectType=Design ;  MaxSubjectSize=20 }
                         { Name="English Literature"; SubjectType=Literature ;  MaxSubjectSize=10 } ] 

        let config = { ``Validation Rule 3 - Percentage capacity`` = 50.0; 
                       ``Validation Rule 3 - Accepted Ranking`` = 70;
                       ``Validation Rule 4 - Percentage capacity`` = 70.0; 
                       ``Validation Rule 5 - Percentage capacity`` = 50.0; }

        printfn "Enrolment log results" |> ignore

        let log, _ = RandomEnrolmentGenerator.generate 54 4 subjects |> StudentEnroller.enrolMany config
            
        File.WriteAllLines("log.txt", log |> List.map(fun item -> item.ToString()));

        log |> List.iter(fun (code, message) -> printfn "Code - %d, Message - %s" code message |> ignore);

        System.Console.ReadLine() |> ignore
         
        0 // return an integer exit code
