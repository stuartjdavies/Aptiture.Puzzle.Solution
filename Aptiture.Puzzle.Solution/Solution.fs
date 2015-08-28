namespace Aptiture.Puzzle.Solution

open System

type SubjectType = | Programming | Design | Literature
type Student = { Id : Guid; StudentNumber : string; Ranking : int; IsInternationalStudent : bool; HasScholarship : bool }
type Subject = { Id : Guid; Name : string; SubjectType : SubjectType;  MaxSubjectSize: int }
type Enrolment = { StudentId : Guid; SubjectId : Guid } 
type StudentEnrollerValidatorConfig = { ``Validation Rule 3 - Percentage capacity`` : int; ``Validation Rule 3 - Accepted Ranking`` : int
                                        ``Validation Rule 4 - Percentage capacity`` : int;
                                        ``Validation Rule 5 - Percentage capacity`` : int; }
type EnrolStudentEvent = { Id : int; Message : String; Student : Student; Subject : Subject }


module StudentGenerator =                    
           let generate() =
                    { Id=Guid.NewGuid(); 
                      StudentNumber=((new Random()).Next(10000, 20000)).ToString(); 
                      Ranking=((new Random()).Next(0, 100)); 
                      IsInternationalStudent=(new Random()).Next(0,1) = 1; HasScholarship=(new Random()).Next(0,1) = 1 }           

module StudentEnrollerValidator =        
        let getStudentsInSubject (subject : Subject) (es : Enrolment list) = (es |> List.filter(fun e -> e.SubjectId = subject.Id))
        let getPercentageFull (subject : Subject) (es : Enrolment list) = (getStudentsInSubject subject es |> List.length) / subject.MaxSubjectSize * 100

        let ``Validation Rule 1 - Students have an entrance ranking between 0 and 100`` (s : Student) = 
                 if s.Ranking >= 0 && s.Ranking <= 100 then 
                    None 
                 else 
                    Some(1, sprintf "Student %s has an invalid entrance ranking, the value must between between 0 and 100)" s.StudentNumber)
        
        let ``Validation Rule 2 - All subjects must be below the max student size`` (student : Student) (subject : Subject) (es : Enrolment list) =        
                if (getStudentsInSubject subject es).Length >= subject.MaxSubjectSize then
                    Some(2, sprintf "Can't enrol %s because the maximum number of enrolments has been reached, the maximum students for the subject %s is %d" student.StudentNumber subject.Name subject.MaxSubjectSize)
                else             
                    None        
 
        let ``Validation Rule 3 - When programming subjects reach a given percentage capacity then only students are accepted with certain rankings.`` 
                                                    (student : Student) (subject : Subject) (es : Enrolment list) (c : StudentEnrollerValidatorConfig) =
                    if getPercentageFull subject es <= c.``Validation Rule 3 - Percentage capacity`` then 
                        None
                    else
                        if student.Ranking > c.``Validation Rule 3 - Accepted Ranking`` then 
                            None
                        else 
                            Some(3, sprintf "Can't enrol %s because when programming subjects are more than %d%% full, 
                                             the student's entrance ranking must be greater than %d for them to be accepted." 
                                            student.StudentNumber c.``Validation Rule 3 - Percentage capacity`` c.``Validation Rule 3 - Accepted Ranking``) 


        let ``Validation Rule 4 - When design subjects reach a given percentage capacity, only international students will be accepted.`` 
                                (student : Student) (subject : Subject) (es : Enrolment list) (c : StudentEnrollerValidatorConfig) =
                    if getPercentageFull subject es <= c.``Validation Rule 4 - Percentage capacity`` then 
                        None                    
                    else
                        if student.IsInternationalStudent = false then 
                            None 
                        else 
                            Some(4, sprintf "Can't enrol %s because when design subjects are more than %d%% full, only international students will be accepted." student.StudentNumber c.``Validation Rule 4 - Percentage capacity``) 

        let ``Validation Rule 5 - When literature subjects reach a given percentage capacity, people on scholarships will no longer be accepted`` 
                                (student : Student) (subject : Subject) (es : Enrolment list) (c : StudentEnrollerValidatorConfig) =
                    if getPercentageFull subject es <= c.``Validation Rule 5 - Percentage capacity`` then 
                        None
                    else
                        if (student.HasScholarship = false) then 
                            None
                        else 
                            Some(5, sprintf "Can't enrol %s because when design subjects are more than %d%% full, only international students will be accepted." student.StudentNumber c.``Validation Rule 5 - Percentage capacity``)         

        let isValidEnrolment (student : Student) (subject : Subject) (es : Enrolment list) (c : StudentEnrollerValidatorConfig) =
                     [ lazy ``Validation Rule 1 - Students have an entrance ranking between 0 and 100`` student
                       lazy ``Validation Rule 2 - All subjects must be below the max student size`` student subject es
                       lazy ``Validation Rule 3 - When programming subjects reach a given percentage capacity then only students are accepted with certain rankings.`` student subject es c
                       lazy ``Validation Rule 4 - When design subjects reach a given percentage capacity, only international students will be accepted.`` student subject es c
                       lazy ``Validation Rule 5 - When literature subjects reach a given percentage capacity, people on scholarships will no longer be accepted`` student subject es c  ]
                     |> List.tryFind(fun item -> match item.Value with | Some _ -> true | None -> false) 
                     |> (fun r -> if (r.IsNone) then None else r.Value.Force())


module StudentEnroller =         
         let enrol student subject enrolments config  =
                 match StudentEnrollerValidator.isValidEnrolment student subject enrolments config with
                 | Some (code, msg) -> code, msg, { StudentId=student.Id; SubjectId=subject.Id }::enrolments
                 | None -> 0, sprintf "Student with Student number %s was successfully enroled in subject %s" student.StudentNumber subject.Name, enrolments
                                                                     


         




