module Aptiture.Puzzle.Solution.Tests

open System
open FsUnit
open FsCheck
open NUnit.Framework
open Swensen.Unquote
open Aptiture.Puzzle.Solution.FSharp.Solution
open FSharp.Core
open System.IO


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


[<Test>]
let ``Verify Validation Rule 1: Given generated student enrolments with various rankings. Only enrolments with rankings between 0 and 100 are valid``() =
            let rg = RandomValueGenerator()
            
            let enrolments = [for x in -150 .. 10 .. 250 -> { Student.StudentNumber=x.ToString(); Ranking=rg.genInt(-5000, 5000); IsInternationalStudent=false; HasScholarship=false },
                                                            { Subject.Name=x.ToString(); SubjectType=Programming;  MaxSubjectSize=1000 }]   
                                
            let numberOfRatingsBetween0and100 = enrolments |> List.filter(fun (s,_) -> s.Ranking >=0 && s.Ranking <= 100) |> List.length            
            let log, _ = enrolments |> StudentEnroller.enrolMany config 

            log |> List.filter(fun (code, _) -> code = 0) |> List.length |> should equal numberOfRatingsBetween0and100

[<Test>]
let ``Verify Validation Rule 2: After we try to enrol more student in as subject than the maximum allowed, the students enroled should not exceed the maximum``() =
            let maxSubjectSize = 2
            let ``Validation Rule 3 - Percentage capacity`` = 400.00
            let enrolments = [for x in 0.. 40 -> { Student.StudentNumber=x.ToString(); Ranking=10; IsInternationalStudent=false; HasScholarship=false },
                                                 { Subject.Name="Test"; SubjectType=Design;  MaxSubjectSize=maxSubjectSize }]   
                                
            let log, _ = enrolments |> StudentEnroller.enrolMany { config with ``Validation Rule 3 - Percentage capacity`` = ``Validation Rule 3 - Percentage capacity`` }
                                               
            log |> List.filter(fun (code, _) -> code = 0) |> List.length |> should equal maxSubjectSize
  
[<Test>]
let ``Verify Validation Rule 3: Given we generate maxSubjectSize number of enrolments for a programming subject with various marks, there should n * percentage capacity students enroled.``() =
            let maxSubjectSize = 10
            let ``Validation Rule 3 - Percentage capacity`` = 50.00
            let ``Validation Rule 3 - Accepted Ranking`` = 70
            
            let enrolments = [for x in 0.. maxSubjectSize -> { Student.StudentNumber=x.ToString(); Ranking=10; IsInternationalStudent=false; HasScholarship=false },
                                                             { Subject.Name="Test"; SubjectType=Programming;  MaxSubjectSize=maxSubjectSize }]   
                                
            let log, _ = enrolments |> StudentEnroller.enrolMany { config with ``Validation Rule 3 - Percentage capacity`` = ``Validation Rule 3 - Percentage capacity``
                                                                               ``Validation Rule 3 - Accepted Ranking`` = ``Validation Rule 3 - Accepted Ranking``}
                                               
            log |> List.filter(fun (code, _) -> code = 0) |> List.length |> should equal (float maxSubjectSize * (``Validation Rule 3 - Percentage capacity`` / 100.00))

[<Test>]
let ``Verify Validation Rule 4: Given that we generate maxSubjectSize number of international student enrolments, there should n * percentage capacity students enroled``() =
            let maxSubjectSize = 10
            let ``Validation Rule 4 - Percentage capacity`` = 70.0

            let r = new RandomValueGenerator()
            let enrolments = [for x in 0 .. maxSubjectSize -> { Student.StudentNumber=x.ToString(); Ranking=r.genInt(51, 100); IsInternationalStudent=false; HasScholarship=false },
                                                              { Subject.Name="Test"; SubjectType=Design;  MaxSubjectSize=maxSubjectSize }]   
                                
            let log, _ = enrolments |> StudentEnroller.enrolMany { config with ``Validation Rule 4 - Percentage capacity`` = ``Validation Rule 4 - Percentage capacity`` }
                                               
            log |> List.filter(fun (code, _) -> code = 0) |> List.length |> should equal (float maxSubjectSize * (``Validation Rule 4 - Percentage capacity`` / 100.00))
  

[<Test>]
let ``Verify Validation Rule 5: Given that we generate maxSubjectSize of scholarship enrolments for a literature subject, there should n * percentage capacity students enroled ``() =
            let maxSubjectSize = 10
            let ``Validation Rule 5 - Percentage capacity`` = 50.00            
            
            let enrolments = [for x in 0.. maxSubjectSize -> { Student.StudentNumber=x.ToString(); Ranking=10; IsInternationalStudent=false; HasScholarship=true },
                                                             { Subject.Name="Test"; SubjectType=Literature;  MaxSubjectSize=maxSubjectSize }]   
                                
            let log, _ = enrolments |> StudentEnroller.enrolMany { config with ``Validation Rule 5 - Percentage capacity`` = ``Validation Rule 5 - Percentage capacity`` }
                                               
            log |> List.filter(fun (code, _) -> code = 0) |> List.length |> should equal (float maxSubjectSize * (``Validation Rule 5 - Percentage capacity`` / 100.00))
 

             
[<Test>]              
let ``Verify that the CSharp result is the same FSharp algorithm result by generating n number enrolments and caparing the log results``() =
            let mapFSharpStudentEnrolmentsToCSharp (es : (Student * Subject) list) = 
                    es |> List.map(fun (student, subject) -> let cstud = new Aptiture.Puzzle.Solution.CSharp.Student(StudentNumber=student.StudentNumber, Ranking=student.Ranking, 
                                                                                                                                IsInternationalStudent=student.IsInternationalStudent,
                                                                                                                                HasScholarship=student.HasScholarship) 
                                                             let csub = Aptiture.Puzzle.Solution.CSharp.Subject(Name=subject.Name, SubjectType=(match subject.SubjectType with
                                                                                                                                                | Programming -> Aptiture.Puzzle.Solution.CSharp.SubjectTypeEnum.Programming
                                                                                                                                                | Design -> Aptiture.Puzzle.Solution.CSharp.SubjectTypeEnum.Design
                                                                                                                                                | Literature -> Aptiture.Puzzle.Solution.CSharp.SubjectTypeEnum.Literature 
                                                                                                                                                | Unknown -> Aptiture.Puzzle.Solution.CSharp.SubjectTypeEnum.Unknown),
                                                                                                                        MaxSubjectSize=subject.MaxSubjectSize)
                                                             cstud, csub)


            // Create a list of 1000 students          
            let enrolments = RandomEnrolmentGenerator.generate 200 4 subjects

            let inputEnrolmentsCSharp = enrolments |> mapFSharpStudentEnrolmentsToCSharp

            let logCSharp, _ = Aptiture.Puzzle.Solution.CSharp.StudentEnroller.enrol(inputEnrolmentsCSharp, [||], 
                                        new Aptiture.Puzzle.Solution.CSharp.EnrolmentConfig(ValidationRule3_PercentageCapacity=50.0,ValidationRule3_AcceptedRanking=70,
                                                                                            ValidationRule4_PercentageCapacity=70.0,ValidationRule5_PercentageCapacity=50.0))                                                                                                                                        
                      
            let log, _ = enrolments |> StudentEnroller.enrolMany config

            
            // File.WriteAllLines("c:\\test\\csharp.txt", logCSharp |> Array.map(fun x -> x.ToString()))
            // File.WriteAllLines("c:\\test\\fsharp.txt", log |> List.map(fun x -> x.ToString()))
            logCSharp |> should equal log
            
                                            
                                                                               
            
                    



