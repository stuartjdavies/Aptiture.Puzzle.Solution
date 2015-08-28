Your task is to write a simple subject assignment system for students. There are 55 students all wanting to enrol in subjects this year.

A student:

·  Has an entrance ranking (value between 0 and 100).

·  May be an international student.

·  May be on a scholarship for reduced fees.

·  Will have to specify 4 subjects they want to enrol in.

This year there are only 3 different types of subjects:

·  Programming subjects

·  Design subjects

·  Literature subjects

The college is growing and is expected to add more subject types in future years, e.g. Business subjects and Science subjects, but these are not required right now.

These are the actual subjects the college is running this year:

·  Java Programming

·  C# Programming

·  PHP Programmming

·  Graphic Design

·  Web Design

·  3D Design

·  English Literature

Requirements:

·  Generate each student object by initialising it with information generated randomly.

·  Create instances of each subject and student then assign the students to subjects.

·  Literature subjects have a limit of 10 students

·  All other subject types have a limit of 20 students

·  When programming subjects are more than 50% full, the student's entrance ranking must be greater than 70 for them to be accepted.

·  When design subjects are more than 70% full, only international students will be accepted.

·  When literature subjects are more than 50% full, people on scholarships will no longer be accepted.

·  When a student can't be enrolled in a subject, print the reason why.

·  Don't worry about object persistence or saving items to a database. When the program ends all data disappears.

·  Note any assumptions you have made.

To save time on this question, you can leave out implementing the body of some methods / member functions, especially those which generate the student instances with random data.