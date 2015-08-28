using Aptiture.Puzzle.Solution.CSharp;
using System.Linq;
using System.IO;

namespace Aptiture.Puzzle.Solution.CSharp.Console
{
    class Program
    {
        static void Main(string[] args)
        {            
            var c = new EnrolmentConfig() {
                ValidationRule3_PercentageCapacity = 50,
                ValidationRule3_AcceptedRanking = 70,
                ValidationRule4_PercentageCapacity = 50,
                ValidationRule5_PercentageCapacity = 50
            };

            var subjects = new Subject[]
            {
                new Subject() { Name = "Java Programming", SubjectType = SubjectTypeEnum.Programming, MaxSubjectSize = 20 },
                new Subject() { Name = "C# Programming", SubjectType = SubjectTypeEnum.Programming, MaxSubjectSize = 20 },
                new Subject() { Name = "PHP Programming", SubjectType = SubjectTypeEnum.Programming, MaxSubjectSize = 20 },
                new Subject() { Name = "Graphic Design", SubjectType = SubjectTypeEnum.Programming, MaxSubjectSize = 20 },
                new Subject() { Name = "Web Design", SubjectType = SubjectTypeEnum.Design, MaxSubjectSize = 20 },
                new Subject() { Name = "3D Design", SubjectType = SubjectTypeEnum.Design, MaxSubjectSize = 20 },
                new Subject() { Name = "English Literature", SubjectType = SubjectTypeEnum.Literature, MaxSubjectSize = 10 }
            };
            
            System.Console.WriteLine("Enrolment log results");

            var enrolments = RandomEnrolmentGenerator.generate(55, subjects, 4);

            // Result is a tuple of log items and enrolments
            var result = StudentEnroller.enrol(enrolments, new Enrolment[] { }, c);
            
            File.WriteAllLines("log.txt", result.Item1.Select(item => item.ToString()));

            result.Item1.ToList().ForEach(item => System.Console.WriteLine("Code - {0}, Message - {1}", item.Item1, item.Item2));

            System.Console.ReadLine();
        }
    }
}
