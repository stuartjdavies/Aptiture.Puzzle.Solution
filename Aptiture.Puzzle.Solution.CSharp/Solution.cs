using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aptiture.Puzzle.Solution.CSharp
{
    public enum SubjectTypeEnum { Programming, Design, Literature, Unknown }

    public class Student
    {
        public string StudentNumber;
        public int Ranking;
        public bool IsInternationalStudent;
        public bool HasScholarship;
    }

    public class Subject
    {
        public string Name;
        public SubjectTypeEnum SubjectType;
        public int MaxSubjectSize;
    }

    public class Enrolment
    {
        public string StudentNumber;
        public string SubjectName;
    }

    public class EnrolmentConfig
    {
        public double ValidationRule3_PercentageCapacity;
        public int ValidationRule3_AcceptedRanking;
        public double ValidationRule4_PercentageCapacity;
        public double ValidationRule5_PercentageCapacity;
    }

   
    // Used for picking random subjects
    public class RandomItemPicker
    {
        public static IEnumerable<T> PickNItems<T>(IEnumerable<T> items, int count)
        {
            var picks = new List<T>();
            var tmp = new List<T>(items);
            var r = new Random();

            for (int i = 0; i < count; i++)
            {
                var index = r.Next(0, tmp.Count);
                picks.Add(tmp[index]);
                tmp.RemoveAt(index);
            }

            return tmp.ToArray();
        }            
    }

    public class RandomStudentGenerator
    {
        Random rg = new Random();

        public Student genOne(string studentNumber)
        {
            return new Student()
            {
                StudentNumber = studentNumber.ToString(),
                Ranking = rg.Next(0, 100),
                IsInternationalStudent = Convert.ToBoolean(rg.Next(0, 1)),
                HasScholarship = Convert.ToBoolean(rg.Next(0, 1))
            };
        }

        public Student[] genMany(int numberOfStudents)
        {
            return Enumerable.Range(0, numberOfStudents).Select(num => genOne((num * 10000).ToString())).ToArray();
        }
    }


    public class RandomEnrolmentGenerator
    {
        public static List<Tuple<Student, Subject>> generate(int numberOfStudents, IEnumerable<Subject> subjects, int numberOfSubjectsPerSubject) {
            var students = (new RandomStudentGenerator()).genMany(numberOfStudents);
            var studentSubjectsMapping = new List<Tuple<Student, Subject>>();

            foreach (var student in students)
            {
                var randomSubjects = RandomItemPicker.PickNItems(subjects, numberOfSubjectsPerSubject);

                foreach (var randomSubject in randomSubjects)
                {
                    studentSubjectsMapping.Add(new Tuple<Student, Subject>(student, randomSubject));
                }
            }

            return studentSubjectsMapping;
        }
    }

    
    public class EnrolmentValidator 
    {       
        public static Tuple<int, string> isValidEnrolment(Student student, Subject subject, IEnumerable<Enrolment> es, EnrolmentConfig c)
        {
            // Validation Rule 1 - Students have an entrance ranking between 0 and 100``
            if (student.Ranking < 0 || student.Ranking > 100)
                return new Tuple<int, string>(1, string.Format("Student {0} has an invalid entrance ranking, the value must between between 0 and 100)", student.StudentNumber));

            var enroledStudents = es.Where(e => e.SubjectName == subject.Name).Count();
                    
            // Validation Rule 2 - All subjects must be below the max student size
            if (enroledStudents >= subject.MaxSubjectSize)
                return new Tuple<int, string>(2, string.Format("Can't enrol {0} because the maximum number of enrolments has been reached, the maximum students for the subject {1} is {2}",
                                                                                                student.StudentNumber, subject.Name, subject.MaxSubjectSize));
            var percentageFull = (Convert.ToDouble(enroledStudents) / Convert.ToDouble(subject.MaxSubjectSize)) * 100.0;

            // Validation Rule 3 - When programming subjects reach a given percentage capacity then only students are accepted with certain rankings.
            if (subject.SubjectType == SubjectTypeEnum.Programming && percentageFull >= c.ValidationRule3_PercentageCapacity && student.Ranking < c.ValidationRule3_AcceptedRanking)
                return new Tuple<int, string>(3, string.Format("Can't enrol {0} because when programming subjects are more than {1}% full, " +
                                                               "the student's entrance ranking must be greater than {2} for them to be accepted.",
                                                                student.StudentNumber, c.ValidationRule3_PercentageCapacity, c.ValidationRule3_AcceptedRanking));

            // Validation Rule 4 - When design subjects reach a given percentage capacity, only international students will be accepted.
            if (subject.SubjectType == SubjectTypeEnum.Design && percentageFull >= c.ValidationRule4_PercentageCapacity && student.IsInternationalStudent == false)
                return new Tuple<int, string>(4, String.Format("Can't enrol {0} because when design subjects are more than {1}% full, only international students will be accepted.",
                                                                                                student.StudentNumber, c.ValidationRule4_PercentageCapacity));

            // Validation Rule 5 - When literature subjects reach a given percentage capacity, people on scholarships will no longer be accepted``                
            if (subject.SubjectType == SubjectTypeEnum.Literature && percentageFull >= c.ValidationRule5_PercentageCapacity && student.HasScholarship == true)
                return new Tuple<int, string>(5, String.Format("Can't enrol {0} because when design subjects are more than {1}% full, only international students will be accepted.",
                                                                                                student.StudentNumber, c.ValidationRule5_PercentageCapacity));

            return new Tuple<int, string>(0, "");
        }
    }
   
    public class StudentEnroller
    {       
        public static Tuple<Tuple<int, string>[], Enrolment[]> enrol(IEnumerable<Tuple<Student, Subject>> studentSubjects, Enrolment[] enrolments, EnrolmentConfig c,
                                                                     Func<Student, Subject, IEnumerable<Enrolment>, EnrolmentConfig, Tuple<int, string>> isValidEnrolment)
        {
            var es = new List<Enrolment>(enrolments);
            var log = new List<Tuple<int,string>>();            

            foreach (var s in studentSubjects)
            {
                var result = isValidEnrolment(s.Item1, s.Item2, es, c);

                if (result.Item1 == 0)
                {
                    es.Add(new Enrolment() { StudentNumber = s.Item1.StudentNumber, SubjectName = s.Item2.Name });
                    log.Add(new Tuple<int, string>(result.Item1, String.Format("Student with Student number {0} was successfully enroled in subject {1}", 
                                                                                    s.Item1.StudentNumber, s.Item2.Name)));
                }
                else
                    log.Add(result);         
            }

            return new Tuple<Tuple<int, string>[], Enrolment[]>(log.ToArray(), es.ToArray());         
        }

        public static Tuple<Tuple<int, string>[], Enrolment[]> enrol(IEnumerable<Tuple<Student, Subject>> studentSubjects, Enrolment[] enrolments, EnrolmentConfig c)
        {
            return enrol(studentSubjects, enrolments, c, EnrolmentValidator.isValidEnrolment);
        }
    }
}
