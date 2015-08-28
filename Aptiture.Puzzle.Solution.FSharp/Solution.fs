namespace Aptiture.Puzzle.Solution.FSharp.Solution

open System
open FSharp.Linq

//
// A Student can be enrolled in 0..4 subjects
// A Subject can have many students
// A Enrolment has a Student and a Subject, it is a join table.
//    
type SubjectType = | Programming | Design | Literature | Unknown
type Student = { StudentNumber : string; Ranking : int; IsInternationalStudent : bool; HasScholarship : bool }
type Subject = { Name : string; SubjectType : SubjectType;  MaxSubjectSize: int }
type Enrolment = { StudentNumber : string; SubjectName : string } 
type EnrolmentConfig = { ``Validation Rule 3 - Percentage capacity`` : float; ``Validation Rule 3 - Accepted Ranking`` : int
                         ``Validation Rule 4 - Percentage capacity`` : float;
                         ``Validation Rule 5 - Percentage capacity`` : float; }
                  
type RandomValueGenerator() =
        let r = new Random()   
        member __.genInt(min, max) = r.Next(min, max)
        member __.genBool() = __.genInt(0, 1) = 1

// Used for picking subjects randomly
type RandomItemPicker() =    
        let r = new RandomValueGenerator()    
        member __.pickItem (items : _ list) =                       
                     let index = r.genInt(0, items.Length)    
                     let _, lst = items |> List.partition (fun item -> item = items.[index])                        
                     items.[index], lst

        member __.pickNItems n (items : _ list) =                                       
                     let rec aux count acc (items : _ list) =                        
                                   if count = 0 then
                                       acc
                                   else
                                       match items with
                                       | [] -> raise(new Exception("Invalid count"))                            
                                       | _ -> let item, lst = __.pickItem items
                                              aux (count - 1) (item::acc) lst                         
                     aux n [] items

type RandomStudentGenerator() =
        let r = RandomValueGenerator()          
        member __.genOne studentNumber =
                    { StudentNumber=studentNumber.ToString(); Ranking=r.genInt(0, 100); 
                      IsInternationalStudent=r.genBool(); HasScholarship=r.genBool() }                    
        member __.genMany numberOfStudents = [ for x in 1 .. numberOfStudents -> __.genOne x ] 

module RandomEnrolmentGenerator =                  
        let generate numberOfStudents numberOfSubjectsPerStudents (subjects : Subject list) =                     
                    let students = numberOfStudents |> (new RandomStudentGenerator()).genMany                     
                    students |> List.map(fun s -> subjects |> (new RandomItemPicker()).pickNItems numberOfSubjectsPerStudents |> List.map(fun sub -> s, sub)) |> List.concat  

type ValidatorBuilder() =
        member this.Bind(x, f) =
                    match x with
                    | Some(x) -> Some(x)
                    | _ -> f()
        member this.Delay(f) = f()
        member this.Return(x) = Some x
        member this.ReturnFrom(x) = x

module EnrolmentValidator =                                    
        let Validate = new ValidatorBuilder()
        
        let getPercentageFull (subject : Subject) (es : Enrolment list) = 
                   ((es |> List.filter(fun e -> e.SubjectName = subject.Name) |> List.length |> Convert.ToDouble) / (float subject.MaxSubjectSize)) * 100.0                                      

        let ``Validation Rule 1 - Students have an entrance ranking between 0 and 100``
                               (student : Student) (subject : Subject) (es : Enrolment list) (c : EnrolmentConfig) = 
                    if student.Ranking >= 0 && student.Ranking <= 100 then 
                        None 
                    else 
                        Some(1, sprintf "Student %s has an invalid entrance ranking, the value must between between 0 and 100)" student.StudentNumber)
        
        let ``Validation Rule 2 - All subjects must be below the max student size`` 
                               (student : Student) (subject : Subject) (es : Enrolment list) (c : EnrolmentConfig) =        
                let numberOfStudentsInSubject = es |> List.filter(fun e -> e.SubjectName = subject.Name) |> List.length
                
                if numberOfStudentsInSubject >= subject.MaxSubjectSize then
                    Some(2, sprintf "Can't enrol %s because the maximum number of enrolments has been reached, the maximum students for the subject %s is %d" 
                                                                                                student.StudentNumber subject.Name subject.MaxSubjectSize)
                else             
                    None        
 
        let ``Validation Rule 3 - When programming subjects reach a given percentage capacity then only students are accepted with certain rankings.`` 
                                (student : Student) (subject : Subject) (es : Enrolment list) (c : EnrolmentConfig) =
                    match subject.SubjectType with
                    | Programming when getPercentageFull subject es >= c.``Validation Rule 3 - Percentage capacity`` && student.Ranking < c.``Validation Rule 3 - Accepted Ranking`` -> 
                                                         Some(3, sprintf "Can't enrol %s because when programming subjects are more than %.0f%% full, the student's entrance ranking must be greater than %d for them to be accepted." 
                                                                            student.StudentNumber c.``Validation Rule 3 - Percentage capacity`` c.``Validation Rule 3 - Accepted Ranking``)                              
                    | _ -> None       

        let ``Validation Rule 4 - When design subjects reach a given percentage capacity, only international students will be accepted.`` 
                                (student : Student) (subject : Subject) (es : Enrolment list) (c : EnrolmentConfig) =
                    match subject.SubjectType with 
                    | Design when getPercentageFull subject es >= c.``Validation Rule 4 - Percentage capacity`` && student.IsInternationalStudent = false ->                                                   
                                      Some(4, sprintf "Can't enrol %s because when design subjects are more than %.0f%% full, only international students will be accepted." 
                                                    student.StudentNumber c.``Validation Rule 4 - Percentage capacity``)                     
                    | _ -> None
                    
        let ``Validation Rule 5 - When literature subjects reach a given percentage capacity, people on scholarships will no longer be accepted`` 
                                (student : Student) (subject : Subject) (es : Enrolment list) (c : EnrolmentConfig) =
                    match subject.SubjectType with
                    | Literature when getPercentageFull subject es >= c.``Validation Rule 5 - Percentage capacity`` && student.HasScholarship = true -> 
                                                         Some(5, sprintf "Can't enrol %s because when design subjects are more than %.0f%% full, only international students will be accepted." 
                                                                             student.StudentNumber c.``Validation Rule 5 - Percentage capacity``)         

                    | _ -> None                    
                    
        let isValidEnrolment (student : Student) (subject : Subject) (es : Enrolment list) (c : EnrolmentConfig) =
                         Validate {
                            let! _ = ``Validation Rule 1 - Students have an entrance ranking between 0 and 100`` student subject es c 
                            let! _ = ``Validation Rule 2 - All subjects must be below the max student size`` student subject es c
                            let! _ = ``Validation Rule 3 - When programming subjects reach a given percentage capacity then only students are accepted with certain rankings.`` student subject es c
                            let! _ = ``Validation Rule 4 - When design subjects reach a given percentage capacity, only international students will be accepted.`` student subject es c
                            let! _ = ``Validation Rule 5 - When literature subjects reach a given percentage capacity, people on scholarships will no longer be accepted``  student subject es c                                                                       
                            return! None
                         } 
                         
module StudentEnroller =
        let enrolOne student subject enrolments config  =
                        match EnrolmentValidator.isValidEnrolment student subject enrolments config with
                        | Some (code, msg) -> code, msg, enrolments
                        | None -> 0, sprintf "Student with Student number %s was successfully enroled in subject %s" student.StudentNumber subject.Name, 
                                  { StudentNumber=student.StudentNumber; SubjectName=subject.Name }::enrolments

        let enrolMany config (ss : (Student * Subject) list) =
                        let log, enrolments = ss |> List.fold(fun (log,enrols) (student,subject) -> let code, msg, result = enrolOne student subject enrols config 
                                                                                                    ((code, msg)::log), result) ([],[])
                        List.rev log, List.rev enrolments

                                                                            

         




