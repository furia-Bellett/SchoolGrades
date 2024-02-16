
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Text;
using gamon;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;


namespace SchoolGrades
{
    public abstract partial class abstractConvert// DataLayer
    {
         internal abstract List<StudentAnnotation> AnnotationsAboutThisStudent(Student currentStudent, 
            string IdSchoolYear, bool IncludeOnlyActiveAnnotations); 
         internal abstract int? UpdateAnnotationsGroup(StudentAnnotation currentAnnotation, Student currentStudent)
            {
                throw new NotImplementedException();
            }
         internal abstract void EraseAnnotationByText(string AnnotationText, Student Student); 

         internal abstract int? SaveAnnotation(StudentAnnotation Annotation, Student s)
         {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "";
                if (Annotation.IdAnnotation != null && Annotation.IdAnnotation != 0)
                {
                    query = "UPDATE StudentsAnnotations" +
                    " SET" +
                    " idStudent=" + SqlInt(s.IdStudent) + "," +
                    " idSchoolYear=" + SqlString(Annotation.IdSchoolYear) + "," +
                    " instantTaken=" + SqlDate(Annotation.InstantTaken) + "," +
                    " instantClosed=" + SqlDate(Annotation.InstantClosed) + "," +
                    " isActive=" + SqlBool(Annotation.IsActive) + ",";
                    if (FieldExists("StudentsAnnotations", "isPopUp"))
                        query += " isPopUp=" + SqlBool(Annotation.IsPopUp) + ",";
                    query += " annotation=" + SqlString(Annotation.Annotation) + "" +
                    " WHERE idAnnotation=" + SqlInt(Annotation.IdAnnotation) + 
                    ";";
                }
                else
                {
                    Annotation.InstantTaken = DateTime.Now;
                    Annotation.IsActive = true;
                    // create an annotation on database
                    int? nextId = NextKey("StudentsAnnotations", "IdAnnotation");
                    Annotation.IdAnnotation = nextId;

                    query = "INSERT INTO StudentsAnnotations " +
                    "(idAnnotation, idStudent, annotation,instantTaken," +
                    "instantClosed,isActive";
                    if (FieldExists("StudentsAnnotations", "isPopUp"))
                        query += ",isPopUp"; 
                    if (Annotation.IdSchoolYear != null && Annotation.IdSchoolYear != "")
                        query += ",idSchoolYear";
                    query += ")";
                    query += " Values(";
                    query += "" + SqlInt(Annotation.IdAnnotation) + ",";
                    query += "" + SqlInt(s.IdStudent) + ",";
                    query += "" + SqlString(Annotation.Annotation) + "";
                    query += "," + SqlDate(Annotation.InstantTaken);
                    query += "," + SqlDate(Annotation.InstantClosed);
                    query += "," + SqlBool(Annotation.IsActive);
                    if (FieldExists("StudentsAnnotations", "isPopUp"))
                        query += "," + SqlBool(Annotation.IsPopUp);
                    if (Annotation.IdSchoolYear != null && Annotation.IdSchoolYear != "")
                        query += "," + SqlString(Annotation.IdSchoolYear) + "";
                    query += ");";
                }
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return Annotation.IdAnnotation;
        }
         internal abstract StudentAnnotation GetAnnotation(int? IdAnnotation)
        {
            StudentAnnotation a;
            if (IdAnnotation == null)
                return null;
            a = new StudentAnnotation();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM StudentsAnnotations" +
                    " WHERE IdAnnotation=" + IdAnnotation;
                query += ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                dRead.Read();
                a = GetAnnotationFromRow(dRead);
                cmd.Dispose();
            }
            return a;
        }
        private abstract StudentAnnotation GetAnnotationFromRow(DbDataReader Row)
        {
            StudentAnnotation a = new StudentAnnotation();
            a.IdAnnotation = Safe.Int(Row["idAnnotation"]);
            a.IdStudent = Safe.Int(Row["idStudent"]);
            a.IdSchoolYear = Safe.String(Row["idSchoolYear"]);
            a.Annotation = Safe.String(Row["annotation"]);
            a.InstantTaken = Safe.DateTime(Row["instantTaken"]);
            a.InstantClosed = Safe.DateTime(Row["instantClosed"]);
            a.IsActive = Safe.Bool(Row["isActive"]);
             
            // the program must work also with old versions of database 
            if(FieldExists("StudentsAnnotations", "isPopUp"))
            {
                a.IsPopUp = Safe.Bool(Row["isPopUp"]);
            }
            return a;
        }
         internal abstract void EraseAnnotationById(int? IdAnnotation)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM StudentsAnnotations" +
                    " WHERE idAnnotation=" + SqlInt(IdAnnotation) +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract DataTable GetAnnotationsOfClass(int? IdClass, 
            bool IncludeAlsoNonActive, bool IncludeJustPopUp)
        {
            DataTable table = new DataTable(); 
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapter;
                DataSet dSet = new DataSet();
                string query = "SELECT Students.lastName, Students.firstName, StudentsAnnotations.annotation" +
                    ",Students.IdStudent, StudentsAnnotations.IdAnnotation" +
                    " FROM StudentsAnnotations" +
                    " JOIN Students ON Students.idStudent = StudentsAnnotations.idStudent" +
                    " JOIN Classes_Students ON Classes_Students.idStudent = Students.idStudent" +
                    " WHERE Classes_Students.idClass=" + IdClass; 
                if (!IncludeAlsoNonActive)
                    query += " AND isActive=true";
                // !!!! TODO avoid to check field existence after some versions
                // (made to avoid breaking the code with an old database) !!!!
                if (IncludeJustPopUp && FieldExists("StudentsAnnotations", "isPopUp")) 
                    query += " AND isPopUp=true";
                query += ";";
                dAdapter = new SQLiteDataAdapter(query, (System.Data.SQLite.SQLiteConnection)conn);

                dAdapter.Fill(dSet);
                table = dSet.Tables[0];

                dAdapter.Dispose();
                dSet.Dispose();
            }
            return table;
        }
    }
}
    public abstract partial class DataLayer
    {
         internal abstract StudentsAnswer GetStudentsAnswerFromRow(DbDataReader Row)
        {
            StudentsAnswer a = new StudentsAnswer();
            a.IdAnswer = Safe.Int(Row["IdAnswer"]);
            a.IdStudent = Safe.Int(Row["IdStudent"]);
            a.IdStudentsAnswer = Safe.Int(Row["IdStudentsAnswer"]);
            a.IdTest = Safe.Int(Row["IdTest"]);
            a.StudentsBoolAnswer = Safe.Bool(Row["StudentsBoolAnswer"]);
            a.StudentsTextAnswer = Safe.String(Row["StudentsTextAnswer"]);

            return a;
        }
         internal abstract List<StudentsAnswer> GetAllAnswersOfAStudentToAQuestionOfThisTest(
            int? IdStudent, int? IdQuestion, int? IdTest)
        {
            List<StudentsAnswer> list = new List<StudentsAnswer>();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM StudentsAnswers" +
                    " JOIN Answers ON Answers.idAnswer = StudentsAnswers.idAnswer" +
                    " JOIN Questions ON Questions.IdQuestion = Answers.IdQuestion" +
                    " JOIN Tests_Questions ON Questions.IdQuestion = Tests_Questions.IdQuestion" +
                    " WHERE StudentsAnswers.idStudent=" + IdStudent +
                    " AND Questions.IdQuestion=" + IdQuestion + "" +
                    " AND Tests_Questions.IdTest=" + IdTest + "" +
                    ";";

                cmd.CommandText = query;
                DbDataReader dRead;
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    StudentsAnswer a = GetStudentsAnswerFromRow(dRead);
                    list.Add(a);
                }
            }
            return list;
        }
         internal abstract void AddAnswerToQuestion(int? idQuestion, int? idAnswer)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Answers" +
                    " SET idAnswer=" + idAnswer +
                    " WHERE idQuestion =" + idQuestion +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void PurgeDatabase()
        {
            try
            {
                File.Delete(dbName);
            }
            catch (Exception ex)
            {
                ////////Common.LogOfProgram.Error("Sqlite_DataLayerConstructorsAndGeneral | SaveParameter", ex);
            };
        }
         internal abstract List<Answer> GetAllCorrectAnswersToThisQuestionOfThisTest(int? IdQuestion, int? IdTest)
        {
            List<Answer> list = new List<Answer>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT Answers.*" +
                    " FROM Answers" +
                    " JOIN Questions ON Questions.IdQuestion=Answers.IdQuestion" +
                    " JOIN Tests_Questions ON Questions.IdQuestion=Tests_Questions.IdQuestion" +
                    " WHERE Questions.IdQuestion=" + IdQuestion + "" +
                    " AND Tests_Questions.IdTest=" + IdTest + "" +
                    " ORDER BY idAnswer" +
                    ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Answer a =  GetAnswerFromRow(dRead);
                    list.Add(a);
                }
            }
            return list;
        }
         internal abstract Answer GetAnswerFromRow(DbDataReader Row)
        {
            Answer a = new Answer();
            a.IdAnswer = Safe.Int(Row["IdAnswer"]);
            a.IdQuestion = Safe.Int(Row["IdQuestion"]);
            a.ShowingOrder = Safe.Int(Row["ShowingOrder"]);
            a.Text = Safe.String(Row["Text"]);
            a.ErrorCost = Safe.Int(Row["ErrorCost"]);
            a.IsCorrect = Safe.Bool(Row["IsCorrect"]);
            a.IsOpenAnswer = Safe.Bool(Row["IsOpenAnswer"]);
            a.IsMutex = Safe.Bool(Row["IsMutex"]);

            return a;
        }
         internal abstract int CreateAnswer(Answer currentAnswer)
        {
            // trova una chiave da assegnare alla nuova domanda
            int codice = NextKey("Answers", "idAnswer");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Answers" +
                    " (idAnswer,idQuestion,showingOrder,text,errorCost,isCorrect,isOpenAnswer)" +
                    " Values (" + codice +
                    "," + SqlInt(currentAnswer.IdQuestion) +
                    "," + SqlInt(currentAnswer.ShowingOrder) +
                    "," + SqlString(currentAnswer.Text) + "" +
                    "," + SqlDouble(currentAnswer.ErrorCost) +
                    "," + SqlBool(currentAnswer.IsCorrect) +
                    "," + SqlBool(currentAnswer.IsOpenAnswer) +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return codice;
        }
         internal abstract void SaveAnswer(Answer currentAnswer)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Answers" +
                    " SET idAnswer=" + currentAnswer.IdAnswer + "," +
                    " idQuestion=" + currentAnswer.IdQuestion + "," +
                    " isCorrect='" + SqlBool(currentAnswer.IsCorrect) + "'," +
                    " isOpenAnswer='" + SqlBool(currentAnswer.IsOpenAnswer) + "'," +
                    " Text=" + SqlString(currentAnswer.Text) + "," +
                    " errorCost=" + SqlDouble(currentAnswer.ErrorCost.ToString()) + "" +
                    " WHERE idAnswer = " + currentAnswer.IdAnswer +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract List<Answer> GetAnswersOfAQuestion(int? idQuestion)
        {
            List<Answer> l = new List<Answer>();
            DbDataReader dRead;
            DbCommand cmd;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT *" +
                    " FROM Answers" +
                    " WHERE idQuestion=" + idQuestion +
                    " ORDER BY showingOrder;";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Answer t = new Answer();
                    t.IdAnswer = (int)dRead["idAnswer"];
                    t.IdQuestion = (int)dRead["idQuestion"];
                    t.ShowingOrder = (int)dRead["showingOrder"];
                    t.Text = (string)dRead["text"];
                    t.IdAnswer = (int)dRead["idAnswer"];
                    t.ErrorCost = (int)dRead["errorCost"];
                    t.IsCorrect = Safe.Bool(dRead["isCorrect"]);
                    t.IsOpenAnswer = Safe.Bool(dRead["isOpenAnswer"]);

                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract void DeleteOneStudentFromClass(int? IdDeletingStudent, int? IdClass)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Classes_Students" +
                    " WHERE Classes_Students.idClass=" + IdClass +
                    " AND Classes_Students.idStudent=" + IdDeletingStudent.ToString() +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void EraseAllStudentsOfAClass(Class Class)
        {
            using (DbConnection conn = Connect())
            {
                // erase all the info in tables linked to student

                // erase all the grades of the students of the class 
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Grades WHERE idStudent IN" +
                    "(SELECT Students.idStudent FROM Students" +
                    " JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent" +
                    " WHERE Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();

                // erase all the questions of the students of the class
                cmd.CommandText = "DELETE FROM StudentsQuestions WHERE idStudent IN" +
                    "(SELECT Students.idStudent FROM Students" +
                    " JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent" +
                    " WHERE Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();

                // erase all the answers of students of the class
                cmd.CommandText = "DELETE FROM StudentsAnswers WHERE idStudent IN" +
                    "(SELECT Students.idStudent FROM Students" +
                    " JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent" +
                    " WHERE Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();

                // erase all the tests of students of the class
                cmd.CommandText = "DELETE FROM StudentsTests WHERE idStudent IN" +
                    "(SELECT Students.idStudent FROM Students" +
                    " JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent" +
                    " WHERE Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();

                // delete all the photos of students of the class 
                cmd.CommandText = "DELETE FROM StudentsPhotos WHERE StudentsPhotos.idStudentsPhoto IN" +
                    "(SELECT StudentsPhotos_Students.idStudentsPhoto" +
                    " FROM StudentsPhotos, StudentsPhotos_Students, Classes_Students" +
                    " WHERE StudentsPhotos_Students.idStudent = Classes_Students.idStudent" +
                    " AND StudentsPhotos.idStudentsPhoto = StudentsPhotos_Students.idStudentsPhoto" +
                    " AND Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();

                // delete all the references in link table to photos of students of the class 
                cmd.CommandText = "DELETE FROM StudentsPhotos_Students WHERE idStudent IN" +
                    "(SELECT StudentsPhotos_Students.idStudent" +
                    " FROM StudentsPhotos_Students, Classes_Students" +
                    " WHERE StudentsPhotos_Students.idStudent = Classes_Students.idStudent" +
                    " AND Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();

                // delete all the students in class
                // AFTER THIS idStudent OF DELETED IN NOT AVAILABLE ANY LONGER 
                cmd.CommandText = "DELETE FROM Students WHERE idStudent IN" +
                    "(SELECT Students.idStudent FROM Students" +
                    " JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent" +
                    " WHERE Classes_Students.idClass=" + Class.IdClass + ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void EraseClassFromClasses(Class Class)
        {
            //EraseAllStudentsOfAClass(Class); 
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // erase all the photos of the students of the class 
                cmd.CommandText = "DELETE FROM StudentsPhotos_Students" +
                    " WHERE IdStudent IN (" +
                    "SELECT IdStudent FROM Classes_Students WHERE IdClass=" + Class.IdClass + ")" +
                    " AND IdSchoolYear=" + Class.SchoolYear +
                    ";";
                cmd.ExecuteNonQuery();
                // delete all the references in link table between students and classes
                cmd.CommandText = "DELETE FROM Classes_Students" +
                    " WHERE Classes_Students.idClass=" + Class.IdClass +
                    ";";
                cmd.ExecuteNonQuery();
                // erase class from Classes_SchoolSubjects
                cmd.CommandText = "DELETE FROM Classes_SchoolSubjects" +
                    " WHERE Classes_SchoolSubjects.idClass=" + Class.IdClass +
                    ";";
                cmd.ExecuteNonQuery();
                // erase class from Classes_Tests
                cmd.CommandText = "DELETE FROM Classes_Tests" +
                    " WHERE Classes_Tests.idClass=" + Class.IdClass +
                    ";";
                cmd.ExecuteNonQuery();
                // erase class from table Classes 
                cmd.CommandText = "DELETE FROM Classes" +
                    " WHERE Classes.idClass=" + Class.IdClass +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract string CreateOneClassOnlyDatabase(Class Class)
        {
            string newDatabasePathName = Path.Combine(Class.PathRestrictedApplication, @"SchoolGrades\Data");
            if (!Directory.Exists(newDatabasePathName))
                Directory.CreateDirectory(newDatabasePathName);

            string NewDatabasePathName = Path.Combine(newDatabasePathName,
                System.DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss") +
                "_" + Class.Abbreviation + "_" + Class.SchoolYear + "_" +
                Commons.DatabaseFileName_Teacher);
            File.Copy(Commons.PathAndFileDatabase, NewDatabasePathName);

            // open a local connection to database 
            DataLayer newDatabaseDl = Commons.SetDataLayer(NewDatabasePathName);

            // erase all the data of the students of other classes
            using (DbConnection conn = newDatabaseDl.Connect())
            {
                DbCommand cmd = conn.CreateCommand();

                // erase all the other classes
                cmd.CommandText = "DELETE FROM Classes" +
                " WHERE idClass<>" + Class.IdClass + ";";
                cmd.ExecuteNonQuery();

                // erase all the lessons of other classes
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Lessons" +
                    " WHERE idClass<>" + Class.IdClass + ";";
                cmd.ExecuteNonQuery();

                // erase all the students of other classes from the link table
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Classes_Students" +
                 " WHERE idClass<>" + Class.IdClass + ";";
                cmd.ExecuteNonQuery();

                // erase all the students of other classes 
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Students" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students);";
                cmd.ExecuteNonQuery();

                // erase all the StartLinks of other classes
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Classes_StartLinks" +
                    " WHERE idClass<>" + Class.IdClass + ";";
                cmd.ExecuteNonQuery();

                // erase all the grades of other classes' students
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Grades" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students);";
                cmd.ExecuteNonQuery();

                // erase all the links to photos of other classes' students
                // !! retains previous year's photos of this classes students !!
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM StudentsPhotos_Students" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students);";
                cmd.ExecuteNonQuery();

                // erase all the annotations of other classes
                cmd.CommandText = "DELETE FROM StudentsAnnotations" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students)" +
                    ";";
                cmd.ExecuteNonQuery();

                // erase all the photos of other classes' students
                // !! retains previous year's photos of this classes students !!
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM StudentsPhotos" +
                    " WHERE idStudentsPhoto NOT IN" +
                    " (SELECT idStudentsPhoto FROM StudentsPhotos_Students);";
                cmd.ExecuteNonQuery();

                // erase all the questions of the students of the other classes
                // !! StudentsQuestions currently not used !!
                cmd.CommandText = "DELETE FROM StudentsQuestions" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students);";
                cmd.ExecuteNonQuery();

                // erase all the answers  of the students of the other classes
                // !! StudentsAnswers currently not used !!
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM StudentsAnswers" +
                " WHERE idStudent NOT IN" +
                " (SELECT idStudent FROM Classes_Students);";
                cmd.ExecuteNonQuery();

                // erase all the tests of students of the other classes
                // !! StudentsTests currently not used !!
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM StudentsTests" +
                " WHERE idStudent NOT IN" +
                " (SELECT idStudent FROM Classes_Students);";
                cmd.ExecuteNonQuery();

                // erase all the images of other classes' lessons
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Lessons_Images" +
                    " WHERE idLesson NOT IN" +
                    " (SELECT idLesson from Lessons);";
                cmd.ExecuteNonQuery();

                // erase all the topics of other classes' lessons
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Lessons_Topics" +
                    " WHERE idLesson NOT IN" +
                    " (SELECT idLesson from Lessons);";
                cmd.ExecuteNonQuery();

                // erase all the users (table currently not used) 
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Users" +
                    ";";
                cmd.ExecuteNonQuery();

                // null all Special Needs flags (privacy) 
                cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Students" +
                    " SET hasSpecialNeeds=null" +
                    ";";
                cmd.ExecuteNonQuery();

                // copy all the students' photo files that aren't already there or that have a newer date 
                string query = "SELECT StudentsPhotos.photoPath" +
                " FROM StudentsPhotos" +
                " JOIN StudentsPhotos_Students ON StudentsPhotos_Students.idStudentsPhoto = StudentsPhotos.idStudentsPhoto" +
                " JOIN Classes_Students ON StudentsPhotos_Students.idStudent = Classes_Students.idStudent" +
                " WHERE Classes_Students.idClass = " + Class.IdClass + "; ";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dReader = cmd.ExecuteReader();
                while (dReader.Read())
                {
                    string destinationFile = Path.Combine(Class.PathRestrictedApplication,
                        "SchoolGrades", "Images" + (string)dReader["photoPath"]);
                    if (!Directory.Exists(Path.GetDirectoryName(destinationFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    }
                    if (!File.Exists(destinationFile) ||
                        File.GetLastWriteTime(destinationFile)
                        < File.GetLastWriteTime(Path.Combine(Commons.PathImages, (string)dReader["photoPath"])))
                        try
                        {
                            // destination file not existing or older
                            File.Copy(Path.Combine(Path.Combine(Commons.PathImages, (string)dReader["photoPath"])), destinationFile);
                        }
                        catch { }
                }
                // copy all the picture's files that aren't already there or that have a newer date 
                query = "SELECT Images.imagePath, Classes.pathRestrictedApplication" +
                    " FROM Images" +
                    " JOIN Lessons_Images ON Lessons_Images.idImage=Images.idImage" +
                    " JOIN Lessons ON Lessons_Images.idLesson=Lessons.idLesson" +
                    " JOIN Classes ON Classes.idClass=Lessons.idClass" +
                    " WHERE Lessons.idClass=" + Class.IdClass +
                    ";";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                dReader = cmd.ExecuteReader();
                while (dReader.Read())
                {
                    if (dReader["pathRestrictedApplication"] is DBNull)
                    {
                        Console.Beep();
                        break;
                    }
                    if (dReader["imagePath"] is DBNull)
                    {
                        Console.Beep();
                        break;
                    }
                    string destinationFile = Path.Combine((string)dReader["pathRestrictedApplication"],
                        "SchoolGrades", "Images", (string)dReader["imagePath"]);
                    if (!Directory.Exists(Path.GetDirectoryName(destinationFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    }
                    if (!File.Exists(destinationFile) ||
                        File.GetLastWriteTime(destinationFile)
                        < File.GetLastWriteTime(Path.Combine(Commons.PathImages,
                        (string)dReader["imagePath"])))
                        // destination file not existing or older
                        try
                        {
                            File.Copy(Path.Combine(Commons.PathImages, (string)dReader["imagePath"]),
                                destinationFile);
                        }
                        catch { }
                }
                dReader.Dispose();
                // compact the database 
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            return Class.PathRestrictedApplication;
        }
         internal abstract int CreateClass(string ClassAbbreviation, string ClassDescription, string SchoolYear,
            string IdSchool)
        {
            // find a key for the new class
            int idClass = NextKey("Classes", "idClass");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // creation of the new class into the Classes table (just creation!) 
                cmd.CommandText = "INSERT INTO Classes " +
                    "(idClass, Desc, idSchoolYear, idSchool, abbreviation) " +
                    "Values (" + idClass + "," + SqlString(ClassDescription) + "," +
                    SqlString(SchoolYear) + "," + SqlString(IdSchool) + "," +
                    SqlString(ClassAbbreviation) +
                    ");";
                cmd.ExecuteNonQuery();

                int nextId = NextKey("Classes_StartLinks", "idStartLink");
                cmd = conn.CreateCommand();
                // create a link in StartLinks' link table
                cmd.CommandText = "INSERT INTO Classes_StartLinks " +
                    "(idStartLink,idClass,startLink,desc)" +
                    " Values (" + nextId +
                    "," + idClass + ",'http://www.ingmonti.it','Test link'" +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            // if it doesn't exist, create the folder of classes student's images
            if (!Directory.Exists(Path.Combine(Commons.PathImages, SchoolYear, ClassAbbreviation)))
            {
                Directory.CreateDirectory(Path.Combine(Commons.PathImages, SchoolYear, ClassAbbreviation));
            }
            return idClass;
        }
         internal abstract int CreateClassAndStudents(string[,] StudentsData, string ClassAbbreviation,
                    string ClassDescription, string SchoolYear, string OfficialSchoolAbbreviation,
                    bool LinkPhoto)
        {
            // creation of a new class in the Classes table

            // finds a key for the new class
            int idClass = NextKey("Classes", "idClass");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Classes " +
                    "(idClass, Desc, idSchoolYear, idSchool, abbreviation) " +
                    "Values (" + idClass + "," + SqlString(ClassDescription) + "," +
                    SqlString(SchoolYear) + "," + SqlString(OfficialSchoolAbbreviation) + "," +
                    SqlString(ClassAbbreviation) + "" +
                    ");";
                cmd.ExecuteNonQuery();

                // find the key for next student
                int idNextStudent = NextKey("Students", "idStudent");
                // find the key for next picture 
                int idNextPhoto = NextKey("StudentsPhotos", "idStudentsPhoto");
                // add the student to the students' table 
                // start from the second row of the file, first row is descriptions of columns 
                for (int row = 1; row < StudentsData.GetLength(0); row++)
                {
                    int nColumns = StudentsData.GetLength(1);
                    int rigap1 = row + 1, dummy;
                    string query = "INSERT INTO Students " +
                        "(idStudent, lastName, firstName, birthDate, residence, origin, email, birthPlace) " +
                        "Values (" + idNextStudent;
                    // create new student
                    // last name in column 1 
                    query += "," + SqlString(StudentsData[row, 1])
                        + "," + SqlString(StudentsData[row, 2]);
                    // if we have a column 3, it is birth date 
                    if (nColumns > 3)
                        query += "," + SqlString(StudentsData[row, 3]);
                    else
                        query += ", ''";
                    if (nColumns > 4)
                        query += ", ''" + SqlString(StudentsData[row, 4]);
                    else
                        query += ", ''";
                    if (nColumns > 5)
                        query += ", ''" + SqlString(StudentsData[row, 5]);
                    else
                        query += ", ''";
                    if (nColumns > 6)
                        query += ", ''" + SqlString(StudentsData[row, 6]);
                    else
                        query += ", ''";
                    if (nColumns > 7)
                        query += ", ''" + SqlString(StudentsData[row, 7]);
                    else
                        query += ", ''";
                    query += ");";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();

                    // add the new student to the class
                    cmd.CommandText = "INSERT INTO Classes_Students " +
                        "(idClass, idStudent, registerNumber) " +
                        "Values ('" + idClass + "','" + idNextStudent + "','" + rigap1.ToString() + "'" +
                        ");";
                    cmd.ExecuteNonQuery();

                    if (LinkPhoto)
                    {
                        string photoPath = Path.Combine(SchoolYear + ClassAbbreviation,
                            StudentsData[row, 1] + "_" + StudentsData[row, 2] + "_" +
                            ClassAbbreviation + SchoolYear + ".jpg");  // !! TODO here we should put the actual file extension!!
                        // aggiunge la foto alle foto
                        cmd.CommandText = "INSERT INTO StudentsPhotos " +
                            "(idStudentsPhoto, photoPath)" +
                            "Values " +
                            "(" + idNextPhoto + "," + SqlString(photoPath) +
                            ");"; // relative path. Home path will be added at visualization time 
                        cmd.ExecuteNonQuery();

                        // add the picture to the link table
                        cmd.CommandText = "INSERT INTO StudentsPhotos_Students " +
                            "(idStudentsPhoto, idStudent, idSchoolYear) " +
                            "Values (" + idNextPhoto + "," + idNextStudent + "," + SqlString(SchoolYear) +
                            ");";
                        cmd.ExecuteNonQuery();
                        idNextPhoto++;
                    }
                    idNextStudent++;
                }
                cmd.Dispose();
            }
            return idClass;
        }
         internal abstract List<Class> GetClassesOfYear(string School, string Year)
        {
            DbDataReader dRead;
            DbCommand cmd;
            List<Class> lc = new List<Class>();

            // Execute the query
            using (DbConnection conn = Connect())
            {
                string query = "SELECT * " +
                " FROM Classes" +
                " WHERE idSchoolYear = '" + Year + "'" +
                " ORDER BY abbreviation" +
                ";";
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                // fill the list with this year's classes
                while (dRead.Read())
                {
                    Class c = new Class();
                    GetClassFromRow(c, dRead);
                    lc.Add(c);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return lc;
        }
         internal abstract DataTable GetClassTable(int? idClass)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapter;
                DataSet dSet = new DataSet();

                string query = "SELECT * FROM Classes" +
                " WHERE Classes.idClass = " + idClass + ";";
                dAdapter = new SQLiteDataAdapter(query, (System.Data.SQLite.SQLiteConnection)conn);
                dAdapter.Fill(dSet);
                t = dSet.Tables[0];
                dAdapter.Dispose();
                dSet.Dispose();
            }
            return t;
        }
         internal abstract Class GetClassById(int? IdClass)
        {
            Class c = null;
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Classes" +
                    " WHERE Classes.idClass=" + IdClass +
                    ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                dRead.Read();

                c = new Class(IdClass, Safe.String(dRead["abbreviation"]), Safe.String(dRead["idSchoolYear"]),
                    Safe.String(dRead["idSchool"]));
                c.PathRestrictedApplication = Safe.String(dRead["pathRestrictedApplication"]);
                c.UriWebApp = Safe.String(dRead["uriWebApp"]);

                dRead.Dispose();
                cmd.Dispose();
            }
            return c;
        }
         internal abstract DataTable GetClassDataTable(string IdSchool, string IdSchoolYear, string ClassAbbreviation)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapter;
                DataSet dSet = new DataSet();

                string query = "SELECT DISTINCT registerNumber, Classes.idSchool, Classes.idSchoolYear, " +
                                "Classes.abbreviation, Students.*" +
                " FROM Students, Classes_Students, Classes" +
                " WHERE Students.idStudent=Classes_Students.idStudent AND Classes.idClass = Classes_Students.idClass" +
                    " AND Classes.idSchool=" + SqlString(IdSchool) + " AND Classes.idSchoolYear = " + SqlString(IdSchoolYear) +
                    " AND Classes.abbreviation=" + SqlString(ClassAbbreviation) +
                    " ORDER BY Students.lastName, Students.firstName;";
                dAdapter = new SQLiteDataAdapter(query,
                    (System.Data.SQLite.SQLiteConnection)conn);
                dAdapter.Fill(dSet);
                t = dSet.Tables[0];

                dAdapter.Dispose();
                dSet.Dispose();
            }
            return t;
        }
         internal abstract Class GetClass(string IdSchool, string IdSchoolYear, string ClassAbbreviation)
        {
            Class c = new Class();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                string query = "SELECT Classes.*" +
                   " FROM Classes" +
                   " WHERE" +
                   " Classes.idSchoolYear=" + SqlString(IdSchoolYear) +
                   " AND Classes.abbreviation=" + SqlString(ClassAbbreviation);
                if (IdSchool != null && IdSchool != "")
                    query += " AND Classes.idSchool = " + SqlString(IdSchool);
                query += ";";

                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    GetClassFromRow(c, dRead);
                    break; // just the first! 
                }
            }
            return c;
        }
         internal abstract Class GetClassOfStudent(string IdSchool, string SchoolYearCode, Student Student)
        {
            Class c = new Class();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Classes.*" +
                   " FROM Classes" +
                   " JOIN Classes_Students ON Classes.idClass = Classes_Students.idClass" +
                   " JOIN Students ON Students.idStudent = Classes_Students.idStudent" +
                   " WHERE" +
                   " Classes.idSchool = " + SqlString(IdSchool) + "" +
                   " AND Classes.idSchoolYear = " + SqlString(SchoolYearCode) + "" +
                   " AND Students.IdStudent = " + Student.IdStudent +
                   ";";
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    GetClassFromRow(c, dRead);
                    break; // just the first! 
                }
            }
            return c;
        }
         internal abstract void SaveClass(Class Class)
        {
            //bool leaveConnectionOpen = true;
            //if (conn == null)
            //{
            //    conn = dl.Connect();
            //    leaveConnectionOpen = false;
            //}
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "UPDATE Classes" +
                    " SET" +
                    " idClass=" + Class.IdClass + "" +
                    ",idSchoolYear=" + SqlString(Class.SchoolYear) + "" +
                    ",idSchool=" + SqlString(Class.IdSchool) + "" +
                    ",abbreviation=" + SqlString(Class.Abbreviation) + "" +
                    ",desc=" + SqlString(Class.Description) + "" +
                    ",uriWebApp=" + SqlString(Class.UriWebApp) + "" +
                    ",pathRestrictedApplication=" + SqlString(Class.PathRestrictedApplication) + "" +
                    " WHERE idClass=" + Class.IdClass +
                    ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void GetClassFromRow(Class Class, DbDataReader Row)
        {
            if (Class == null)
                Class = new Class();
            Class.IdClass = (int)Row["idClass"];
            Class.Abbreviation = Safe.String(Row["abbreviation"]);
            Class.IdSchool = Safe.String(Row["idSchool"]);
            Class.PathRestrictedApplication = Safe.String(Row["pathRestrictedApplication"]);
            Class.SchoolYear = Safe.String(Row["idSchoolYear"]);
            Class.UriWebApp = Safe.String(Row["uriWebApp"]);
            Class.Description = Safe.String(Row["desc"]);
        }
         internal abstract List<SchoolYear> GetSchoolYearsThatHaveClasses()
        {
            DbDataReader dRead;
            DbCommand cmd;
            List<SchoolYear> ly = new List<SchoolYear>();

            // Execute the query
            using (DbConnection conn = Connect())
            {
                string query = "SELECT DISTINCT SchoolYears.*" +
                " FROM SchoolYears" +
                " JOIN Classes ON Classes.IdSchoolYear = SchoolYears.IdSchoolYear" +
                " WHERE SchoolYears.IdSchoolYear IS NOT NULL" +
                " ORDER BY IdSchoolYear" +
                ";";
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    SchoolYear y = new SchoolYear();
                    y.IdSchoolYear = (string)dRead["idSchoolYear"];
                    y.ShortDescription = Safe.String(dRead["shortDesc"]);
                    y.Notes = Safe.String(dRead["notes"]);

                    ly.Add(y);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return ly;
        }
         internal abstract Class GetThisClassNextYear(Class Class)
        {
            string nextYear = Commons.IncreaseIntegersInString(Class.SchoolYear);
            string nextAbbreviation = Commons.IncreaseIntegersInString(Class.Abbreviation);
            return GetClass(Class.IdSchool, nextYear, nextAbbreviation);
        }
        private abstract string BuildAndClauseOnPassedField(List<Class> classes, string FieldName)
        {
            // we assume that classes have no nulls 
            string andClause = string.Empty;
            foreach (Class c in classes)
            {
                andClause += FieldName + "<>" + c.IdClass + " AND ";
            }
            andClause = andClause.Substring(0, andClause.Length - 5);
            return andClause;
        }
        private abstract string BuildOrClauseOnPassedField(List<Class> classes, string FieldName)
        {
            // we assume that classes have no nulls 
            string orClause = string.Empty;
            foreach (Class c in classes)
            {
                orClause += FieldName + "=" + c.IdClass + " OR ";
            }
            orClause = orClause.Substring(0, orClause.Length - 4);
            return orClause;
        }

    }
}
using gamon;
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
        /// <summary>
        /// Data Access Layer: abstracts the access to dbms using to transfer data 
        /// DbClasses and ADO db classes (ADO should be avoided, if possible) 
        /// </summary>
        private abstract string dbName;
        #region constructors
        /// <summary>
        /// Constructor of DataLayer class that uses the default database of the program
        /// Assumes that the file exists.
        /// </summary>
         internal abstract DataLayer()
        {
            // ???? is next if() useful ????
            if (!System.IO.File.Exists(Commons.PathAndFileDatabase))
            {
                string err = @"[" + Commons.PathAndFileDatabase + " not in the current nor in the dev directory]";
                Commons.ErrorLog(err);
                throw new System.IO.FileNotFoundException(err);

            }
            dbName = Commons.PathAndFileDatabase;
        }
        /// <summary>
        /// Constructor of DataLayer class that get from outside the databases to use
        /// Assumes that the file exists.
        /// </summary>
         internal abstract DataLayer(string PathAndFile)
        {
            dbName = PathAndFile;
        }
        #endregion
        #region properties
         internal abstract string NameAndPathDatabase
        {
            get { return dbName; }
            //set { nomeEPathDatabase = value; }
        }
        #endregion
         internal abstract DbConnection Connect()
        {
            DbConnection connection;
            try
            {
                connection = new SQLiteConnection("Data Source=" + dbName +
                ";version=3;new=False;datetimeformat=CurrentCulture");
                connection.Open();
            }
            catch (Exception ex)
            {
#if DEBUG
                //Get call stack
                StackTrace stackTrace = new StackTrace();
                //Log calling method name
                Commons.ErrorLog("Connect Method in: " + stackTrace.GetFrame(1).GetMethod().Name);
#endif
                Commons.ErrorLog("Error connecting to the database: " + ex.Message + "\r\nFile SQLIte>: " + dbName + " " + "\n");
                connection = null;
            }
            return connection;
        }
        public int nFieldDbDataReader(string NomeCampo, DbDataReader dr)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i) == NomeCampo)
                {
                    return i;
                }
            }
            return -1;
        }
         internal abstract void CompactDatabase()
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // compact the database 
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            //Application.Exit();
        }
         internal abstract School GetSchool(string OfficialSchoolAbbreviation)
        {
            // !!!! TODO read school info from the database !!!!
            School news = new School();
            // the next should be a real integer id, 
            news.IdSchool = Commons.IdSchool;
            news.Name = "ITT Pascal - Cesena";
            news.Desc = "Istituto Tecnico Tecnologico Blaise Pascal, Cesena";
            news.OfficialSchoolAbbreviation = Commons.IdSchool;
            return news;
        }
         internal abstract int NextKey(string Table, string KeyName)
        {
            int nextId;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT MAX(" + KeyName + ") FROM " + Table + ";";
                var firstColumn = cmd.ExecuteScalar();
                if (firstColumn != DBNull.Value)
                {
                    nextId = int.Parse(firstColumn.ToString()) + 1;
                }
                else
                {
                    nextId = 1;
                }
                cmd.Dispose();
            }
            return nextId;
        }
         internal abstract bool CheckKeyExistence
            (string TableName, string KeyName, string KeyValue)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT " + KeyName + " FROM " + TableName +
                    " WHERE " + KeyName + "=" + SqlString(KeyValue) +
                    ";";
                var keyResult = cmd.ExecuteScalar();
                cmd.Dispose();
                if (keyResult != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
         internal abstract void CreateNewDatabase()
        {
            DbCommand cmd;
            // erase all the data on all the tables
            using (DbConnection conn = Connect()) // connect to the new database, just copied
            {
                cmd = conn.CreateCommand();

                // erase all the answers to questions
                cmd.CommandText = "DELETE FROM Answers;" +
                "DELETE FROM Students;" +
                "DELETE FROM SchoolYears;" +
                "DELETE FROM Schools;" +
                "DELETE FROM Classes;" +
                "DELETE FROM QuestionTypes;" +
                "DELETE FROM Topics;" +
                "DELETE FROM Subjects;" +
                "DELETE FROM SchoolSubjects;" +
                "DELETE FROM Images;" +
                "DELETE FROM Questions;" +
                "DELETE FROM Answers;" +
                "DELETE FROM TestTypes;" +
                "DELETE FROM Tests;" +
                "DELETE FROM Classes_Tests;" +
                "DELETE FROM Tags;" +
                "DELETE FROM Tests_Tags;" +
                "DELETE FROM Tests_Questions;" +
                "DELETE FROM Questions_Tags;" +
                "DELETE FROM Answers_Questions;" +
                "DELETE FROM Classes_SchoolSubjects;" +
                "DELETE FROM GradeCategories;" +
                "DELETE FROM GradeTypes;" +
                "DELETE FROM Grades;" +
                "DELETE FROM Students_GradeTypes;" +
                "DELETE FROM SchoolPeriodTypes;" +
                "DELETE FROM SchoolPeriods;" +
                "DELETE FROM StudentsAnswers;" +
                "DELETE FROM StudentsQuestions;" +
                "DELETE FROM StudentsTests;" +
                "DELETE FROM StudentsPhotos;" +
                "DELETE FROM StudentsTests_StudentsPhotos;" +
                "DELETE FROM StudentsPhotos_Students;" +
                "DELETE FROM Classes_Students;" +
                "DELETE FROM Lessons;" +
                "DELETE FROM Lessons_Topics;" +
                "DELETE FROM Lessons_Images;" +
                "DELETE FROM Classes_StartLinks;" +
                "DELETE FROM Flags;" +
                "DELETE FROM usersCategories;" +
                "DELETE FROM Users;";
                cmd.ExecuteNonQuery();
                // compact the database 
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void BackupAllStudentsDataTsv()
        {
            BackupTableTsv("Students");
            BackupTableTsv("StudentsPhotos");
            BackupTableTsv("StudentsPhotos_Students");
            BackupTableTsv("Classes_Students");
            BackupTableTsv("Grades");
        }
         internal abstract void BackupAllStudentsDataXml()
        {
            BackupTableXml("Students");
            BackupTableXml("StudentsPhotos");
            BackupTableXml("StudentsPhotos_Students");
            BackupTableXml("Classes_Students");
            BackupTableXml("Grades");
        }
         internal abstract void RestoreAllStudentsDataTsv(bool MustErase)
        {
            RestoreTableTsv("Students", MustErase);
            RestoreTableTsv("StudentsPhotos", MustErase);
            RestoreTableTsv("StudentsPhotos_Students", MustErase);
            RestoreTableTsv("Classes_Students", MustErase);
            RestoreTableTsv("Grades", MustErase);
        }
         internal abstract void RestoreAllStudentsDataXml(bool MustErase)
        {
            RestoreTableXml("Students", MustErase);
            RestoreTableXml("StudentsPhotos", MustErase);
            RestoreTableXml("StudentsPhotos_Students", MustErase);
            RestoreTableXml("Classes_Students", MustErase);
            RestoreTableXml("Grades", MustErase);
        }
         internal abstract void BackupTableTsv(string TableName)
        {
            DbDataReader dRead;
            DbCommand cmd;
            string fileContent = "";

            using (DbConnection conn = Connect())
            {
                string query = "SELECT *" +
                    " FROM " + TableName + " ";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                dRead = cmd.ExecuteReader();
                int y = 0;
                while (dRead.Read())
                {
                    // field names only in first row 
                    if (y == 0)
                    {
                        string types = "";
                        for (int i = 0; i < dRead.FieldCount; i++)
                        {
                            fileContent += "\"" + dRead.GetName(i) + "\"\t";
                            types += "\"" + Safe.String(dRead.GetDataTypeName(i)) + "\"\t";
                        }
                        fileContent = fileContent.Substring(0, fileContent.Length - 1) + "\r\n";
                        fileContent += types.Substring(0, types.Length - 1) + "\r\n";
                    }
                    // field values
                    string values = "";
                    if (dRead.GetValue(0) != null)
                    {
                        Console.Write(dRead.GetValue(0));
                        for (int i = 0; i < dRead.FieldCount; i++)
                        {
                            values += "\"" + Safe.String(dRead.GetValue(i).ToString()) + "\"\t";
                        }
                        fileContent += values.Substring(0, values.Length - 1) + "\r\n";
                    }
                    else
                    {

                    }
                    y++;
                }
                TextFile.StringToFile(Path.Combine(Commons.PathDatabase, TableName + ".tsv"), fileContent, false);
                dRead.Dispose();
                cmd.Dispose();
            }
        }
         internal abstract void BackupTableXml(string TableName)
        {
            DataAdapter dAdapt;
            DataSet dSet = new DataSet();
            DataTable t;
            string query = "SELECT *" +
                    " FROM " + TableName + ";";

            using (DbConnection conn = Connect())
            {
                dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                dSet = new DataSet("GetTable");
                dAdapt.Fill(dSet);
                t = dSet.Tables[0];

                t.WriteXml(Path.Combine(Commons.PathDatabase, TableName + ".xml"),
                    XmlWriteMode.WriteSchema);

                dAdapt.Dispose();
                dSet.Dispose();
            }
        }
         internal abstract void RestoreTableTsv(string TableName, bool EraseBefore)
        {
            List<string> fieldNames;
            List<string> fieldTypes = new List<string>();
            //string[,] dati = FileDiTesto.FileInMatrice(Commons.PathDatabase +
            //    "\\" + TableName + ".tsv", '\t',
            //    out fieldsNames, out fieldTypes);
            string dati = TextFile.FileToString(Path.Combine(Commons.PathDatabase,
                TableName + ".tsv"));
            if (dati is null)
                return;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                if (EraseBefore)
                {
                    // first: erases existing rows in the table
                    cmd.CommandText += "DELETE FROM " + TableName + ";";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    throw new Exception("Append of table records to an existing table id not implemented yet");
                    //return; 
                }
                string fieldsString = " (";
                string valuesString;
                int fieldsCount = 0;

                int index = 0;
                string fieldName = "";
                while (index < dati.Length)
                {
                    // parse first line: field names
                    fieldNames = new List<string>();
                    do
                    {
                        if (dati[index++] != '\"')
                            return; // error! 
                        fieldName = "";
                        while (dati[index] != '\"')
                        {
                            fieldName += dati[index++];
                        }
                        fieldNames.Add(fieldName);
                        fieldsString += fieldName + ",";
                        fieldsCount++;
                        if (dati[++index] != '\t' && dati[index] != '\r')
                            return; // ERROR!
                    } while (dati[++index] != '\r');
                    index++; // void line feed

                    // parse second line: field types
                    string fieldType = "";
                    while (dati[index] != '\r')
                    {
                        while (dati[index] != '\"')
                        {
                            fieldType += dati[index++];
                        }
                        fieldTypes.Add(fieldType);
                        fieldsString += fieldName + ",";
                        fieldsCount++;
                    }
                    index++; // void line feed

                    // parse the rest of the rows: values
                    string fieldValue = "";
                    while (dati[index] != '\r')
                    {
                        while (dati[index] != '\"')
                        {
                            fieldType += dati[index++];
                        }
                        fieldTypes.Add(fieldType);
                        fieldsString += fieldName + ",";
                        fieldsCount++;
                    }
                }
                //for (int col = 0; col < dati.GetLength(1); col++)
                //{
                //    if (fieldNames[col] != "")
                //    {
                //        fieldsString += fieldNames[col] + ",";
                //        fieldsCount++; 
                //    }
                //}
                //fieldsString = fieldsString.Substring(0, fieldsString.Length - 1);
                //fieldsString += ")";
                //for (int row = 0; row < dati.GetLength(0); row++)
                //{
                //    valuesString = " Values (";
                //    for (int col = 0; col < fieldsCount; col++)
                //    {
                //        if (fieldNames[col] != "")
                //        {
                //            if (fieldTypes[col].IndexOf("VARCHAR") >= 0)
                //                valuesString += "" + SqlString(dati[row, col]) + ",";
                //            else if (fieldTypes[col].IndexOf("INT") >= 0)
                //                valuesString +=  SqlInt(dati[row, col]) + ",";
                //            else if (fieldTypes[col].IndexOf("REAL") >= 0)
                //                valuesString += SqlFloat(dati[row, col]) + ",";
                //            else if (fieldTypes[col].IndexOf("FLOAT") >= 0)
                //                valuesString += SqlFloat(dati[row, col]) + ",";
                //            else if (fieldTypes[col].IndexOf("DATE") >= 0)
                //                valuesString += SqlDate(dati[row, col]) + ",";
                //        }
                //    }
                //    valuesString = valuesString.Substring(0, valuesString.Length - 1);
                //    valuesString += ")";
                //    cmd.CommandText = "INSERT INTO " + TableName +
                //                fieldsString +
                //                valuesString;
                //    //" WHERE " + fieldsNames[0] + "=";
                //    //if (fieldTypes[0].IndexOf("VARCHAR") >= 0)
                //    //    cmd.CommandText += "'" + StringSql(dati[row, 0]) + "'";
                //    //else
                //    //    cmd.CommandText += StringSql(dati[row, 0]);
                //    cmd.CommandText += ";";
                //    cmd.ExecuteNonQuery();
                //}
                //cmd.Dispose();
            }
        }
         internal abstract void RestoreTableXml(string TableName, bool EraseBefore)
        {
            DataSet dSet = new DataSet();
            DataTable t = null;
            dSet.ReadXml(Commons.PathDatabase + "\\" + TableName + ".xml", XmlReadMode.ReadSchema);
            t = dSet.Tables[0];
            if (t.Rows.Count == 0)
                return;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd;
                cmd = conn.CreateCommand();
                if (EraseBefore)
                {
                    cmd.CommandText = "DELETE FROM " + TableName + ";";
                    cmd.ExecuteNonQuery();
                }
                cmd.CommandText = "INSERT INTO " + TableName + "(";
                // column names
                DataRow r = t.Rows[0];
                foreach (DataColumn c in t.Columns)
                {
                    cmd.CommandText += c.ColumnName + ",";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 1);
                cmd.CommandText += ")VALUES";
                // row values
                foreach (DataRow row in t.Rows)
                {
                    cmd.CommandText += "(";
                    foreach (DataColumn c in t.Columns)
                    {
                        switch (Type.GetTypeCode(c.DataType))
                        {
                            case TypeCode.String:
                            case TypeCode.Char:
                                {
                                    cmd.CommandText += "" + SqlString(row[c.ColumnName].ToString()) + ",";
                                    break;
                                };
                            case TypeCode.DateTime:
                                {
                                    DateTime? d = Safe.DateTime(row[c.ColumnName]);
                                    cmd.CommandText += "'" +
                                        ((DateTime)(d)).ToString("yyyy-MM-dd_HH.mm.ss") + "',";
                                    break;
                                }
                            default:
                                {
                                    if (!(row[c.ColumnName] is DBNull))
                                        cmd.CommandText += row[c.ColumnName] + ",";
                                    else
                                        cmd.CommandText += "0,";
                                    break;
                                }
                        }
                    }
                    cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 1);
                    cmd.CommandText += "),";
                }
                cmd.CommandText = cmd.CommandText.Substring(0, cmd.CommandText.Length - 1);
                cmd.CommandText += ";";
                cmd.ExecuteNonQuery();
                dSet.Dispose();
                t.Dispose();
                cmd.Dispose();
            }
        }
        private abstract bool FieldExists(string TableName, string FieldName)
        {
            // watch if field isPopUp exist in the database
            DataTable table = new DataTable();
            bool fieldExists;
            using (DbConnection conn = Connect())
            {
                table = conn.GetSchema("Columns", new string[] { null, null, TableName, null });
                fieldExists = false;
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        if (row["COLUMN_NAME"].ToString() == FieldName)
                        {
                            fieldExists = true;
                            break;
                        }
                    }
                }
            }
            return fieldExists;
        }
         internal abstract bool IsTableReadable(string Table)
        {
            try
            {
                int Id;
                //var row = null; 
                using (DbConnection conn = Connect())
                {
                    DbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM (" + Table + ") LIMIT 1 ;";
                    var row = cmd.ExecuteScalar();
                    cmd.Dispose();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract void EraseAllNotConcerningDataOfOtherClasses(DataLayer newDatabaseDl, List<Class> Classes)
        {
            DbCommand cmd;
            using (DbConnection conn = newDatabaseDl.Connect()) // connect to the new database, just copied
            {
                // erase all the users
                cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Users" +
                    ";";
                cmd.ExecuteNonQuery();

                cmd = conn.CreateCommand();

                // erase the classes not involved in the generation of demo classes 
                cmd.CommandText = "DELETE FROM Classes" +
                " WHERE " + BuildAndClauseOnPassedField(Classes, "IdClass") +
                ";";
                cmd.ExecuteNonQuery();

                // erase all the lessons of other classes
                cmd.CommandText = "DELETE FROM Lessons" +
                    " WHERE " + BuildAndClauseOnPassedField(Classes, "IdClass") +
                    ";";
                cmd.ExecuteNonQuery();
                // erase all the students of other classes from the link table
                cmd.CommandText = "DELETE FROM Classes_Students" +
                 " WHERE " + BuildAndClauseOnPassedField(Classes, "IdClass") +
                 ";";
                cmd.ExecuteNonQuery();
                // erase all the students of other classes 
                cmd.CommandText = "DELETE FROM Students" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students" +
                    " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                    ");";
                cmd.ExecuteNonQuery();

                // erase all the annotations, of all classes
                cmd.CommandText = "DELETE FROM StudentsAnnotations" +
                    ";";
                cmd.ExecuteNonQuery();

                // erase all the StartLinks of ALL the classes (they will be re-done in the new database) 
                cmd.CommandText = "DELETE FROM Classes_StartLinks" +
                    ";";
                cmd.ExecuteNonQuery();

                // erase all the grades of other classes' students
                cmd.CommandText = "DELETE FROM Grades" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT idStudent FROM Classes_Students" +
                    " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                    ");";
                cmd.ExecuteNonQuery();

                //// erase all the links to photos of other classes' students
                //cmd.CommandText = "DELETE FROM StudentsPhotos_Students" +
                //    " WHERE idStudent NOT IN" +
                //    " (SELECT idStudent FROM Classes_Students" +
                //    " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                //    ");";

                // erase all the links to photos of all students
                cmd.CommandText = "DELETE FROM StudentsPhotos_Students" +
                    ";";
                cmd.ExecuteNonQuery();

                //// erase all the photos of other classes' students
                //cmd.CommandText = "DELETE FROM StudentsPhotos WHERE StudentsPhotos.idStudentsPhoto NOT IN" +
                //    "(SELECT StudentsPhotos_Students.idStudentsPhoto" +
                //    " FROM StudentsPhotos, StudentsPhotos_Students, Classes_Students" +
                //    " WHERE StudentsPhotos_Students.idStudent = Classes_Students.idStudent" +
                //    " AND StudentsPhotos.idStudentsPhoto = StudentsPhotos_Students.idStudentsPhoto" +
                //    " AND (" + BuildOrClauseOnPassedField(Classes, "Classes_Students.idClass") +
                //    "));";
                //cmd.ExecuteNonQuery();

                // erase all the photos of students. Those of new students will be created after 
                cmd.CommandText = "DELETE FROM StudentsPhotos" +
                    ";";
                cmd.ExecuteNonQuery();

                // erase all the images of other classes
                cmd.CommandText = "DELETE FROM Images WHERE Images.idImage NOT IN" +
                    "(SELECT DISTINCT Lessons_Images.idImage" +
                    " FROM Images, Lessons_Images, Lessons" +
                    " WHERE Lessons_Images.idImage = Images.idImage" +
                    " AND Lessons_Images.idLesson = Lessons.idLesson" +
                    " AND (" + BuildOrClauseOnPassedField(Classes, "Lessons.idClass") +
                    ")); ";
                cmd.ExecuteNonQuery();

                //erase all links to the images of other classes
                cmd.CommandText = "DELETE FROM Lessons_Images WHERE Lessons_Images.idImage NOT IN" +
                    "(SELECT DISTINCT Lessons_Images.idImage" +
                    " FROM Images, Lessons_Images, Lessons" +
                    " WHERE Lessons_Images.idImage = Images.idImage" +
                    " AND Lessons_Images.idLesson = Lessons.idLesson" +
                    " AND (" + BuildOrClauseOnPassedField(Classes, "Lessons.idClass") +
                    "));";
                cmd.ExecuteNonQuery();

                // erase all the questions of the students of the other classes
                // !! StudentsQuestions currently not used !!
                cmd.CommandText = "DELETE FROM StudentsQuestions" +
                    " WHERE idStudent NOT IN" +
                    " (SELECT DISTINCT idStudent FROM Classes_Students" +
                    " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                    ");";
                cmd.ExecuteNonQuery();

                // erase all the answers  of the students of the other classes
                // !! StudentsAnswers is currently not used !!
                cmd.CommandText = "DELETE FROM StudentsAnswers" +
                " WHERE idStudent NOT IN" +
                " (SELECT idStudent FROM Classes_Students" +
                " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                    ");";
                cmd.ExecuteNonQuery();

                // erase all the tests of students of the other classes
                // !! StudentsTests is currently not used !!
                cmd.CommandText = "DELETE FROM StudentsTests" +
                " WHERE idStudent NOT IN" +
                " (SELECT idStudent FROM Classes_Students" +
                " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                    ");";
                cmd.ExecuteNonQuery();

                // erase all the topics of other classes' lessons
                cmd.CommandText = "DELETE FROM Lessons_Topics" +
                    " WHERE idLesson NOT IN" +
                    " (SELECT idLesson from Lessons" +
                    " WHERE " + BuildOrClauseOnPassedField(Classes, "IdClass") +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();

                // !!!! TODO delete SchoolPeriods not involved in the database demo" !!!!
                //int IdStartLink = NextKey("Classes_StartLinks", "IdStartLink");
                //cmd.CommandText = "DELETE FROM Lessons_Topics" +
                //    " WHERE idLesson NOT IN" +
                //    " (SELECT idLesson from Lessons" +
                //    " WHERE " + OrClauseOnPassedField(Classes, "IdClass") +
                //    ");";
                //cmd.ExecuteNonQuery();
                //cmd.Dispose();

                // !!!! TODO delete SchoolYears not involved in the database demo" !!!!
                //int IdStartLink = NextKey("Classes_StartLinks", "IdStartLink");
                //cmd.CommandText = "DELETE FROM Lessons_Topics" +
                //    " WHERE idLesson NOT IN" +
                //    " (SELECT idLesson from Lessons" +
                //    " WHERE " + OrClauseOnPassedField(Classes, "IdClass") +
                //    ");";
                //cmd.ExecuteNonQuery();
                //cmd.Dispose();
            }
        }
         internal abstract void CreateDemoDataInDatabase(DataLayer newDatabaseDl, List<Class> Classes)
        {
            DbCommand cmd;
            string query;
            // modify all the data that hasn't been erased
            using (DbConnection conn = newDatabaseDl.Connect()) // connect to the new database, just copied
            {
                cmd = conn.CreateCommand();
                int classCount = 1;
                // change the data of all the classes
                foreach (Class c in Classes)
                {
                    // general data of the class
                    c.Abbreviation = c.Abbreviation.Substring(0, 1) + "DEMO" + classCount;
                    c.Description = "SchoolGrades demo class " + c.Abbreviation + ", year " + c.SchoolYear;
                    c.PathRestrictedApplication = Path.Combine(@".\", c.Abbreviation);
                    c.IdSchool = Commons.IdSchool;
                    c.UriWebApp = ""; // ???? decide what to put here ????
                    query = "UPDATE Classes" +
                        " SET" +
                        " idClass=" + c.IdClass + "" +
                        ",idSchoolYear=" + SqlString(c.SchoolYear) + "" +
                        ",idSchool=" + SqlString(c.IdSchool) + "" +
                        ",abbreviation=" + SqlString(c.Abbreviation) + "" +
                        ",desc=" + SqlString(c.Description) + "" +
                        ",uriWebApp=" + SqlString(c.UriWebApp) + "" +
                        ",pathRestrictedApplication=" + SqlString(c.PathRestrictedApplication) + "" +
                        " WHERE idClass=" + c.IdClass +
                        ";";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    classCount++;
                    // rename every student of the previous class
                    // according to the names found in the pictures' filenames
                    RenameStudentsNamesAndManagePictures(c, cmd);
                    // change the paths of the images to match the new names
                    ChangeImagesPath(c, cmd);
                    // randomly change all grades 
                    RandomizeGrades(cmd);

                    // make example start links 
                    // !!!! TODO insert demo startlinks in the database 
                    //int IdStartLink = NextKey("Classes_StartLinks", "IdStartLink");
                    //query = "INSERT INTO Classes_StartLinks" +
                    //    "(idStartLink, idClass, startLink, desc)" +
                    //    " Values (" + IdStartLink + "," +
                    //    c.IdClass + "," +
                    //    SqlString(@"https://web.spaggiari.eu/home/app/default/login.php?custcode=FOIP0004") + "," +
                    //    SqlString("Registro di classe") +
                    //    ");";
                    //cmd.CommandText = query;
                }
                // compact the database 
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();
            }
        }
        private abstract void RandomizeGrades(DbCommand cmd)
        {
            DbDataReader dRead;
            cmd.CommandText = "SELECT * FROM Grades" +
                ";";
            dRead = cmd.ExecuteReader();
            Random rnd = new Random();
            while (dRead.Read())
            {
                double? grade = Safe.Double(dRead["value"]);
                int? id = Safe.Int(dRead["IdGrade"]);
                // add to the grade a random delta between -10 and +10 
                if (grade > 0)
                {
                    grade = grade + rnd.NextDouble() * 20 - 10;
                    if (grade < 10) grade = 10;
                    if (grade > 100) grade = 100;
                }
                else
                    grade = 0;
                SaveGradeValue(id, grade);
            }
            cmd.Dispose();
        }
        private abstract void RenameStudentsNamesAndManagePictures(Class Class, DbCommand cmd)
        {
            // get the "previous" students from database 
            List<Student> StudentsInClass = GetStudentsOfClass(Class.IdClass, cmd);

            // rename the students' names according to the names found in the image files 
            string[] OriginalDemoStudentPictures = Directory.GetFiles(Path.Combine(Commons.PathImages, "DemoPictures\\"));
            // start assigning the names from a random image
            Random rnd = new Random();

            int pictureIndex;
            string lastName;
            string firstName;
            // copy the students "photos" taking the name of the student from the name of the file 
            foreach (Student s in StudentsInClass)
            {
                do
                {   // avoid the same name and picture for different students 
                    pictureIndex = rnd.Next(0, OriginalDemoStudentPictures.Length - 1);
                    string justFileName = Path.GetFileName(OriginalDemoStudentPictures[pictureIndex]);
                    string fileWithNoExtension = justFileName.Substring(0, justFileName.LastIndexOf('.'));
                    string[] wordsInFileName = (Path.GetFileName(fileWithNoExtension)).Split(' ');
                    lastName = "";
                    firstName = "";
                    foreach (string word in wordsInFileName)
                    {
                        // last name in picture filename must be upper case 
                        if (word == word.ToUpper())
                        {
                            lastName += " " + word;
                        }
                        else
                        {
                            firstName += " " + word;
                        }
                    }
                    lastName = lastName.Trim();
                    firstName = firstName.Trim();
                } while (isDuplicate(lastName, firstName, StudentsInClass));
                s.LastName = lastName;
                s.FirstName = firstName;
                s.BirthDate = null;
                s.BirthPlace = null;
                s.ClassAbbreviation = "";
                s.Email = "";
                s.IdClass = 0;
                s.ArithmeticMean = 0;
                s.RegisterNumber = null;
                s.Residence = null;
                s.RevengeFactorCounter = 0;
                s.Origin = null;
                s.SchoolYear = null;
                s.Sum = 0;
                UpdateStudent(s, cmd);
                // save the image with standard name in the outFolder of the demo class
                string fileExtension = Path.GetExtension(OriginalDemoStudentPictures[pictureIndex]);
                string outFolder = Path.Combine(Commons.PathImages, Class.SchoolYear + "_" + Class.Abbreviation + "\\");
                string filename = s.LastName + "_" + s.FirstName + "_" + Class.Abbreviation + Class.SchoolYear + fileExtension;
                if (!Directory.Exists(outFolder))
                {
                    Directory.CreateDirectory(outFolder);
                }
                if (File.Exists(outFolder + filename))
                {
                    File.Delete(outFolder + filename);
                }
                // save student pictures' paths in table StudentsPhotos
                string relativeOutPathAndFile = Path.Combine(Class.SchoolYear + "_" + Class.Abbreviation, filename);
                string absoluteOutPathAndFile = Path.Combine(Commons.PathImages, relativeOutPathAndFile);
                File.Copy(OriginalDemoStudentPictures[pictureIndex], absoluteOutPathAndFile);
                int? idImage = SaveDemoStudentPhotoPath(relativeOutPathAndFile, cmd);
                AddLinkPhotoToStudent(s.IdStudent, idImage, Class.SchoolYear, cmd);
                if (++pictureIndex >= OriginalDemoStudentPictures.Length)
                    pictureIndex = 0;
            }
            // copy all the lessons images files
            string query = "SELECT Images.imagePath, Classes.pathRestrictedApplication" +
            " FROM Images" +
                " JOIN Lessons_Images ON Lessons_Images.idImage=Images.idImage" +
                " JOIN Lessons ON Lessons_Images.idLesson=Lessons.idLesson" +
                " JOIN Classes ON Classes.idClass=Lessons.idClass" +
                " WHERE Lessons.idClass=" + Class.IdClass +
                ";";
            cmd.CommandText = query;
            DbDataReader dReader = cmd.ExecuteReader();
            while (dReader.Read())
            {
                string finalPart = (string)dReader["imagePath"];
                string originalPathAndFile = Path.Combine(Commons.PathImages, finalPart);
                string partToBeReplaced = finalPart.Substring(0, finalPart.IndexOf("\\"));
                string destinationPathAndFile = originalPathAndFile.Replace(partToBeReplaced, Class.SchoolYear + "_" + Class.Abbreviation);
                string destinationFolder = Path.GetDirectoryName(destinationPathAndFile);
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }
                if (!File.Exists(destinationPathAndFile) ||
                    File.GetLastWriteTime(destinationPathAndFile) < File.GetLastWriteTime(originalPathAndFile))
                    // destination file not existing or older
                    try
                    {
                        File.Copy(originalPathAndFile, destinationPathAndFile);
                    }
                    catch (Exception ex)
                    {
                        Console.Beep();
                    }
            }
            dReader.Close();
        }
        private abstract void AddLinkPhotoToStudent(int? idStudent, int? idStudentsPhoto, string schoolYear, DbCommand cmd)
        {
            cmd.CommandText = "";
            cmd.CommandText = "INSERT INTO StudentsPhotos_Students" +
            " (idStudentsPhoto, idStudent, idSchoolYear)" +
                "Values(" + SqlInt(idStudentsPhoto) + "," + SqlInt(idStudent) + "," + SqlString(schoolYear) + ")" +
            ";";
            cmd.ExecuteNonQuery();
        }
        private abstract bool isDuplicate(string lastName, string firstName, List<Student> StudentsInClass)
        {
            bool found = false;
            foreach (Student s in StudentsInClass)
            {
                if (s.FirstName == firstName && s.LastName == lastName)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }
}
using System.Data.Common;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract object ReadFirstRowFirstField(string Table)
        {
            object r;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + Table +
                    " LIMIT 1" +
                    ";";
                r = cmd.ExecuteScalar();
            }
            return r;
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract Grade GetGrade(int? IdGrade)
        {
            Grade g = null;
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT  * FROM Grades" +
                    " WHERE Grades.idGrade=" + IdGrade.ToString() +
                    ";";
                dRead = cmd.ExecuteReader();
                dRead.Read();
                g = GetGradeFromRow(dRead);
                dRead.Dispose();
                cmd.Dispose();
            }
            return g;
        }
         internal abstract void SaveMacroGrade(int? IdStudent, int? IdParent,
            double Grade, double Weight, string IdSchoolYear,
            string IdSchoolSubject)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // creazione del macrovoto nella tabella dei voti  
                cmd.CommandText = "UPDATE Grades " +
                    "SET IdStudent=" + SqlInt(IdStudent) +
                    ",value=" + SqlDouble(Grade) +
                    ",weight=" + SqlDouble(Weight) +
                    ",idSchoolYear='" + IdSchoolYear + "'" +
                    ",idSchoolSubject='" + IdSchoolSubject +
                    "',timestamp ='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':') + "' " +
                    "WHERE idGrade = " + IdParent +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void GetGradeAndStudentFromIdGrade(ref Grade Grade, ref Student Student)
        {
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Grades.*,Students.* FROM Grades" +
                    " JOIN Students ON Grades.idStudent = Students.idStudent" +
                    " WHERE Grades.idGrade=" + Grade.IdGrade.ToString() +
                    ";";
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Grade = GetGradeFromRow(dRead);
                    Student = GetStudentFromRow(dRead);
                    break; // just the first! 
                }
                dRead.Dispose();
                cmd.Dispose();
            }
        }
         internal abstract double GetDefaultWeightOfGradeType(string IdGradeType)
        {
            double d;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT defaultWeight " +
                    "FROM GradeTypes " +
                    "WHERE idGradeType='" + IdGradeType + "'; ";
                d = (double)cmd.ExecuteScalar();
                cmd.Dispose();
            }
            return d;
        }
        /// <summary>
        /// Gets all the grades of a students of a specified IdGradeType that are the sons 
        /// of another grade which has value greater than zero
        /// </summary>
        /// <param Name="IdStudent"></param> student's Id
         internal abstract DataTable GetMicroGradesOfStudentWithMacroOpen(int? IdStudent, string IdSchoolYear,
            string IdGradeType, string IdSchoolSubject)
        {
            using (DbConnection conn = Connect())
            {
                // find the macro grade type of the micro grade
                // TODO take it from a Grade passed as parameter 
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idGradeTypeParent " +
                    "FROM GradeTypes " +
                    "WHERE idGradeType='" + IdGradeType + "'; ";
                string idGradeTypeParent = (string)cmd.ExecuteScalar();

                string query = "SELECT datetime(Grades.timestamp),Questions.text,Grades.value" +
                    ",Grades.weight,Grades.IdGrade,Grades.idGradeParent,Grades.cncFactor" +
                    ",Questions.IdQuestion,Questions.IdQuestionType,Grades.IdSchoolSubject" +
                    ",Questions.IdTopic,Questions.Image,Questions.Duration,Questions.Difficulty" +
                    ",Questions.*" +
                    ",Grades.*" +
                    " FROM Grades" +
                    " JOIN Grades AS Parents" +
                    " ON Grades.idGradeParent=Parents.idGrade" +
                    " LEFT JOIN Questions" +
                    " ON Grades.idQuestion=Questions.idQuestion" +
                    " WHERE Grades.idStudent =" + IdStudent +
                    " AND Grades.idSchoolYear='" + IdSchoolYear + "'" +
                    " AND Grades.idGradeType = '" + IdGradeType + "'" +
                    " AND Grades.idSchoolSubject = '" + IdSchoolSubject + "'" +
                    " AND Parents.idGradeType = '" + idGradeTypeParent + "'" +
                    " AND Grades.idGradeParent = Parents.idGrade" +
                    " AND (Parents.value = 0 OR Parents.value is NULL)" +
                    " ORDER BY Grades.timestamp;";

                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("OpenMicroGrades");
                DAdapt.Fill(DSet);
                DAdapt.Dispose();
                DSet.Dispose();
                return DSet.Tables[0];
            }
        }
         internal abstract DataTable GetSubGradesOfGrade(int? IdGrade)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT datetime(Grades.timestamp),Questions.text,Grades.value," +
                    " Grades.weight,Grades.cncFactor,Grades.idGradeParent" +
                    " FROM Grades" +
                    " JOIN Questions ON Grades.idQuestion=Questions.idQuestion" +
                    " WHERE Grades.idGradeParent =" + IdGrade +
                    " ORDER BY Grades.timestamp;";

                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("OpenMicroGrades");
                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
         internal abstract Student GetWeightedAveragesOfStudent(Student Student, string stringKey1, string stringKey2, DateTime value1, DateTime value2)
        {

            //List<StudentAndGrade> l = new List<StudentAndGrade>(); 

            //using (DbConnection conn = Connect())
            //{
            //    DataAdapter dAdapt;
            //    DataSet dSet = new DataSet();

            //    string query = "SELECT Grades.idGrade, Students.idStudent,lastName,firstName," +
            //" SUM(Grades.value * Grades.weight)/SUM(Grades.weight) AS 'Weighted average'" +
            //// weighted RMS (Root Mean Square) as defined here:   
            //// https://stackoverflow.com/questions/10947180/weighted-standard-deviation-in-sql-server-without-aggregation-error
            //// !!!! fix the calculation of weighted RMS 
            ////",SQRT( SUM(Grades.weight * SQUARE(Grades.value)) / SUM(Grades.weight) - SQUARE(SUM(Grades.weight * Grades.value) / SUM(Grades.weight)) )  AS 'Weighted RMS'" +
            //",COUNT() AS 'Grades Count'" +
            //" FROM Grades" +
            //" JOIN Students" +
            //" ON Students.idStudent=Grades.idStudent" +
            //" JOIN Classes_Students" +
            //" ON Classes_Students.idStudent=Students.idStudent" +
            //" WHERE Classes_Students.idClass =" + Class.IdClass +
            //" AND Grades.idSchoolYear='" + Class.SchoolYear + "'" +
            //" AND Grades.idGradeType = '" + IdGradeType + "'" +
            //" AND Grades.idSchoolSubject = '" + IdSchoolSubject + "'" +
            //" AND Grades.Value > 0" +
            //" AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
            //" GROUP BY Students.idStudent" +
            //" ORDER BY 'Weighted average';";
            //    //" ORDER BY lastName, firstName, Students.idStudent;";
            //    dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
            //    dSet = new DataSet("GetUnfixedGradesInTheYear");
            //    dAdapt.Fill(dSet);
            //    t = dSet.Tables[0];

            //    dSet.Dispose();
            //    dAdapt.Dispose();
            //}
            return Student;
        }
         internal abstract DataTable GetGradesOfClass(Class Class,
             string IdGradeType, string IdSchoolSubject,
             DateTime DateFrom, DateTime DateTo)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT Grades.idGrade,datetime(Grades.timeStamp),Students.idStudent," +
                "lastName,firstName," +
                "Grades.value AS 'grade',Grades.weight," +
                "Grades.idGradeParent" +
                " FROM Grades" +
                " JOIN Students" +
                " ON Students.idStudent=Grades.idStudent" +
                " JOIN Classes_Students" +
                " ON Classes_Students.idStudent=Students.idStudent" +
                " WHERE Classes_Students.idClass =" + Class.IdClass +
                " AND (Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                " OR Grades.idSchoolYear='" + Class.SchoolYear.Replace("-", "") + "'" +
                " )" +
                " AND Grades.idGradeType = '" + IdGradeType + "'" +
                " AND Grades.idSchoolSubject = '" + IdSchoolSubject + "'" +
                " AND Grades.Value > 0" +
                " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
                " ORDER BY lastName, firstName, Students.idStudent, Grades.timestamp Desc;";

                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("ClosedMicroGrades");

                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
        /// <summary>
        /// Gets the number of microquestions that haven't yet a global grade
        /// </summary>
        /// <param Name="id"></param>
        /// <returns></returns>
         internal abstract List<Grade> CountNonClosedMicroGrades(Class Class, GradeType GradeType)
        {
            DbDataReader dRead;
            DbCommand cmd;
            List<Grade> ls = new List<Grade>();
            using (DbConnection conn = Connect())
            {
                string query = "SELECT Grades.idStudent, Count(*) as nGrades FROM Grades," +
                    "Grades AS Parents,Classes_Students" +
                    " WHERE Classes_Students.idStudent = Grades.idStudent" +
                    " AND Classes_Students.idClass =" + Class.IdClass.ToString() +
                    " AND Grades.idGradeType = '" + GradeType.IdGradeType + "'" +
                    " AND Parents.idGradeType = '" + GradeType.IdGradeTypeParent + "'" +
                    " AND Grades.idGradeParent = Parents.idGrade" +
                    " AND Parents.Value is null or Parents.Value = 0" +
                    " GROUP BY Grades.idStudent;";
                cmd = new SQLiteCommand(query);
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Grade g = new Grade();
                    g.IdStudent = (int)dRead["idStudent"];
                    g.DummyInt = (int)dRead["nGrades"];
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return ls;
        }
         internal abstract Grade LastOpenGradeOfStudent(Student Student, string IdSchoolYear,
            SchoolSubject SchoolSubject, string IdGradeType)
        {
            DbDataReader dRead;
            DbCommand cmd;
            Grade g = new Grade();
            using (DbConnection conn = Connect())
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Grades.* FROM Grades" +
                    " WHERE Grades.idStudent=" + Student.IdStudent.ToString() +
                    " AND Grades.idSchoolSubject='" + SchoolSubject.IdSchoolSubject + "'" +
                    " AND Grades.idGradeType='" + IdGradeType + "'" +
                    " AND Grades.idSchoolYear='" + IdSchoolYear + "'" +
                    " AND (Grades.value=0 OR Grades.value IS NULL)" +
                    " ORDER BY Grades.idGrade DESC;";
                // !!!! changed name from LastGradeOfStudent and query changed,that was WRONG, this change must be brought to every branch !!!!
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    g = GetGradeFromRow(dRead);
                    break; // just the first! 
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return g;
        }
         internal abstract void CloneGrade(DataRow Riga)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // mette peso 0 nel voto precedente  
                cmd.CommandText = "UPDATE Grades " +
                    " SET weight=0" +
                    " WHERE idGrade = " + Riga["idGrade"] +
                    ";";
                cmd.ExecuteNonQuery();
                // crea un nuovo voto copiato dalla riga passata
                int codiceVoto = NextKey("Grades", "idGrade");

                // aggiunge il voto copiato dalla riga passata
                cmd.CommandText = "INSERT INTO Grades " +
                "(idGrade,idStudent,value,weight,cncFactor,idGradeType,idGradeParent,idSchoolYear" +
                ",timestamp,idQuestion) " +
                "Values (" + codiceVoto + "," + Riga["idStudent"] + "," +
                SqlDouble(Riga["value"]) + "," +
                SqlDouble(Riga["weight"]) + ",'" +
                SqlDouble(Riga["cncFactor"]) + ",'" +
                Riga["idGradeType"] + "'," + Riga["idGradeParent"] + ",'" +
                Riga["idSchoolYear"] + "','" +
                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':') +
                //"'," + riga["idQuestion"].ToString() + "" +
                "',NULL" +
                ");";
                cmd.ExecuteNonQuery();
                // aggiusta tutte le domande figlie
                cmd.CommandText = "UPDATE Grades " +
                    " SET idGradeParent=" + codiceVoto +
                    " WHERE idGradeParent = " + Riga["idGrade"] +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract List<GradeType> GetListGradeTypes()
        {
            List<GradeType> lg = new List<GradeType>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM GradeTypes;";
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    GradeType gt = GetGradeTypeFromRow(dRead);
                    lg.Add(gt);
                }
                dRead.Dispose();
                cmd.Dispose();
                return lg;
            }
        }
         internal abstract void DeleteValueOfGrade(int IdGrade)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE Grades" +
                           " Set" +
                           " value=null" +
                           " WHERE IdGrade=" + IdGrade +
                           ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract GradeType GetGradeTypeFromRow(DbDataReader Row)
        {
            if (Row.HasRows)
            {
                GradeType gt = new GradeType();
                gt.IdGradeType = (string)Row["idGradeType"];
                gt.IdGradeTypeParent = Safe.String(Row["IdGradeTypeParent"]);
                gt.IdGradeCategory = (string)Row["IdGradeCategory"];
                gt.Name = (string)Row["Name"];
                gt.DefaultWeight = (double)Row["DefaultWeight"];
                gt.Desc = (string)Row["Desc"];
                return gt;
            }
            return null;
        }
         internal abstract GradeType GetGradeType(string IdGradeType)
        {
            GradeType gt = null;
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM GradeTypes";
                cmd.CommandText += " WHERE idGradeType ='" + IdGradeType + "'";
                cmd.CommandText += ";";
                dRead = cmd.ExecuteReader();
                dRead.Read();
                gt = GetGradeTypeFromRow(dRead);
                dRead.Dispose();
                cmd.Dispose();
            }
            return gt;
        }
        private abstract void SaveGradeValue(int? id, double? grade)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Grades" +
                " SET value=" + SqlDouble(grade) +
                " WHERE idGrade=" + id +
                ";";
                cmd.ExecuteNonQuery();
            }
        }
         internal abstract void EraseGrade(int? KeyGrade)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Grades" +
                    " WHERE idGrade=" + KeyGrade +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract Grade GetGradeFromRow(DbDataReader Row)
        {
            Grade g = new Grade();
            g.IdGrade = (int)Row["idGrade"];
            g.IdGradeParent = Safe.Int(Row["idGradeParent"]);
            g.IdStudent = Safe.Int(Row["idStudent"]);
            g.IdGradeType = Safe.String(Row["IdGradeType"]);
            g.IdSchoolSubject = Safe.String(Row["IdSchoolSubject"]);
            //g.IdGradeTypeParent = Safe.SafeString(Row["idGradeTypeParent"]);
            g.IdQuestion = Safe.Int(Row["idQuestion"]);
            g.Timestamp = (DateTime)Row["timestamp"];
            g.Value = Safe.Double(Row["value"]);
            g.Weight = Safe.Double(Row["weight"]);
            g.CncFactor = Safe.Double(Row["cncFactor"]);
            g.IdSchoolYear = Safe.String(Row["idSchoolYear"]);
            //g.DummyInt = (int)Row["dummyInt"]; 
            return g;
        }
         internal abstract DataTable GetGradesOfStudent(Student Student, string SchoolYear, string IdGradeType, string IdSchoolSubject, DateTime DateFrom, DateTime DateTo)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT DISTINCT Grades.idGrade,datetime(Grades.timeStamp)," +
                "Grades.value AS 'grade', Grades.weight," +
                "Questions.text,lastName,firstName," +
                " Grades.idGradeParent" +
                " FROM Grades" +
                " JOIN Students" +
                " ON Students.idStudent=Grades.idStudent" +
                " JOIN Classes_Students" +
                " ON Classes_Students.idStudent=Students.idStudent" +
                " LEFT JOIN Questions" +
                " ON Grades.idQuestion=Questions.idQuestion" +
                " WHERE Students.idStudent=" + Student.IdStudent +
                " AND (Grades.idSchoolYear='" + SchoolYear + "'" +
                " OR Grades.idSchoolYear='" + SchoolYear.Replace("-", "") + "'" +
                ")" +
                " AND Grades.idGradeType='" + IdGradeType + "'" +
                " AND Grades.idSchoolSubject='" + IdSchoolSubject + "'" +
                " AND Grades.Value > 0" +
                " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
                " ORDER BY lastName, firstName, Students.idStudent, Grades.timestamp Desc;";

                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("ClosedMicroGrades");

                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
         internal abstract object GetGradesWeightsOfStudentOnOpenGrades(Student currentStudent, string stringKey1, string stringKey2, DateTime value1, DateTime value2)
        {
            throw new NotImplementedException();
        }
         internal abstract DataTable GetWeightedAveragesOfClassByGradesFraction(Class Class,
            string IdGradeType, string IdSchoolSubject, DateTime DateFrom, DateTime DateTo)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT Grades.idGrade,Students.idStudent,lastName,firstName" +
                ",SUM(weight)/100 AS 'GradesFraction', 1 - SUM(weight)/100 AS LeftToCloseAssesments" +
                ",COUNT() AS 'GradesCount'" +
                " FROM Classes_Students" +
                " LEFT JOIN Grades ON Students.idStudent=Grades.idStudent" +
                " JOIN Students ON Classes_Students.idStudent=Students.idStudent" +
                " WHERE Classes_Students.idClass =" + Class.IdClass +
                " AND (Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                " OR Grades.idSchoolYear='" + Class.SchoolYear.Replace("-", "") + "'" + // TEMPORARY line for compatibility
                ")" +
                " AND (Grades.idGradeType='" + IdGradeType + "'" +
                " OR Grades.idGradeType IS NULL)" +
                " AND Grades.idSchoolSubject='" + IdSchoolSubject + "'" +
                " AND Grades.value IS NOT NULL AND Grades.value <> 0" +
                " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
                " GROUP BY Students.idStudent" +
                " ORDER BY GradesFraction ASC, lastName, firstName, Students.idStudent;";
                // !!!! TODO change the query to include at first rows also those students that have no grades !!!! 
                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("ClosedMicroGrades");

                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
         internal abstract DataTable GetGradesWeightedAveragesOfClassByAverage(Class Class, string IdGradeType,
            string IdSchoolSubject, DateTime DateFrom, DateTime DateTo)
        {
            DataTable t;
            //List<StudentAndGrade> l = new List<StudentAndGrade>(); 

            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapt;
                DataSet dSet = new DataSet();

                string query = "SELECT Grades.idGrade, Students.idStudent,lastName,firstName," +
            " SUM(Grades.value * Grades.weight)/SUM(Grades.weight) AS 'Weighted average'" +
            // weighted RMS (Root Mean Square) as defined here: 
            // https://stackoverflow.com/questions/10947180/weighted-standard-deviation-in-sql-server-without-aggregation-error
            // !!!! fix the calculation of weighted RMS 
            //",SQRT( SUM(Grades.weight * SQUARE(Grades.value)) / SUM(Grades.weight) - SQUARE(SUM(Grades.weight * Grades.value) / SUM(Grades.weight)) )  AS 'Weighted RMS'" +
            ",COUNT() AS 'Grades Count'" +
            " FROM Grades" +
            " JOIN Students" +
            " ON Students.idStudent=Grades.idStudent" +
            " JOIN Classes_Students" +
            " ON Classes_Students.idStudent=Students.idStudent" +
            " WHERE Classes_Students.idClass =" + Class.IdClass +
            " AND (Grades.idSchoolYear='" + Class.SchoolYear + "'" +
            " OR Grades.idSchoolYear='" + Class.SchoolYear.Replace("-", "") + "'" +
            ")" +
            " AND Grades.idGradeType = '" + IdGradeType + "'" +
            " AND Grades.idSchoolSubject = '" + IdSchoolSubject + "'" +
            " AND Grades.Value > 0" +
            " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
            " GROUP BY Students.idStudent" +
            " ORDER BY 'Weighted average';";
                //" ORDER BY lastName, firstName, Students.idStudent;";
                dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                dSet = new DataSet("GetUnfixedGradesInTheYear");
                dAdapt.Fill(dSet);
                t = dSet.Tables[0];

                dSet.Dispose();
                dAdapt.Dispose();
            }
            return t;
        }
         internal abstract List<StudentAndGrade> GetListGradesWeightedAveragesOfClassByName(Class Class, string IdGradeType,
            string IdSchoolSubject, DateTime DateFrom, DateTime DateTo)
        {
            //DataTable t;
            List<StudentAndGrade> l = new List<StudentAndGrade>();

            using (DbConnection conn = Connect())
            {
                string query = "SELECT Grades.idGrade, Students.idStudent,lastName,firstName," +
                " SUM(Grades.value * Grades.weight)/SUM(Grades.weight) AS 'Weighted average'" +
                // weighted RMS (Root Mean Square) as defined here: 
                // https://stackoverflow.com/questions/10947180/weighted-standard-deviation-in-sql-server-without-aggregation-error
                // !!!! fix the calculation of weighted RMS 
                //",SQRT( SUM(Grades.weight * SQUARE(Grades.value)) / SUM(Grades.weight) - SQUARE(SUM(Grades.weight * Grades.value) / SUM(Grades.weight)) )  AS 'Weighted RMS'" +
                ",COUNT() AS 'Grades Count'" +
                " FROM Grades" +
                " JOIN Students" +
                " ON Students.idStudent=Grades.idStudent" +
                " JOIN Classes_Students" +
                " ON Classes_Students.idStudent=Students.idStudent" +
                " WHERE Classes_Students.idClass =" + Class.IdClass +
                " AND Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                " AND Grades.idGradeType = '" + IdGradeType + "'" +
                " AND Grades.idSchoolSubject = '" + IdSchoolSubject + "'" +
                " AND Grades.Value > 0" +
                " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
                " GROUP BY Students.idStudent" +
                " ORDER BY lastName, firstName, Students.idStudent;";
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    StudentAndGrade sg = new StudentAndGrade();

                    sg.Student.IdStudent = Safe.Int(dRead["idStudent"]);
                    sg.Student.LastName = Safe.String(dRead["lastName"]);
                    sg.Student.FirstName = Safe.String(dRead["firstName"]);
                    sg.Grade.IdGrade = Safe.Int(dRead["idGrade"]);
                    sg.WeightedAverage = Safe.Double(dRead["Weighted average"]);
                    //sg.WeightedRms = Safe.Double(dRead["Weighted RMS"]); // when RMS calculation in fixed, reenact this statement 
                    sg.GradesCount = Safe.Int(dRead["Grades Count"]);

                    l.Add(sg);
                }
                dRead.Dispose();
            }
            return l;
        }
         internal abstract DataTable GetUnfixedGrades(Student Student, string IdSchoolSubject,
            double Threshold)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapt;
                DataSet dSet = new DataSet();
                string query = "SELECT Grades.IdGrade,Grades.idStudent,Grades.value,Grades.timestamp,Grades.isFixed," +
                    "Grades.idGradeType,Grades.idQuestion,Questions.text,Questions.*,Grades.*" +
                    " FROM Grades" +
                    " JOIN Questions ON Grades.idQuestion=Questions.idQuestion" +
                    " WHERE idStudent=" + Student.IdStudent +
                    " AND Grades.value<" + Threshold.ToString() +
                    " AND (Grades.isFixed=0 OR Grades.isFixed is NULL)";
                if (IdSchoolSubject != "")
                    query += " AND Grades.idSchoolSubject='" + IdSchoolSubject + "'";
                query += ";";
                dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                dSet = new DataSet("GetUnfixedGradesInTheYear");
                dAdapt.Fill(dSet);
                t = dSet.Tables[0];

                dSet.Dispose();
                dAdapt.Dispose();
            }
            return t;
        }
        /// <summary>
        /// Gets all the grades of a students of a specified IdGradeType that are the sons 
        /// of another grade which has value NOT null AND NOT equal to zero
        /// </summary>
        /// <param name="IdStudent"></param>
        /// <param name="IdSchoolYear"></param>
        /// <param name="IdGradeType"></param>
        /// <param name="IdSchoolSubject"></param>
        /// <returns></returns>
         internal abstract DataTable GetMacroGradesOfStudentClosed(int? IdStudent, string IdSchoolYear,
            string IdGradeType, string IdSchoolSubject)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT idGrade, idStudent, value, idSchoolSubject," +
                    "weight, cncFactor, idSchoolYear, datetime(timestamp), idGradeType, " +
                    "idGradeParent,idQuestion" +
                " FROM Grades" +
                " WHERE Grades.idStudent =" + IdStudent +
                " AND Grades.idSchoolYear='" + IdSchoolYear + "'" +
                " AND Grades.idGradeType = '" + IdGradeType + "'" +
                " AND Grades.idSchoolSubject = '" + IdSchoolSubject + "'" +
                " AND Grades.Value > 0" +
                " ORDER BY datetime(Grades.timestamp) Desc;";

                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("ClosedMicroGrades");

                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
         internal abstract int CreateMacroGrade(ref Grade Grade, Student Student, string IdMicroGradeType)
        {
            int key = NextKey("Grades", "idGrade");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // find the type of the macro grade of this micrograde
                cmd.CommandText = "SELECT IdGradeTypeParent" +
                    " FROM GradeTypes WHERE idGradetype='" + IdMicroGradeType + "';";
                string IdMacroGrade = (string)cmd.ExecuteScalar();

                // Get the Default Weight of that Grade Type
                cmd.CommandText = "SELECT defaultWeight " +
                    "FROM GradeTypes " +
                    "WHERE idGradeType='" + IdMicroGradeType + "'; ";
                double weight = (double)cmd.ExecuteScalar();

                // add macrograde
                cmd.CommandText = "INSERT INTO Grades " +
                "(idGrade,idStudent,idGradeType,weight,cncFactor,idSchoolYear,timestamp,idSchoolSubject) " +
                "Values (" + key + "," + Student.IdStudent +
                ",'" + IdMacroGrade + "'" +
                "," + SqlDouble(weight) + "" +
                ",0" +
                ",'" + Grade.IdSchoolYear + "'," +
                SqlDate(System.DateTime.Now) + "" +
                ",'" + Grade.IdSchoolSubject +
                "');";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return key;
        }
         internal abstract int? SaveMicroGrade(Grade Grade)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // create a new micro assessment in grades table
                if (Grade == null || Grade.IdGrade == null || Grade.IdGrade == 0)
                {
                    Grade.IdGrade = NextKey("Grades", "idGrade");
                    cmd.CommandText = "INSERT INTO Grades " +
                    "(idGrade, idGradeType, idGradeParent, idStudent, value, weight, " +
                    "cncFactor,idSchoolYear, timestamp, idQuestion,idSchoolSubject) " +
                    "Values (" + Grade.IdGrade +
                    "," + SqlString(Grade.IdGradeType) + "" +
                    "," + SqlInt(Grade.IdGradeParent.ToString()) + "" +
                    "," + Grade.IdStudent + "" +
                    "," + SqlDouble(Grade.Value) + "" +
                    "," + SqlDouble(Grade.Weight) + "" +
                    "," + SqlDouble(Grade.CncFactor) + "" +
                    "," + SqlString(Grade.IdSchoolYear) + "" +
                    //"," + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':') + "" +
                    "," + SqlDate(System.DateTime.Now) + "" +
                    "," + SqlInt(Grade.IdQuestion.ToString()) + "" +
                    ",'" + Grade.IdSchoolSubject + "'" +
                    ");";
                }
                else
                {
                    cmd.CommandText = "UPDATE Grades " +
                    "SET" +
                    " idGrade=" + SqlInt(Grade.IdGrade.ToString()) + "" +
                    ",idGradeType=" + SqlString(Grade.IdGradeType) + "" +
                    ",idGradeParent=" + SqlInt(Grade.IdGradeParent.ToString()) + "" +
                    ",idStudent=" + SqlInt(Grade.IdStudent.ToString()) + "" +
                    ",idSchoolYear=" + SqlString(Grade.IdSchoolYear) + "" +
                    ",timestamp=" + SqlDate((DateTime)Grade.Timestamp) + "" +
                    ",idQuestion=" + SqlInt(Grade.IdQuestion.ToString()) + "" +
                    ",idSchoolSubject=" + SqlString(Grade.IdSchoolSubject) + "" +
                    ",value=" + SqlDouble(Grade.Value) + "" +
                    ",weight=" + SqlDouble(Grade.Weight) + "" +
                    ",cncFactor=" + SqlDouble(Grade.CncFactor) + "" +
                    " WHERE idGrade=" + SqlInt(Grade.IdGrade.ToString()) +
                    ";";
                }

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return Grade.IdGrade;
        }
         internal abstract object GetGradesWeightedAveragesOfStudent(Student currentStudent,
            string stringKey1, string stringKey2, DateTime value1, DateTime value2)
        {
            throw new NotImplementedException();
        }
         internal abstract List<Couple> GetGradesOldestInClass(Class Class,
            GradeType GradeType, SchoolSubject SchoolSubject)
        {
            List<Couple> couples = new List<Couple>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query;
                query = "SELECT Classes_Students.idStudent" +
                ",MAX(timestamp) AS InstantLastQuestion" +
                " FROM Classes_Students LEFT JOIN Grades" +
                " ON Classes_Students.idStudent = Grades.idStudent" +
                " WHERE Classes_Students.idClass =" + Class.IdClass +
                " AND Grades.idGradeType = '" + GradeType.IdGradeType + "'" +
                " AND Grades.idSchoolSubject='" + SchoolSubject.IdSchoolSubject + "'" +
                " AND Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                " OR Grades.idGrade IS NULL" + // takes those that haven't any grades
                " GROUP BY Classes_Students.idStudent" +
                " ORDER BY InstantLastQuestion DESC;"; // ???? DESC o no 
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Couple c = new Couple();
                    // we give back also the nulls, as Nows
                    DateTime now = System.DateTime.Now;
                    c.Key = (int)dRead["IdStudent"];
                    if (!dRead.IsDBNull(1))
                        c.Value = Safe.DateTime(dRead["InstantLastQuestion"]);
                    else
                        c.Value = now;
                    couples.Add(c);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return couples;
        }
         internal abstract DataTable GetGradesWeightsOfClassOnOpenGrades(Class Class,
            string IdGradeType, string IdSchoolSubject, DateTime DateFrom, DateTime DateTo)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                // find the macro grade type of the micro grade
                // TODO take it from a Grade passed as parameter 
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idGradeTypeParent " +
                    "FROM GradeTypes " +
                    "WHERE idGradeType='" + IdGradeType + "'; ";
                string idGradeTypeParent = (string)cmd.ExecuteScalar();

                string query = "SELECT Grades.idGrade,Students.idStudent,lastName,firstName" +
                ",SUM(Grades.weight)/100 AS 'GradesFraction', 1 - SUM(Grades.weight)/100 AS LeftToCloseAssesments" +
                ",COUNT() AS 'GradesCount'" +
                " FROM Grades, Grades AS Parents " +
                " JOIN Classes_Students ON Students.idStudent=Grades.idStudent" +
                " JOIN Students ON Classes_Students.idStudent=Students.idStudent" +
                " WHERE Classes_Students.idClass =" + Class.IdClass +
                " AND (Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                " OR Grades.idSchoolYear='" + Class.SchoolYear.Replace("-", "") + "'" +
                ")" +
                " AND (Grades.idGradeType='" + IdGradeType + "'" +
                " OR Grades.idGradeType IS NULL)" +
                " AND Grades.idSchoolSubject='" + IdSchoolSubject + "'" +
                " AND Grades.value IS NOT NULL AND Grades.value <> 0" +
                " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
                " AND Parents.idGradeType = '" + idGradeTypeParent + "'" +
                " AND Grades.idGradeParent = Parents.idGrade" +
                " AND (Parents.Value is null or Parents.Value = 0)" +
                " AND NOT Students.disabled" +
                " GROUP BY Students.idStudent" +
                " ORDER BY GradesFraction ASC, lastName, firstName, Students.idStudent;";

                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("ClosedMicroGrades");

                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract List<Image> GetAllImagesShownToAClassDuringLessons(Class Class, SchoolSubject Subject,
            DateTime DateStart = default(DateTime), DateTime DateFinish = default(DateTime))
        {
            List<Image> images = new List<Image>();

            DbDataReader dRead;
            DbCommand cmd;
            using (DbConnection conn = Connect())
            {
                cmd = conn.CreateCommand();
                string query;
                query = "SELECT * FROM Images" +
                        " JOIN Lessons_Images ON Images.idImage=Lessons_Images.idImage" +
                        " JOIN Lessons ON Lessons.idLesson=Lessons_Images.idLesson" +
                        " WHERE Lessons.idClass=" + Class.IdClass +
                        " AND Lessons.idSchoolSubject='" + Subject.IdSchoolSubject + "'";
                if (DateStart != default(DateTime) && DateFinish != default(DateTime))
                    query += " AND Lessons.date BETWEEN " +
                    SqlDate(DateStart) + " AND " + SqlDate(DateFinish);
                query += ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Image i = new Image();
                    i.IdImage = (int)dRead["IdImage"];
                    i.Caption = (string)dRead["Caption"];
                    i.RelativePathAndFilename = (string)dRead["ImagePath"];

                    images.Add(i);
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return images;
        }
         internal abstract List<string> GetCaptionsOfThisImage(string FileName)
        {
            List<string> captions = new List<string>();

            DbDataReader dRead;
            DbCommand cmd;
            using (DbConnection conn = Connect())
            {
                cmd = conn.CreateCommand();
                string query;
                query = "SELECT Caption FROM Images" +
                        " WHERE imagePath " + SqlLikeStatement(FileName) + "";
                query += ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    captions.Add((string)dRead["Caption"]);
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return captions;
        }
         internal abstract void EraseStudentsPhoto(int? IdStudent, string SchoolYear)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM StudentsPhotos_Students" +
                    " WHERE idStudent=" + IdStudent +
                    " AND idSchoolYear='" + SchoolYear + "'" +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract string GetFilePhoto(int? IdStudent, string SchoolYear)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT StudentsPhotos.photoPath" +
                    " FROM StudentsPhotos_Students, StudentsPhotos" +
                    " WHERE StudentsPhotos_Students.idStudentsPhoto = StudentsPhotos.idStudentsPhoto";
                if (SchoolYear != null && SchoolYear != "")
                {
                    query += " AND (StudentsPhotos_Students.idSchoolYear='" + SchoolYear + "'" +
                        " OR StudentsPhotos_Students.idSchoolYear = '" + SchoolYear.Replace("-", "") + "'" + // !!!! temporary !!!!
                        ")";
                }
                query += " AND StudentsPhotos_Students.idStudent = " + IdStudent + "; ";
                string NamePath = null;
                try
                {
                    cmd.CommandText = query;
                    NamePath = (string)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {

                }
                cmd.Dispose();
                return NamePath;
            }
        }
        private abstract void ChangeImagesPath(Class Class, DbCommand cmd)
        {
            // find 
            DbDataReader dRead;
            cmd.CommandText = "SELECT Images.idImage, Images.imagePath" +
                " FROM Images" +
                " JOIN Lessons_Images ON Images.idImage=Lessons_Images.idImage" +
                " JOIN Lessons ON Lessons.idLesson = Lessons_Images.idLesson" +
                " WHERE Lessons.idClass=" + Class.IdClass +
                " LIMIT 1" +
            ";";
            dRead = cmd.ExecuteReader();
            dRead.Read();
            string originalPath = Path.GetDirectoryName(Safe.String(dRead["imagePath"]));
            string originalFolder = originalPath.Substring(0, originalPath.IndexOf("\\"));
            dRead.Close();
            string newFolder = Class.SchoolYear + "_" + Class.Abbreviation;

            // replace the folder name in Images path 
            cmd.CommandText = "UPDATE Images" +
                " SET imagePath=REPLACE(Images.imagePath,'" + originalFolder + "','" + newFolder + "')" +
            " FROM Images Img" +
            " JOIN Lessons_Images ON Img.IdImage=Lessons_Images.idImage" +
            " JOIN Lessons ON Lessons.idLesson=Lessons_Images.idLesson" +
            " WHERE Lessons.idClass=" + Class.IdClass +
            ";";
            cmd.ExecuteNonQuery();
        }
        private abstract void SaveImagePath(int? id, string path)
        {
            string query;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd1 = conn.CreateCommand();
                query = "UPDATE Images" +
                " SET imagePath=" + SqlString(path) + "" +
                " WHERE idImage=" + id +
                ";";
                cmd1.CommandText = query;
                cmd1.ExecuteNonQuery();
            }
        }
        private abstract int? SaveDemoStudentPhotoPath(string relativePath, DbCommand cmd)
        {
            int? nextId = null;
            try
            {
                cmd.CommandText = "SELECT MAX(idStudentsPhoto) FROM StudentsPhotos;";
                var firstColumn = cmd.ExecuteScalar();
                if (firstColumn != DBNull.Value)
                {
                    nextId = int.Parse(firstColumn.ToString()) + 1;
                }
                else
                {
                    nextId = 1;
                }
                cmd.CommandText = "INSERT INTO StudentsPhotos" +
                " (idStudentsPhoto, photoPath)" +
                " Values (" + SqlInt(nextId.ToString()) + "," + SqlString(relativePath) + ");";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            }
            return nextId;
        }
         internal abstract void RemoveImageFromLesson(Lesson Lesson, Image Image, bool AlsoEraseImageFile)
        {
            // delete from the link table
            string query;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                query = "DELETE FROM Lessons_Images" +
                    " WHERE idImage=" +
                    Image.IdImage +
                    ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                if (AlsoEraseImageFile)
                {
                    // delete from the Images table 
                    query = "DELETE FROM Images" +
                        " WHERE idImage=" +
                        Image.IdImage +
                        ";";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
            }
        }
         internal abstract void SaveImage(Image Image)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query;
                query = "UPDATE Images" +
                    " SET caption=" + SqlString(Image.Caption) + "" +
                    ", imagePath=" + SqlString(Image.RelativePathAndFilename) + "" +
                    " WHERE idImage=" +
                    Image.IdImage +
                    ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
        }
         internal abstract Image FindImageWithGivenFile(string PathAndFileNameOfImage)
        {
            Image i = new Image();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                DbDataReader dRead;
                string query;
                query = "SELECT * FROM Images" +
                        " WHERE Images.imagePath=" +
                        SqlString(PathAndFileNameOfImage.Remove(0, Commons.PathImages.Length + 1)) +
                        ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                dRead.Read(); // just one record ! 
                if (!dRead.HasRows)
                    return null;
                i.IdImage = (int)dRead["IdImage"];
                i.Caption = (string)dRead["Caption"];
                i.RelativePathAndFilename = (string)dRead["ImagePath"];
                cmd.Dispose();
                dRead.Dispose();
            }
            return i;
        }
        /// <summary>
        /// Creates a new Image in Images and links it to the lesson
        /// If the image has an nextId != 0, it exists and is not created 
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="Lesson"></param>
        /// <returns></returns>
         internal abstract int? LinkOneImage(Image Image, Lesson Lesson)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query;
                if (Image.IdImage == 0)
                {
                    Image.IdImage = NextKey("Images", "IdImage");
                    query = "INSERT INTO Images" +
                    " (idImage, imagePath, caption)" +
                    " Values (" + Image.IdImage + "," +
                    SqlString(Image.RelativePathAndFilename) + "," +
                    SqlString(Image.Caption) + "" +
                    ");";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                query = "INSERT INTO Lessons_Images" +
                    " (idImage, idLesson)" +
                    " Values (" + Image.IdImage + "," + Lesson.IdLesson + "" +
                    ");";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            return Image.IdImage;
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract Lesson GetLessonFromRow(DbDataReader dRead)
        {
            Lesson l = new Lesson();
            l.IdLesson = Safe.Int(dRead["IdLesson"]);
            l.Date = Safe.DateTime(dRead["Date"]);
            l.IdClass = Safe.Int(dRead["IdClass"]);
            l.IdSchoolSubject = Safe.String(dRead["IdSchoolSubject"]);
            l.IdSchoolYear = Safe.String(dRead["IdSchoolYear"]);
            l.Note = Safe.String(dRead["Note"]);

            return l;
        }
         internal abstract int NewLesson(Lesson Lesson)
        {
            int key;
            using (DbConnection conn = Connect())
            {
                key = NextKey("Lessons", "idLesson");
                Lesson.IdLesson = key;
                // add new record to Lessons table
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Lessons" +
                " (idLesson, date, idClass, idSchoolSubject, idSchoolYear, note) " +
                "Values (" +
                "" + Lesson.IdLesson + "" +
                "," + SqlDate(Lesson.Date) + "" +
                "," + Lesson.IdClass + "" +
                ",'" + Lesson.IdSchoolSubject + "'" +
                ",'" + Lesson.IdSchoolYear + "'" +
                "," + SqlString(Lesson.Note) + "" +
                ");";
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            return key;
        }
         internal abstract void SaveLesson(Lesson Lesson)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Lessons" +
                " SET" +
                " date=" + SqlDate(Lesson.Date) + "," +
                " idClass=" + Lesson.IdClass + "," +
                " idSchoolSubject='" + Lesson.IdSchoolSubject + "'," +
                " idSchoolYear='" + Lesson.IdSchoolYear + "'," +
                " note=" + SqlString(Lesson.Note) +
                " WHERE idLesson=" + Lesson.IdLesson +
                ";";
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
        }
         internal abstract List<Topic> GetTopicsOfOneLessonOfClass(Class Class, Lesson Lesson)
        {
            List<Topic> topics = new();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT Topics.* FROM Lessons" +
                    " LEFT JOIN Lessons_Topics ON Lessons.IdLesson = Lessons_Topics.idLesson" +
                    " LEFT JOIN Topics ON Topics.idTopic = Lessons_Topics.idTopic" +
                    " WHERE idSchoolSubject='" + Lesson.IdSchoolSubject + "'" +
                    " AND Lessons.idSchoolYear='" + Lesson.IdSchoolYear + "'" +
                    " AND Lessons.idClass='" + Class.IdClass + "'" +
                    " AND Lessons.idLesson='" + Lesson.IdLesson + "'" +
                    //" GROUP BY Lessons.idLesson" +
                    " ORDER BY Lessons.date DESC" +
                    ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                Topic t = new();
                while (dRead.Read())
                {
                    t = GetTopicFromRow(dRead);
                    topics.Add(t);
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return topics;
        }
         internal abstract DataTable GetLessonsOfClass(Class Class, Lesson Lesson)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapt;
                DataSet dSet = new DataSet();
                string query = "SELECT * FROM Lessons" +
                    " WHERE idSchoolSubject='" + Lesson.IdSchoolSubject + "'" +
                    " AND Lessons.idSchoolYear='" + Lesson.IdSchoolYear + "'" +
                    " AND Lessons.idClass='" + Class.IdClass + "'" +
                    //" GROUP BY Lessons.idLesson" +
                    " ORDER BY Lessons.date DESC" +
                    ";";
                dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                dSet = new DataSet("GetLessonsOfClass");
                dAdapt.Fill(dSet);
                t = dSet.Tables[0];

                dAdapt.Dispose();
                dSet.Dispose();
            }
            return t;
        }
         internal abstract List<Lesson> GetLessonsOfClass(Class Class, string IdSchoolSubject,
            bool OrderByAscendingDate)
        {
            List<Lesson> lessons = new List<Lesson>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                Lesson l;
                string query = "SELECT * FROM Lessons" +
                    " WHERE idSchoolSubject='" + IdSchoolSubject + "'" +
                    " AND Lessons.idClass='" + Class.IdClass + "'" +
                    " ORDER BY Lessons.date";
                if (OrderByAscendingDate)
                    query += " ASC";
                else
                    query += " DESC";
                query += ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                l = new Lesson();
                while (dRead.Read())
                {
                    l = GetLessonFromRow(dRead);
                    lessons.Add(l);
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return lessons;
        }
         internal abstract Lesson GetLastLesson(Lesson CurrentLesson)
        {
            using (DbConnection conn = Connect())
            {
                List<Couple> couples = new List<Couple>();
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                Lesson l;
                string query;
                query = "SELECT * FROM Lessons" +
                        " WHERE idClass=" + CurrentLesson.IdClass.ToString() +
                        " AND idSchoolSubject='" + CurrentLesson.IdSchoolSubject + "'" +
                        " AND idSchoolYear='" + CurrentLesson.IdSchoolYear + "'" +
                        " ORDER BY Date DESC LIMIT 1;";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                l = new Lesson();
                while (dRead.Read())
                {
                    l = GetLessonFromRow(dRead);
                    break;
                }
                cmd.Dispose();
                dRead.Dispose();
                return l;
            }
        }
         internal abstract Lesson GetLessonInDate(Class Class, string IdSubject,
            DateTime Date)
        {
            Lesson l = new Lesson();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query;
                query = "SELECT * FROM Lessons" +
                        " WHERE idClass=" + Class.IdClass.ToString() +
                        " AND idSchoolSubject='" + IdSubject + "'" +
                        " AND date BETWEEN " + SqlDate(Date.ToString("yyyy-MM-dd")) +
                        " AND " + SqlDate(Date.AddDays(1).ToString("yyyy-MM-dd")) +
                        " LIMIT 1;";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    l = GetLessonFromRow(dRead);
                    break; // there should be only one record in the query result 
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return l;
        }
         internal abstract void EraseLesson(int? IdLesson, bool AlsoEraseImageFiles)
        {
            using (DbConnection conn = Connect())
            {
                // erase existing links topic-lesson
                DbCommand cmd = conn.CreateCommand();
                string query = "DELETE FROM Lessons_Topics" +
                        " WHERE idLesson=" + IdLesson.ToString() +
                        ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                // erase images' files if permitted
                if (AlsoEraseImageFiles)
                {
                    // !! TODO !! find the images that aren't linked to another lesson and delete
                    // the files if EraseImageFiles is set 
                    throw new NotImplementedException();
                }

                // erase existing links images-lesson
                cmd = conn.CreateCommand();
                query = "DELETE FROM Lessons_Images" +
                        " WHERE idLesson=" + IdLesson.ToString() +
                        ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                // erase the lesson's row from lessons
                cmd = conn.CreateCommand();
                query = "DELETE FROM Lessons" +
                        " WHERE idLesson=" + IdLesson.ToString() +
                        ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract List<Topic> GetTopicsOfLesson(int? IdLesson)
        {
            List<Topic> topicsOfTheLesson = new List<Topic>();
            if (IdLesson == null)
            {
                return null;
            }
            // order by ensures that the order of the result is the order of insertion 
            // in the database (that was the same of the tree traversal) 
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                DbDataReader dRead;
                string query;
                query = "SELECT * FROM Topics" +
                        " JOIN Lessons_Topics ON Topics.idTopic=Lessons_Topics.idTopic" +
                        " WHERE Lessons_Topics.idLesson=" + IdLesson +
                        " ORDER BY insertionOrder" +
                        ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    topicsOfTheLesson.Add(t);
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return topicsOfTheLesson;
        }
         internal abstract void SaveTopicsOfLesson(int? IdLesson, List<Topic> topicsOfTheLesson)
        {
            using (DbConnection conn = Connect())
            {
                // erase existing links topic-lesson
                DbCommand cmd = conn.CreateCommand();
                string query = "DELETE FROM Lessons_Topics" +
                        " WHERE idLesson=" + IdLesson.ToString() +
                        ";";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                int insertionOrder = 1;
                foreach (Topic t in topicsOfTheLesson)
                {
                    // insert links topic-lesson, one at a time 
                    cmd.CommandText = "INSERT INTO Lessons_Topics" +
                    " (idLesson, idTopic, insertionOrder)" +
                    " Values (" +
                    "" + IdLesson + "" +
                    "," + t.Id +
                    "," + insertionOrder++ +
                    ");";
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }

                    catch (Exception e)
                    {
                        {
                            // if it isn't UNIQUE, then it is error (to cure!!!!)
                            // but actually it doesn't matter too much, because
                            // the lesson is already linked to the topic anyway.. 


                        }
                    }
                }
                cmd.Dispose();
            }
        }
         internal abstract List<Image> GetLessonsImagesList(Lesson Lesson)
        {
            if (Lesson.IdLesson == null)
                return null;

            List<Image> imagesOfTheLesson = new List<Image>();

            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                DbDataReader dRead;
                string query;
                query = "SELECT * FROM Images" +
                        " JOIN Lessons_Images ON Images.idImage=Lessons_Images.idImage" +
                        " WHERE Lessons_Images.idLesson=" + Lesson.IdLesson +
                        ";";
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Image i = new Image();
                    i.IdImage = (int)dRead["IdImage"];
                    i.Caption = (string)dRead["Caption"];
                    i.RelativePathAndFilename = (string)dRead["ImagePath"];

                    imagesOfTheLesson.Add(i);
                }
                cmd.Dispose();
                dRead.Dispose();
            }
            return imagesOfTheLesson;
        }
        /// <summary>
        /// Creates a new Image in Images and links it to the lesson
        /// If the image has an id != 0, it exists and is not created 
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="Lesson"></param>
        /// <returns></returns>
         internal abstract int? LinkOneImageToLesson(Image Image, Lesson Lesson)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query;
                if (Image.IdImage == 0)
                {
                    Image.IdImage = NextKey("Images", "IdImage");
                    query = "INSERT INTO Images" +
                    " (idImage, imagePath, caption)" +
                    " Values (" + Image.IdImage + "," +
                    SqlString(Image.RelativePathAndFilename) + "," +
                    SqlString(Image.Caption) + "" +
                    ");";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    query = "UPDATE Images" +
                        " SET " +
                        " imagePath=" + SqlString(Image.RelativePathAndFilename) + "," +
                        " caption=" + SqlString(Image.Caption) + "" +
                        " WHERE idImage=" + Image.IdImage +
                        ";";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                query = "INSERT INTO Lessons_Images" +
                    " (idImage, idLesson)" +
                    " Values (" + Image.IdImage + "," + Lesson.IdLesson + "" +
                    ");";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            return Image.IdImage;
        }
         internal abstract List<Topic> GetTopicsDoneInClassInPeriod(Class Class,
            SchoolSubject Subject,
            DateTime? DateStart, DateTime? DateFinish)
        {
            // node order according to Modified Preorder Tree Traversal algorithm
            List<Topic> l = new List<Topic>();
            using (DbConnection conn = Connect())
            {
                // find topics that are done in a lesson of given class about and given subject 
                //DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Topics" +
                    " JOIN Lessons_Topics ON Lessons_Topics.idTopic = Topics.idTopic " +
                    " JOIN Lessons ON Lessons_Topics.idLesson = Lessons.idLesson" +
                    " JOIN Classes ON Classes.idClass = Lessons.idClass" +
                    " WHERE Lessons.idClass = " + Class.IdClass +
                    " AND Lessons.idSchoolSubject ='" + Subject.IdSchoolSubject + "'";
                if (DateStart != null && DateFinish != null)
                    query += " AND Lessons.date BETWEEN " +
                    SqlDate(DateStart) + " AND " + SqlDate(DateFinish);
                query += " ORDER BY Lessons.date ASC;";
                DbCommand cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract void UpdatePathStartLinkOfClass(Class currentClass, string text)
        {
            // !!!! currently not used, because pathStartLink field does not exist yet in the database !!!!
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE Classes" +
                           " Set" +
                           " pathStartLink='" + text + "'" +
                           " WHERE IdClass=" + currentClass.IdClass +
                           ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void AddLinkToPreviousYearPhoto(int? IdStudent, string IdPreviousSchoolYear, string IdNextSchoolYear)
        {
            using (DbConnection conn = Connect())
            {
                // get the code of the previous photo
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idStudentsPhoto" +
                    " FROM StudentsPhotos_Students" +
                    " WHERE idSchoolYear='" + IdPreviousSchoolYear + "'" +
                    " AND StudentsPhotos_Students.idStudent = " + IdStudent + "; ";
                int? idStudentsPhoto = (int?)cmd.ExecuteScalar();
                if (idStudentsPhoto != null)
                {
                    // add link to old photo
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO StudentsPhotos_Students " +
                    "(idStudent, idStudentsPhoto, idSchoolYear) " +
                    "Values (" +
                    "" + IdStudent + "" +
                    "," + idStudentsPhoto + "" +
                    ",'" + IdNextSchoolYear + "'" +
                    ");";
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
            }
        }
         internal abstract int LinkOnePhoto(Student Student, Class Class, string RelativePathAndFilePhoto)
        {
            // find the key for next photo
            int keyPhoto = NextKey("StudentsPhotos", "idStudentsPhoto");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // add the relative path of the photo to the StudentsPhotos table
                cmd.CommandText = "INSERT INTO StudentsPhotos " +
                "(idStudentsPhoto, photoPath)" +
                "Values " +
                "('" + keyPhoto + "'," + SqlString(RelativePathAndFilePhoto) +
                ");";
                cmd.ExecuteNonQuery();

                // erase all possible links of old photos from the StudentsPhotos_Students table
                cmd.CommandText = "DELETE FROM StudentsPhotos_Students " +
                    "WHERE idStudent=" + Student.IdStudent +
                    " AND idSchoolYear='" + Class.SchoolYear + "'" +
                    ";";
                cmd.ExecuteNonQuery();
                // add this photo to the StudentsPhotos_Students table 
                cmd.CommandText = "INSERT INTO StudentsPhotos_Students " +
                    "(idStudentsPhoto, idStudent, idSchoolYear) " +
                    "Values (" + keyPhoto + "," + Student.IdStudent + "," + SqlString(Class.SchoolYear) +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return keyPhoto;
        }
         internal abstract int? SaveStartLink(int? IdStartLink, int? IdClass, string SchoolYear,
            string StartLink, string Desc)
        {
            try
            {
                using (DbConnection conn = Connect())
                {
                    DbCommand cmd = null;
                    cmd = conn.CreateCommand();
                    if (IdStartLink != null && IdStartLink != 0)
                    {
                        cmd.CommandText = "UPDATE Classes_StartLinks" +
                            " SET" +
                            " idClass=" + IdClass + "" +
                            ",startLink=" + SqlString(StartLink) + "" +
                            ",desc=" + SqlString(Desc) + "" +
                            " WHERE idStartLink=" + IdStartLink +
                            ";";
                    }
                    else
                    {
                        IdStartLink = NextKey("Classes_StartLinks", "IdStartLink");
                        cmd.CommandText = "INSERT INTO Classes_StartLinks" +
                            " (idStartLink,idClass,startLink,desc)" +
                            " VALUES " +
                            "(" +
                            IdStartLink +
                            "," + IdClass +
                            "," + SqlString(StartLink) + "" +
                            "," + SqlString(Desc) + "" +
                            ");";
                    }
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Commons.ErrorLog("DbLayer.SaveStartLink: " + ex.Message);
                IdStartLink = null;
            }
            return IdStartLink;
        }
         internal abstract void DeleteStartLink(Nullable<int> IdStartLink)
        {
            DbCommand cmd = null;
            try
            {
                using (DbConnection conn = Connect())
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "DELETE FROM Classes_StartLinks" +
                            " WHERE idStartLink=" + IdStartLink +
                            ";";
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Commons.ErrorLog("DbLayer.SaveStartLink: " + ex.Message);
                IdStartLink = null;
                cmd.Dispose();
            }
        }
         internal abstract List<StartLink> GetStartLinksOfClass(Class Class)
        {
            List<StartLink> listOfLinks = new List<StartLink>();
            if (Class == null || Class.IdClass == null)
                return listOfLinks;
            DbDataReader dRead;
            DbCommand cmd;
            try
            {
                using (DbConnection conn = Connect())
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT *" +
                        " FROM Classes_StartLinks" +
                        " WHERE idClass=" + Class.IdClass + "; ";
                    dRead = cmd.ExecuteReader();
                    while (dRead.Read())
                    {
                        StartLink l = new StartLink();
                        l.Link = Safe.String(dRead["startLink"]);
                        l.Desc = Safe.String(dRead["Desc"]);
                        l.IdClass = Safe.Int(dRead["IdClass"]);
                        l.IdStartLink = Safe.Int(dRead["IdStartLink"]);
                        listOfLinks.Add(l);
                    }
                    dRead.Dispose();
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Commons.ErrorLog("DbLayer.GetStartLinksOfClass: " + ex.Message);
            }
            return listOfLinks;
        }
    }
}

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract List<SchoolPeriod> GetSchoolPeriodsOfDate(DateTime Date)
        {
            List<SchoolPeriod> l = new List<SchoolPeriod>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT *" +
                    " FROM SchoolPeriods" +
                    " WHERE " + SqlDate(Date) +
                    " BETWEEN dateStart and dateFinish" +
                    ";";
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    SchoolPeriod p = GetOneSchoolPeriodFromRow(dRead);
                    l.Add(p);
                }
            }
            return l;
        }
         internal abstract List<SchoolPeriod> GetSchoolPeriods(string IdSchoolYear)
        {
            List<SchoolPeriod> l = new List<SchoolPeriod>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT * " +
                    "FROM SchoolPeriods"; 
                if (IdSchoolYear != null)
                {
                    query += 
                    " WHERE idSchoolYear=" + SqlString(IdSchoolYear) +
                    " OR IdSchoolYear IS null OR IdSchoolYear=''" +
                    ";";
                }
                cmd.CommandText = query; 
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    SchoolPeriod p = GetOneSchoolPeriodFromRow(dRead);
                    l.Add(p);
                }
            }
            return l;
        }
         internal abstract SchoolPeriod GetOneSchoolPeriodFromRow(DbDataReader Row)
        {
            SchoolPeriod p = new SchoolPeriod();
            p.IdSchoolPeriodType = Safe.String(Row["idSchoolPeriodType"]);
            if (p.IdSchoolPeriodType != "N")
            {
                p.DateFinish = Safe.DateTime(Row["dateFinish"]);
                p.DateStart = Safe.DateTime(Row["dateStart"]);
            }
            p.Name = Safe.String(Row["name"]);
            p.Desc = Safe.String(Row["desc"]);
            p.IdSchoolPeriod = Safe.String(Row["idSchoolPeriod"]);
            p.IdSchoolYear = Safe.String(Row["idSchoolYear"]);
            return p;
        }
         internal abstract void SaveSchoolPeriod(SchoolPeriod SchoolPeriod)
        {
            if (FindIfIdIsAlreadyExisting(SchoolPeriod.IdSchoolPeriod))
            {
                UpdateSchoolPeriod(SchoolPeriod); 
            }
            else
            {
                CreateSchoolPeriod(SchoolPeriod); 
            }
        }
         internal abstract void CreateSchoolPeriod(SchoolPeriod SchoolPeriod)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "INSERT INTO SchoolPeriods" +
                "(idSchoolPeriod, idSchoolPeriodType, dateStart, dateFinish, " +
                " name, desc, idSchoolYear)"; 
                query += " Values(";
                query += "" + SqlString(SchoolPeriod.IdSchoolPeriod);
                query += "," + SqlString(SchoolPeriod.IdSchoolPeriodType);
                query += "," + SqlDate(SchoolPeriod.DateStart);
                query += "," + SqlDate(SchoolPeriod.DateFinish);
                query += "," + SqlString(SchoolPeriod.Name);
                query += "," + SqlString(SchoolPeriod.Desc);
                query += "," + SqlString(SchoolPeriod.IdSchoolYear);
                query += ");";

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return;
        }
         internal abstract void UpdateSchoolPeriod(SchoolPeriod SchoolPeriod)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "UPDATE SchoolPeriods" +
                " SET" +
                //" idSchoolPeriod=" + SqlString(SchoolPeriod.IdSchoolPeriod) + "," +
                " idSchoolPeriodType=" + SqlString(SchoolPeriod.IdSchoolPeriodType) + "," +
                " dateFinish=" + SqlDate(SchoolPeriod.DateFinish) + "," +
                " dateStart=" + SqlDate(SchoolPeriod.DateStart) + "," +
                " name=" + SqlString(SchoolPeriod.Name) + "," +
                " desc=" + SqlString(SchoolPeriod.Desc) + "," +
                " idSchoolYear=" + SqlString(SchoolPeriod.IdSchoolYear) +
                " WHERE idSchoolPeriod=" + SqlString(SchoolPeriod.IdSchoolPeriod) +
                ";";

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void DeleteSchoolPeriod(string IdSchoolPeriod)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "DELETE FROM SchoolPeriods" +
                " WHERE idSchoolPeriod=" + SqlString(IdSchoolPeriod) +
                ";";

                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract bool FindIfPeriodsAreAlreadyExisting(string SchoolYear)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idSchoolPeriod FROM SchoolPeriods" +
                    " WHERE idSchoolPeriod LIKE '%" + SchoolYear + "%'";
                var onlyColumn = cmd.ExecuteScalar();
                cmd.Dispose();
                return onlyColumn != null;
                //return onlyColumn != DBNull.Value;
            }
        }
         internal abstract bool FindIfIdIsAlreadyExisting(string IdSchoolPeriod)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idSchoolPeriod FROM SchoolPeriods" +
                    " WHERE idSchoolPeriod='" + IdSchoolPeriod + "'";
                var onlyColumn = cmd.ExecuteScalar();
                cmd.Dispose();
                return onlyColumn != null;
            }
        }
         internal abstract List<SchoolPeriodType> GetSchoolPeriodTypes()
        {
            List<SchoolPeriodType> l = new List<SchoolPeriodType>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT *" +
                    " FROM SchoolPeriodTypes" +
                    ";";
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    SchoolPeriodType p = new SchoolPeriodType();
                    p.IdSchoolPeriodType = Safe.String(dRead["idSchoolPeriodType"]);
                    p.Desc = Safe.String(dRead["desc"]);
                    l.Add(p);
                }
            }
            return l;
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
        private abstract string MakeStringForFilteredQuestionsQuery(List<Tag> Tags, string IdSchoolSubject,
            string IdQuestionType, Topic QuestionsTopic, bool QueryManyTopics, bool TagsAnd)
        {
            string query = "SELECT DISTINCT Questions.idQuestion" +
                            " FROM Questions";
            if (Tags != null && Tags.Count > 0)
            {
                // must join info from table Questions_Tags
                string queryTags = " JOIN Questions_Tags ON Questions.idQuestion=Questions_Tags.idQuestion";

                // make an IN clause, useful for both queries
                string InClause = " WHERE Questions_Tags.idTag IN(";
                foreach (Tag tag in Tags)
                {
                    InClause += "" + tag.IdTag.ToString() + ",";
                }
                InClause = InClause.Substring(0, InClause.Length - 1);
                InClause += ")";

                // (se http://howto.philippkeller.com/2005/04/24/Tags-Database-schemas/, "Toxi" solution)
                if (!TagsAnd)
                {
                    // The tags are evaluated in Union (OR) 
                    // limits the query only to those questions that have been associated to at least one of the tags in the list
                    queryTags += InClause;
                }
                else
                {
                    // The tags are in intersection (AND) 
                    queryTags += InClause;
                    queryTags += " GROUP BY Questions.idQuestion";
                    queryTags += " HAVING COUNT(Questions.idQuestion)=" + Tags.Count;
                }
                query += queryTags;
            }
            if (IdSchoolSubject != null && IdSchoolSubject != "")
            {
                // if we have already added the SQL for tags, we don't need a where
                if (query.Contains("WHERE"))
                    query += " AND idSchoolSubject=" + SqlString(IdSchoolSubject);
                else
                    query += " WHERE idSchoolSubject=" + SqlString(IdSchoolSubject);
            }
            if (IdQuestionType != null && IdQuestionType != "")
            {
                if (query.Contains("WHERE"))
                    query += " AND idQuestionType=" + SqlString(IdQuestionType);
                else
                    query += " WHERE idQuestionType=" + SqlString(IdQuestionType);
            }
            if (QuestionsTopic != null && QuestionsTopic.Id != null)
            {
                if (!QueryManyTopics)
                {
                    // just one topic
                    if (query.Contains("WHERE"))
                        query += " AND idTopic=" + QuestionsTopic.Id + "";
                    else
                        query += " WHERE idTopic=" + QuestionsTopic.Id + "";
                }
                else
                {
                    // manu topics: all those that stay under the node passed 
                    string queryApplicableTopics = "SELECT idTopic FROM Topics" +
                        " WHERE Topics.leftNode BETWEEN " + QuestionsTopic.LeftNodeOld +
                        " AND " + QuestionsTopic.RightNodeOld;
                    // query the passed Topic, plus all its descendants in the tree
                    if (query.Contains("WHERE"))
                        query += " AND Questions.IdTopic IN (" + queryApplicableTopics + ")";
                    else
                        query += " WHERE Questions.IdTopic IN (" + queryApplicableTopics + ")";
                }
            }
            return query;
        }
         internal abstract List<Question> GetAllQuestionsOfATest(int? IdTest)
        {
            List<Question> lq = new List<Question>();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT *" +
                    " FROM Questions" +
                    " JOIN Tests_Questions ON Tests_Questions.IdQuestion=Questions.IdQuestion" +
                    " WHERE Tests_Questions.idTest=" + IdTest +
                    ";";
                DbDataReader dRead;
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    Question q = GetQuestionFromRow(dRead);
                    lq.Add(q);
                }
            }
            return lq;
        }
         internal abstract void FixQuestionInGrade(int? IdGrade)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE Grades" +
                           " Set" +
                           " isFixed=TRUE" +
                           " WHERE idGrade=" + IdGrade +
                           ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void RemoveQuestionFromTest(int? IdQuestion, int? IdTest)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Tests_Questions " +
                    "WHERE IdQuestion=" + IdQuestion +
                    " AND IdTest=" + IdTest +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
        private abstract Question GetQuestionFromRow(DbDataReader Row)
        {
            Question q = new Question();
            q.Difficulty = Safe.Int(Row["Difficulty"]);
            q.IdImage = Safe.Int(Row["IdImage"]);
            q.Duration = Safe.Int(Row["Duration"]);
            q.IdQuestion = Safe.Int(Row["IdQuestion"]);
            q.IdQuestionType = Safe.String(Row["IdQuestionType"]);
            q.IdSchoolSubject = Safe.String(Row["IdSchoolSubject"]);
            //q.IdSubject = Safe.SafeInt(Row["IdSubject"]);
            q.IdTopic = Safe.Int(Row["IdTopic"]);
            q.Image = Safe.String(Row["Image"]);
            q.Text = Safe.String(Row["Text"]);
            q.Weight = Safe.Double(Row["Weight"]);
            q.NRows = Safe.Int(Row["nRows"]);
            q.IsParamount = Safe.Int(Row["isParamount"]);
            ////////q.IsFixed = Safe.SafeBool(Row["isFixed"]);

            return q;
        }
         internal abstract Question GetQuestionById(int? IdQuestion)
        {
            Question question = new Question();
            if (IdQuestion == 0)
            {
                question.IdQuestion = 0;
                question.Text = "";
                return question;
            }
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * " +
                    "FROM Questions " +
                    "WHERE idQuestion=" + IdQuestion.ToString() +
                    ";";
                dRead = cmd.ExecuteReader();
                while (dRead.Read()) // we cycle even if we need just a row, to check if it exists
                {
                    question.IdQuestion = (int)dRead["idQuestion"];
                    question.IdQuestionType = dRead["idQuestionType"].ToString();
                    question.Text = dRead["text"].ToString();
                    question.QuestionImage = dRead["image"].ToString();
                    question.Difficulty = (int)dRead["difficulty"];
                    question.Duration = (int)dRead["duration"];
                    question.IdSchoolSubject = dRead["idSchoolSubject"].ToString();
                    question.IdTopic = (int)dRead["idTopic"];
                    question.Image = dRead["image"].ToString();
                    question.Weight = (double)dRead["weight"];
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return question;
        }
         internal abstract void SaveQuestion(Question Question)
        {
            using (DbConnection conn = Connect())
            {
                string imageNoHome = Question.Image;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Questions " +
                    "SET idQuestionType=" + SqlString(Question.IdQuestionType) + " " +
                     ", idSchoolSubject=" + SqlString(Question.IdSchoolSubject) + " " +
                     //", idSubject=" + Question.IdSubject + " " +
                     ", idSchoolSubject='" + Question.IdSchoolSubject + "'" +
                     ", idTopic=" + Question.IdTopic + " " +
                     ", duration=" + Question.Duration + " " +
                     ", difficulty=" + Question.Difficulty + " " +
                     ", text=" + SqlString(Question.Text) + " " +
                     ", image=" + SqlString(imageNoHome) + " " +
                     ", weight=" + SqlDouble(Question.Weight.ToString()) + " " +
                    "WHERE idQuestion=" + Question.IdQuestion +
                    ";";
                cmd.ExecuteNonQuery();
                // !!!!TODO sistemare le risposte
                // !!!!TODO gestire i tag
                cmd.Dispose();
            }
        }
         internal abstract void AddQuestionToTest(SchoolGrades.BusinessObjects.SchoolTest Test, Question Question)
        {
            using (DbConnection conn = Connect())
            {
                // get the code of the previous photo
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Tests_Questions" +
                    " (IdTest, IdQuestion)" +
                    " Values" +
                    " (" + Test.IdTest + "," + Question.IdQuestion + ")" +
                    "; ";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract List<QuestionType> GetListQuestionTypes(bool IncludeANullObject)
        {
            List<QuestionType> l = new List<QuestionType>();
            if (IncludeANullObject)
            {
                QuestionType qt = new QuestionType();
                qt.IdQuestionType = "";
                l.Add(qt);
            }
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM QuestionTypes;";
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    QuestionType o = new QuestionType();
                    o.Name = (string)dRead["Name"];
                    o.IdQuestionType = (string)dRead["IdQuestionType"];
                    o.Desc = (string)dRead["Desc"];
                    o.IdQuestionType = (string)dRead["IdQuestionType"];
                    l.Add(o);
                }
                dRead.Dispose();
                cmd.Dispose();
                return l;
            }
        }
         internal abstract Question CreateNewVoidQuestion()
        {
            // trova una chiave da assegnare alla nuova domanda
            Question q = new Question();
            q.IdQuestion = NextKey("Questions", "idQuestion");
            using (DbConnection conn = Connect())
            {
                string imageSenzaHome = q.Image;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Questions " +
                    "(idQuestion) " +
                    "Values (" + q.IdQuestion + ")" +
                     ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            SaveQuestion(q);
            return q;
        }

        /// <summary>
        /// gets the questions regarding the topics taught to the class that 
        /// haven't been made to the student yet. 
        /// Includes also the questions tha do not have a topic 
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="Student"></param>
        /// <param name="Subject"></param>
        /// <returns></returns>
         internal abstract List<Question> GetFilteredQuestionsNotAsked(Student Student, Class Class,
            SchoolSubject Subject, string IdQuestionType, List<Tag> Tags, Topic Topic,
            bool QueryManyTopics, bool TagsAnd, string SearchString,
            DateTime DateFrom, DateTime DateTo)
        {
            List<Question> lq = new List<Question>();
            string filteredQuestions;

            // first part of the query: selection of the interesting fields in Questions
            string query = "SELECT Questions.IdQuestion,Questions.text,Questions.idSchoolSubject,Questions.idQuestionType" +
                ",Questions.weight,Questions.duration,Questions.difficulty,Questions.image,Questions.idTopic" +
                " FROM Questions";
            // add the WHERE clauses
            // if the search string is present, then it must be in the searched field 
            if (SearchString != "")
            {
                query += " WHERE Questions.text " + SqlLikeStatement(SearchString) + "";
            }

            filteredQuestions = MakeStringForFilteredQuestionsQuery(Tags, Subject.IdSchoolSubject, IdQuestionType,
                    Topic, QueryManyTopics, TagsAnd);

            // !!!! IF we don't want to make the same question to the student the next part SHOULD BE FIXED !!!!
            string questionsAlreadyMade = "";
            if (Student != null)
            {
                questionsAlreadyMade = "SELECT Questions.idQuestion" +
                    " FROM Questions" +
                    " JOIN Grades ON Questions.idQuestion=Grades.idQuestion" +
                    " JOIN Students ON Students.idStudent=Grades.IdStudent" +
                    " WHERE Students.idStudent=" + Student.IdStudent +
                    " AND Grades.idSchoolYear='" + Class.SchoolYear + "'";
            }
            string questionsTopicsMade = "";
            if (Class != null && Subject != null)
            {
                // questions made to the class 
                questionsTopicsMade = "SELECT Questions.idQuestion" +
                    " FROM Questions" +
                    " JOIN Lessons_Topics ON Questions.idTopic=Lessons_Topics.idTopic" +
                    " JOIN Lessons ON Lessons_Topics.idLesson=Lessons.idLesson" +
                    " JOIN Classes ON Classes.idClass=Lessons.idClass" +
                    " WHERE Classes.idClass=" + Class.IdClass +
                    " AND (Questions.idSchoolSubject='" + Subject.IdSchoolSubject + "'" +
                    " OR Questions.idSchoolSubject='' OR Questions.idSchoolSubject=NULL)";
                //////////////if (DateFrom != Commons.DateNull)
                //////////////    questionsTopicsMade += " AND (Lessons.Date BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) + ")";
                // PART of the final query that extracts the Ids of the questions already made 
                questionsTopicsMade = " Questions.idQuestion IN(" + questionsTopicsMade + ")";
            }
            if (questionsAlreadyMade != "")
            {
                // take only questions already made 
                if (SearchString == "")
                {
                    query += " WHERE Questions.idQuestion NOT IN(" + questionsAlreadyMade + ")";
                }
                else
                {
                    query += " AND Questions.idQuestion NOT IN(" + questionsAlreadyMade + ")";
                }
            }
            if (filteredQuestions != "")
            {
                if (query.Contains("WHERE"))
                {
                    query += " AND";
                }
                else
                {
                    query += " WHERE";
                }
                query += " Questions.idQuestion IN(" + filteredQuestions + ")";
            }
            query += " OR Questions.idTopic IS NULL OR Questions.idTopic = ''";
            //if (SearchString != "")
            //    query += ")";

            query += " ORDER BY Questions.weight;";

            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();

                cmd.CommandText = query;

                dRead = cmd.ExecuteReader();
                while (dRead.Read()) // 
                {
                    Question questionForList = new Question();

                    questionForList.Difficulty = (int)dRead["difficulty"];
                    questionForList.Duration = (int)dRead["duration"];
                    questionForList.IdQuestion = (int)dRead["idQuestion"];
                    questionForList.IdQuestionType = dRead["idQuestionType"].ToString();
                    questionForList.IdSchoolSubject = dRead["idSchoolSubject"].ToString();
                    //q.idSubject = (int)dRead["idSubject"];
                    questionForList.IdTopic = (int)dRead["idTopic"];
                    questionForList.Image = dRead["image"].ToString();
                    questionForList.QuestionImage = dRead["image"].ToString();
                    questionForList.Text = dRead["text"].ToString();
                    questionForList.Weight = (double)dRead["weight"];

                    lq.Add(questionForList);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return lq;
        }
         internal abstract List<Question> GetFilteredQuestionsAskedToClass(Class Class, SchoolSubject Subject, string IdQuestionType,
            List<Tag> Tags, Topic Topic, bool QueryManyTopics, bool TagsAnd,
            string SearchString, DateTime DateFrom, DateTime DateTo)
        {
            List<Question> lq = new List<Question>();
            string filteredQuestions;

            // first part of the query: selection of the interesting fields in Questions
            string query = "SELECT Questions.IdQuestion,Questions.text,Questions.idSchoolSubject,Questions.idQuestionType" +
                ",Questions.weight,Questions.duration,Questions.difficulty,Questions.image,Questions.idTopic" +
                " FROM Questions";
            // add the WHERE clauses
            // if the search string is present, then it must be in the searched field 
            if (SearchString != "")
            {
                query += " WHERE Questions.text " + SqlLikeStatement(SearchString) + "";
            }
            if (Subject != null)
                filteredQuestions = MakeStringForFilteredQuestionsQuery(Tags, Subject.IdSchoolSubject, IdQuestionType,
                    Topic, QueryManyTopics, TagsAnd);
            else
                filteredQuestions = MakeStringForFilteredQuestionsQuery(Tags, "", IdQuestionType,
                    Topic, QueryManyTopics, TagsAnd);
            // !!!! THIS PART MUST BE FIXED !!!!
            string questionsAlreadyMade = "";
            //if (Student != null)
            //{
            //    questionsAlreadyMade = "SELECT Questions.idQuestion" +
            //        " FROM Questions" +
            //        " JOIN Grades ON Questions.idQuestion=Grades.idQuestion" +
            //        " JOIN Students ON Students.idStudent=Grades.IdStudent" +
            //        " WHERE Students.idStudent=" + Student.IdStudent +
            //        " AND Grades.idSchoolYear='" + Class.SchoolYear + "'";
            //}
            //string questionsTopicsMade = "";
            //if (Class != null && Subject != null)
            //{
            //    // questions made to the class in every time ever 
            //    questionsTopicsMade = "SELECT Questions.idQuestion" +
            //        " FROM Questions" +
            //        " JOIN Lessons_Topics ON Questions.idTopic=Lessons_Topics.idTopic" +
            //        " JOIN Lessons ON Lessons_Topics.idLesson=Lessons.idLesson" +
            //        " JOIN Classes ON Classes.idClass=Lessons.idClass" +
            //        " WHERE Classes.idClass=" + Class.IdClass +
            //        " AND (Questions.idSchoolSubject='" + Subject.IdSchoolSubject + "'" +
            //        " OR Questions.idSchoolSubject='' OR Questions.idSchoolSubject=NULL)";
            //    if (DateFrom != Commons.DateNull)
            //        questionsTopicsMade += " AND (Lessons.Date BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) + ")";
            //    // PART of the final query that extracts the Ids of the questions already made 
            //    questionsTopicsMade = " Questions.idQuestion IN(" + questionsTopicsMade + ")";
            //}

            //if (questionsAlreadyMade != "")
            //{
            //    // take only questions already made 
            //    if (SearchString == "")
            //    {
            //        query += " WHERE Questions.idQuestion NOT IN(" + questionsAlreadyMade + ")";
            //    }
            //    else
            //    {
            //        query += " Questions.idQuestion NOT IN(" + questionsAlreadyMade + ")";
            //    }
            //}
            //if (filteredQuestions != "")
            //{
            //    if (questionsAlreadyMade != "" || SearchString != "")
            //    {
            //        query += " AND Questions.idQuestion IN(" + filteredQuestions + ")";
            //    }
            //    else
            //    {
            //        query += " WHERE Questions.idQuestion IN(" + filteredQuestions + ")";
            //    }
            //}
            //query += " OR Questions.idTopic IS NULL OR Questions.idTopic = ''";
            //if (SearchString != "")
            //    query += ")";

            query += " ORDER BY Questions.weight;";

            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();

                cmd.CommandText = query;

                dRead = cmd.ExecuteReader();
                while (dRead.Read()) // 
                {
                    Question questionForList = new Question();

                    questionForList.Difficulty = (int)dRead["difficulty"];
                    questionForList.Duration = (int)dRead["duration"];
                    questionForList.IdQuestion = (int)dRead["idQuestion"];
                    questionForList.IdQuestionType = dRead["idQuestionType"].ToString();
                    questionForList.IdSchoolSubject = dRead["idSchoolSubject"].ToString();
                    //q.idSubject = (int)dRead["idSubject"];
                    questionForList.IdTopic = (int)dRead["idTopic"];
                    questionForList.Image = dRead["image"].ToString();
                    questionForList.QuestionImage = dRead["image"].ToString();
                    questionForList.Text = dRead["text"].ToString();
                    questionForList.Weight = (double)dRead["weight"];

                    lq.Add(questionForList);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return lq;
        }
    }
}
using System;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
        #region functions that prepare the value of a variable to be used in a SQL statement 
         internal abstract string SqlString(string String)
        {
            if (String == null) return "null";
            string temp;
            if (!(String == null))
            {
                temp = String;

                temp = temp.Replace("'", "''");
            }
            else
                temp = "";
            temp = "'" + temp + "'";
            return temp;
        }
         internal abstract string SqlString(string String, int MaxLenght)
        {
            if (String == null) return "null";
            string temp;
            if (!(String == null))
            {
                temp = String;

                temp = temp.Replace("'", "''");
            }
            else
                temp = "";
            if (MaxLenght > 0 && temp.Length > MaxLenght)
                temp = temp.Substring(0, MaxLenght);
            temp = "'" + temp + "'";
            return temp;
        }
         internal abstract string SqlLikeStatement(string SearchText)
        {
            if (SearchText == null) return "null";
            string temp = SearchText;
            temp = temp.Replace("'", "''");
            temp = "LIKE '%" + temp + "%'";
            return temp;
        }
         internal abstract string SqlLikeStatementWithOptions(string FieldName, string SearchText,
            bool SearchWholeWord = false, bool SearchVerbatimString = false)
        {
            if (SearchText == null) return "null";
            SearchText = SearchText.Replace("'", "''");
            string statement;

            if (SearchVerbatimString)
            {
                statement = FieldName + "='" + SearchText + "'";
                return statement;
            }
            if (SearchWholeWord)
            {   // search words separated by " " with all the possibilities
                statement = FieldName + " LIKE '" + SearchText + " %'"; // word at the beginning 
                statement += " OR " + FieldName + " LIKE '% " + SearchText + "'"; // word at the end 
                statement += " OR " + FieldName + " LIKE '% " + SearchText + " %'"; // word in the middle 
                statement += " OR " + FieldName + " = '" + SearchText + "'"; // word as the whole string 
            }
            else
                // search with any substring also in the middle of the "word" searched  
                statement = FieldName + " LIKE '%" + SearchText + "%'";
            return statement;
        }
        public string SqlBool(object Value)
        {
            if (Value == null)
                return "null";
            if ((bool)Value == false)
            {
                return "0";
            }
            else
            {
                return "1";
            }
        }
         internal abstract string SqlDouble(string Number)
        {
            try
            {
                if (Number != null)
                    return double.Parse(Number).ToString().Replace(",", ".");
                else
                    return "null";
            }
            catch
            {
                return "null";
            }
        }
         internal abstract string SqlDouble(object Number)
        {
            if (Number == null)
                return "null";
            // restituisce null se dà errore, perchè viene usato con double? 
            try
            {
                return Number.ToString().Replace(",", ".");
            }
            catch
            {
                return "null";
            }
        }
         internal abstract string SqlFloat(float Number)
        {
            try
            {
                return Number.ToString().Replace(",", ".");
            }
            catch
            {
                return "null";
            }
        }
         internal abstract string SqlFloat(string Number)
        {
            try
            {
                return float.Parse(Number).ToString().Replace(",", ".");
            }
            catch
            {
                return "null";
            }
        }
         internal abstract string SqlInt(string Number)
        {
            try
            {
                if (Number != null)
                    return int.Parse(Number).ToString();
                else
                    return "null";
            }
            catch
            {
                return "null";
            }
        }
         internal abstract string SqlInt(int? Number)
        {
            if (Number == null) return "null";
            try
            {
                return Number.ToString();
            }
            catch
            {
                return "null";
            }
        }
         internal abstract string CleanStringForQuery(string Query)
        {
            // pulisce la stringa dalle andate a capo e dai tab 
            Query = Query.Replace("\t", " ");
            Query = Query.Replace("\r\n", " ");
            Query = Query.Replace("  ", " ");
            Query = Query.Replace("  ", " ");

            while (Query.Contains("  "))
                Query = Query.Replace("  ", " ");
            return Query;
        }
        public string SqlDate(string Date)
        {
            if (Date is null)
                return "null";
            if (Date == "")
                return "null";

            DateTime? d = System.DateTime.Parse(Date);
            return ("datetime('" + ((DateTime)d).ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':') + "')");
        }

        public string SqlDate(DateTime? Date)
        {
            if (Date != null)
                return ("datetime('" + ((DateTime)Date).ToString("yyyy-MM-dd HH:mm:ss").Replace('.', ':') + "')");
            else
                return "null";
        }
    }
    #endregion
}
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;

namespace SchoolGrades
{
	public abstract partial class DataLayer
	{
		string creationScript = @"
CREATE TABLE 'Answers' (
	'idAnswer'	INT NOT NULL,
	'idQuestion'	INT NOT NULL,
	'showingOrder'	INT,
	'text'	VARCHAR(255),
	'errorCost'	INT,
	'isCorrect'	TINYINT,
	'isOpenAnswer'	TINYINT,
	'isMutex'	TINYINT,
	PRIMARY KEY('idAnswer')
);

CREATE TABLE 'Answers_Questions' (
  'idAnswer' INT NOT NULL,
  'idQuestion' INT NOT NULL,
  PRIMARY KEY('idAnswer', 'idQuestion
);

CREATE TABLE 'Classes' (
	'idClass'	INT NOT NULL,
	'idSchoolYear'	VARCHAR( 4 ) NOT NULL,
	'idSchool'	VARCHAR( 15 ) NOT NULL,
	'abbreviation'	VARCHAR( 8 ),
	'desc'	VARCHAR( 255 ),
	'uriWebApp'	VARCHAR( 255 ),
	'pathRestrictedApplication'	VARCHAR( 255 ),
	PRIMARY KEY('idClass')
); 

CREATE TABLE 'Classes_SchoolSubjects' (
  'idClass' VARCHAR(8) NOT NULL,
  'idSchoolSubject' VARCHAR(6) NOT NULL,
  PRIMARY KEY('idClass', 'idSchoolSubject')
);

CREATE TABLE 'Classes_StartLinks' (
	'idStartLink'	INT NOT NULL,
	'idClass'	INT NOT NULL,
	'startLink'	VARCHAR(255),
	'desc'	VARCHAR(45),
	PRIMARY KEY('idStartLink')
);

CREATE TABLE 'Classes_Students' (
  'idClass' INT NOT NULL,
  'idStudent' INT NOT NULL,
  'registerNumber' INT NULL,
  PRIMARY KEY('idClass', 'idStudent')
);

CREATE TABLE 'Classes_Tests' (
	'idClass'	INT NOT NULL,
	'idTest'	INT NOT NULL,
	'timeAllowed'	INT,
	'dateGiven'	DATE,
	'dateGraded'	DATE,
	PRIMARY KEY('idClass','idTest')
);

CREATE TABLE 'Flags' (
	'areLeftRightConsistent'	INT
);

CREATE TABLE 'GradeCategories' (
  'idGradeCategory' VARCHAR(5) NOT NULL,
  'name' VARCHAR(20) NOT NULL,
  'desc' VARCHAR(255) NULL,
  PRIMARY KEY('idGradeCategory')
);

CREATE TABLE 'GradeTypes' (
  'idGradeType' VARCHAR(5) NOT NULL,
  'idGradeCategory' VARCHAR(5) NULL,
  'name' VARCHAR(20) NOT NULL,
  'desc' VARCHAR(255) NULL,
  'defaultWeight' FLOAT NULL,
  'programsCode' INT NULL,
  'idGradeTypeParent' VARCHAR(5) NULL,
  PRIMARY KEY('idGradeType')
);

CREATE TABLE 'Grades' (
	'idGrade'	INT NOT NULL,
	'idStudent'	INT NOT NULL,
	'value'	FLOAT,
	'idSchoolSubject'	VARCHAR(6),
	'weight'	FLOAT,
	'cncFactor'	FLOAT,
	'idSchoolYear'	VARCHAR(4) NOT NULL,
	'timestamp'	DATETIME,
	'idGradeType'	VARCHAR(5) NOT NULL,
	'idGradeParent'	INT,
	'idQuestion'	INT,
	'isFixed'	TINYINT,
	PRIMARY KEY('idGrade')
);

CREATE TABLE 'Images' (
	'IdImage'	INT NOT NULL,
	'imagePath'	VARCHAR( 255 ),
	'caption'	VARCHAR( 45 ),
	PRIMARY KEY('IdImage')
);

CREATE TABLE 'Lessons' (
	'idLesson'	INT NOT NULL,
	'date'	DATETIME,
	'idClass'	INT NOT NULL,
	'idSchoolSubject'	VARCHAR(6) NOT NULL,
	'note'	VARCHAR(45),
	'idSchoolYear'	VARCHAR(4),
	PRIMARY KEY('idLesson')
);

CREATE TABLE 'Lessons_Images' (
	'idLesson'	INT,
	'idImage'	INT
);

CREATE TABLE 'Lessons_Topics' (
	'idLesson'	INT NOT NULL,
	'idTopic'	INT NOT NULL,
	'insertionOrder'	INT,
	PRIMARY KEY('idLesson','idTopic')
);

CREATE TABLE 'QuestionTypes' (
  'idQuestionType' VARCHAR(5) NOT NULL,
  'name' VARCHAR(20) NOT NULL,
  'desc' VARCHAR(255) NULL,
  PRIMARY KEY('idQuestionType')
);

CREATE TABLE 'Questions' (
	'idQuestion'	INT NOT NULL,
	'text'	VARCHAR( 255 ),
	'weight'	FLOAT,
	'duration'	INT,
	'difficulty'	INT,
	'idImage'	INT,
	'image'	VARCHAR( 90 ),
	'idQuestionType'	VARCHAR( 5 ),
	'idTopic'	INT,
	'idSubject'	INT,
	'idSchoolSubject'	VARCHAR( 6 ),
	'nRows'	INT,
	'isParamount'	INT,
	PRIMARY KEY('idQuestion')
); 

CREATE TABLE 'Questions_Tags' (
  'idQuestion' INT NOT NULL,
  'idTag' INT NOT NULL,
  PRIMARY KEY('idQuestion', 'idTag')
);

CREATE TABLE 'ReminderTypes' (
  'idReminderType' VARCHAR(5) NOT NULL,
  'name' VARCHAR(20) NOT NULL,
  'desc' VARCHAR(255) NULL,
  PRIMARY KEY('idReminderType')
);

CREATE TABLE 'Reminders' (
  'idReminder' INT NOT NULL,
  'idReminderType' VARCHAR(5) NOT NULL,
  'description' VARCHAR(128) NULL,
  'idForeignKey' INT NOT NULL,
  PRIMARY KEY('idReminder')
); 

CREATE TABLE 'SchoolPeriodTypes' (
	'idSchoolPeriodType'	VARCHAR( 3 ),
	'desc'	varchar(45),
	PRIMARY KEY('idSchoolPeriodType')
); 

CREATE TABLE 'SchoolPeriods' (
	'idSchoolPeriod'	VARCHAR( 8 ) NOT NULL,
	'idSchoolPeriodType'	VARCHAR( 3 ),
	'dateStart'	DATE,
	'dateFinish'	DATE,
	'name'	VARCHAR ( 20 ) NOT NULL,
	'desc'	VARCHAR ( 255 ),
	'idSchoolYear'	VARCHAR ( 4 ) NOT NULL,
	PRIMARY KEY('idSchoolPeriod')
);

CREATE TABLE 'SchoolSubjects' (
	'idSchoolSubject'	VARCHAR(6) NOT NULL,
	'name'	VARCHAR(20) NOT NULL,
	'desc'	VARCHAR(255),
	'color'	INT,
	'orderOfVisualization'	INT,
	PRIMARY KEY('idSchoolSubject')
); 

CREATE TABLE 'SchoolYears' (
  'idSchoolYear' VARCHAR(4) NOT NULL,
  'shortDesc' VARCHAR(10) NULL,
  'notes' VARCHAR(255) NULL,
  PRIMARY KEY ('idSchoolYear')
);

CREATE TABLE 'Schools' (
  'idSchool' VARCHAR(15) NOT NULL,
  'name' VARCHAR(80) NULL,
  'desc' VARCHAR(255) NULL,
  'officialSchoolAbbreviation' VARCHAR(10) NULL,
  PRIMARY KEY ('idSchool')
); 

CREATE TABLE 'Students' (
	'idStudent'	INT NOT NULL,
	'lastName'	VARCHAR(45),
	'firstName'	VARCHAR(45),
	'residence'	VARCHAR(45),
	'origin'	VARCHAR(45),
	'email'	VARCHAR(45),
	'drawable'	int,
	'birthDate'	DATE,
	'birthPlace'	VARCHAR(45),
	'VFCounter'	INTEGER,
	'disabled'	int,
	'hasSpecialNeeds'	INTEGER,
	PRIMARY KEY('idStudent')
);

CREATE TABLE 'StudentsAnnotations' (
	'idAnnotation'	INT NOT NULL,
	'idStudent'	INTEGER,
	'annotation'	VARCHAR(256),
	'idSchoolYear'	VARCHAR(4),
	'instantTaken'	DATETIME,
	'instantClosed'	DATETIME,
	'isActive'	INTEGER,
	'isPopUp'	INTEGER,
	PRIMARY KEY('idAnnotation')
); 

CREATE TABLE 'StudentsAnswers' (
	'idStudentsAnswer'	INT NOT NULL,
	'idStudent'	INT,
	'idAnswer'	INT,
	'studentsBoolAnswer'	TINYINT,
	'studentsTextAnswer'	VARCHAR(255),
	'idTest'	INTEGER,
	PRIMARY KEY('idStudentsAnswer')
); 

CREATE TABLE 'StudentsPhotos' (
  'idStudentsPhoto' INT NOT NULL,
  'photoPath' VARCHAR(255) NULL,
  PRIMARY KEY ('idStudentsPhoto')
);

CREATE TABLE 'StudentsPhotos_Students' (
  'idStudentsPhoto' INT NOT NULL,
  'idStudent' INT NOT NULL,
  'idSchoolYear' VARCHAR(4) NOT NULL,
  PRIMARY KEY ('idStudentsPhoto', 'idStudent', 'idSchoolYear')
);

CREATE TABLE 'StudentsQuestions' (
  'idStudentsQuestion' INT NOT NULL,
  'idStudent' INT NOT NULL,
  'idQuestion' INT NULL,
  'grade' FLOAT NULL,
  'timestamp' DATETIME NULL,
  PRIMARY KEY ('idStudentsQuestion')
); 

CREATE TABLE 'StudentsTests' (
  'idStudentsTest' INT NOT NULL,
  'idStudent' INT NOT NULL,
  'idTest' INT NOT NULL,
  'grade' FLOAT NULL,
  PRIMARY KEY ('idStudentsTest')
); 

CREATE TABLE 'StudentsTests_StudentsPhotos' (
  'idStudentsTest' INT NOT NULL,
  'idStudentsPhoto' INT NOT NULL,
  PRIMARY KEY ('idStudentsTest', 'idStudentsPhoto')
); 

CREATE TABLE 'Students_Annotations' (
	'idStudent'	INTEGER,
	'idAnnotation'	INTEGER
);

CREATE TABLE 'Students_GradeTypes' (
  'idStudent' INT NOT NULL,
  'idGradeType' INT NOT NULL,
  PRIMARY KEY ('idStudent', 'idGradeType')
); 

CREATE TABLE 'Subjects' (
	'idSubject'	INT NOT NULL,
	'name'	VARCHAR(20) NOT NULL,
	'desc'	VARCHAR(255),
	'leftNode'	INT,
	'rightNode'	INT,
	PRIMARY KEY('idSubject')
); 

CREATE TABLE 'Tags' (
  'idTag' INT NOT NULL,
  'tag' VARCHAR(20) NOT NULL,
  'desc' VARCHAR(255) NULL,
  PRIMARY KEY ('idTag')
); 

CREATE TABLE 'TestTypes' (
  'idTestType' VARCHAR(6) NOT NULL,
  'name' VARCHAR(20) NOT NULL,
  'desc' VARCHAR(255) NULL,
  PRIMARY KEY ('idTestType')
);

CREATE TABLE 'Tests' (
	'idTest'	INT NOT NULL,
	'name'	VARCHAR(20),
	'desc'	VARCHAR(255),
	'idSubject'	INT,
	'idSchoolSubject'	VARCHAR(6),
	'idTopic'	INT,
	'idTestType'	VARCHAR(6),
	PRIMARY KEY('idTest')
); 

CREATE TABLE 'Tests_Questions' (
	'idTest'	INT NOT NULL,
	'idQuestion'	INT NOT NULL,
	'weight'	REAL,
	PRIMARY KEY('idTest','idQuestion')
); 

CREATE TABLE 'Tests_Tags' (
  'idTest' INT NOT NULL,
  'idTag' INT NOT NULL,
  PRIMARY KEY ('idTest', 'idTag')
); 

CREATE TABLE 'Topics' (
	'idTopic'	INT NOT NULL,
	'name'	VARCHAR(20) NOT NULL,
	'desc'	VARCHAR(255),
	'leftNode'	INT,
	'rightNode'	INT,
	'parentNode'	INT,
	'childNumber'	INT,
	PRIMARY KEY('idTopic')
); 

CREATE TABLE 'Users' (
	'username'	VARCHAR(16) NOT NULL,
	'description'	VARCHAR(64),
	'lastName'	VARCHAR(45),
	'firstName'	VARCHAR(45),
	'email'	VARCHAR(255),
	'password'	VARCHAR(32) NOT NULL,
	'lastChange'	TIMESTAMP,
	'lastPasswordChange'	TIMESTAMP,
	'creationTime'	TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	'salt'	VARCHAR(32),
	'idUserCategory'	INT,
	'isEnabled'	TINYINT,
	PRIMARY KEY('username')
); 

CREATE TABLE 'UsersCategories' (
	'idUserCategory'	INT NOT NULL,
	'name'	VARCHAR(20) NOT NULL,
	'desc'	VARCHAR(255),
	PRIMARY KEY('idUserCategory')
);";

		private abstract void CreateNewDatabase(string dbName)
		{
			// making new, means erasing existent! 
			if (File.Exists(dbName))
				File.Delete(dbName);

			// when the file does not exist 
			// Microsoft.Data.Sqlite creates the file at first connection
			DbConnection c = Connect();
			c.Close();
			c.Dispose();

			try
			{
				using (DbConnection conn = Connect())
				{
					DbCommand cmd = conn.CreateCommand();

					cmd.CommandText = creationScript;
					cmd.ExecuteNonQuery();
					conn.Close(); 
					cmd.Dispose();
				}
				// !!!! TODO fill the tables of enumerations
			}
			catch (Exception ex)
			{
				//Common.LogOfProgram.Error("Sqlite_DataAndGeneral | CreateNewDatabase", ex);
			}
		}

	}
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract Student CreateStudentFromStringMatrix(string[,] StudentData, int? StudentRow)
        {
            // look if exists a student with same name, last name, birth date and place
            Student s = new Student();
            s.RegisterNumber = StudentData[(int)StudentRow, 0];
            s.LastName = StudentData[(int)StudentRow, 1];
            s.FirstName = StudentData[(int)StudentRow, 2];
            s.BirthDate = Safe.DateTime(StudentData[(int)StudentRow, 3]);
            s.Residence = StudentData[(int)StudentRow, 4];
            s.Origin = StudentData[(int)StudentRow, 5];
            s.Email = StudentData[(int)StudentRow, 6];
            s.BirthPlace = StudentData[(int)StudentRow, 7];
            s.Eligible = false;

            Student existingStudent = GetStudent(s);
            if (existingStudent == null)
            {
                // not found an existing student: find a key for the new student
                s.IdStudent = NextKey("Students", "idStudent");
                CreateStudent(s);
            }
            else
            {
                // student already exists, uses old data in the fields from the file that are empty
                // LastName, FirstName, BirthDate and BirthPlace are equal! 
                s.IdStudent = existingStudent.IdStudent;
                if (s.Residence == "") s.Residence = existingStudent.Residence;
                if (s.Origin == "") s.Origin = existingStudent.Origin;
                if (s.Email == "") s.Email = existingStudent.Email;
                if (s.RegisterNumber == "") s.RegisterNumber = existingStudent.RegisterNumber;
                s.Origin = StudentData[(int)StudentRow, 5];
                s.Email = StudentData[(int)StudentRow, 6];
                s.Eligible = false;
                UpdateStudent(s);
            }
            return s;
        }
        private abstract Student GetStudent(Student StudentToFind)
        {
            Student s;
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT *" +
                    " FROM Students" +
                    " WHERE lastName=" + SqlString(StudentToFind.LastName) +
                    " AND firstName=" + SqlString(StudentToFind.FirstName) +
                    " AND (birthDate=" + SqlDate(StudentToFind.BirthDate) + " OR birthDate=NULL)" +
                    //" AND (birthPlace=" + SqlDate(StudentToFind.BirthPlace) + " OR birthPlace=NULL)" +
                    ";";
                dRead = cmd.ExecuteReader();
                dRead.Read();
                if (dRead.HasRows)
                    s = GetStudentFromRow(dRead);
                else
                    s = null;
                dRead.Dispose();
                cmd.Dispose();
            }
            return s;
        }
         internal abstract DataTable GetStudentsWithNoMicrogrades(Class Class, string IdGradeType, string IdSchoolSubject,
            DateTime DateFrom, DateTime DateTo)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                // find the macro grade type of the micro grade
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idGradeTypeParent " +
                    "FROM GradeTypes " +
                    "WHERE idGradeType='" + IdGradeType + "'; ";
                string idGradeTypeParent = (string)cmd.ExecuteScalar();

                string query = "SELECT Students.idStudent, LastName, FirstName, disabled FROM Students" +
                                    " JOIN Classes_Students ON Students.idStudent=Classes_Students.idStudent" +
                                    " WHERE Students.idStudent NOT IN" +
                                    "(";
                query += "SELECT DISTINCT Students.idStudent" +
                " FROM Classes_Students" +
                " LEFT JOIN Grades ON Students.idStudent=Grades.idStudent" +
                " JOIN Students ON Classes_Students.idStudent=Students.idStudent" +
                " WHERE Classes_Students.idClass =" + Class.IdClass +
                " AND (Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                " OR Grades.idSchoolYear='" + Class.SchoolYear.Replace("-", "") + "'" + // TEMPORARY: delete after 
                ")" +
                " AND (Grades.idGradeType='" + IdGradeType + "'" +
                " OR Grades.idGradeType IS NULL)" +
                " AND Grades.idSchoolSubject='" + IdSchoolSubject + "'" +
                " AND Grades.value IS NOT NULL AND Grades.value <> 0" +
                " AND Grades.Timestamp BETWEEN " + SqlDate(DateFrom) + " AND " + SqlDate(DateTo) +
                ")" +
                " AND NOT Students.disabled";
                query += " AND Classes_Students.idClass=" + Class.IdClass;
                query += ";";
                DataAdapter DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DataSet DSet = new DataSet("ClosedMicroGrades");

                DAdapt.Fill(DSet);
                t = DSet.Tables[0];

                DAdapt.Dispose();
                DSet.Dispose();
            }
            return t;
        }
         internal abstract List<Student> GetAllStudentsThatAnsweredToATest(SchoolTest Test, Class Class)
        {
            List<Student> list = new List<Student>();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT DISTINCT StudentsAnswers.IdStudent" +
                    " FROM StudentsAnswers" +
                    " JOIN Classes_Students ON StudentsAnswers.IdStudent=Classes_Students.IdStudent" +
                    " JOIN Students ON Classes_Students.IdStudent=Students.IdStudent" +
                    " WHERE StudentsAnswers.IdTest=" + Test.IdTest + "" +
                    " AND Classes_Students.IdClass=" + Class.IdClass + "" +
                    " ORDER BY Students.LastName, Students.FirstName, Students.IdStudent " +
                    ";";
                cmd.CommandText = query;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    int? idStudent = Safe.Int(dRead["idStudent"]);
                    Student s = GetStudent(idStudent);
                    list.Add(s);
                }
            }
            return list;
        }
         internal abstract int? SaveStudent(Student Student)
        {
            if (Student.IdStudent != null)
                return UpdateStudent(Student);
            else
                return CreateStudent(Student);
        }
         internal abstract int? CreateStudent(Student Student)
        {
            // trova una chiave da assegnare al nuovo studente
            int idStudent = NextKey("Students", "idStudent");
            Student.IdStudent = idStudent;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Students " +
                    "(idStudent,lastName,firstName,residence,origin," +
                    "email,birthDate,birthPlace,disabled,hasSpecialNeeds) " +
                    "VALUES (" + SqlInt(Student.IdStudent) + "," +
                    SqlString(Student.LastName) + "," +
                    SqlString(Student.FirstName) + "," +
                    SqlString(Student.Residence) + "," +
                    SqlString(Student.Origin) + "," +
                    SqlString(Student.Email) + "," +
                    SqlDate(Student.BirthDate.ToString()) + "," +
                    SqlString(Student.BirthPlace) + "," +
                    "false," +
                    "false" +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return idStudent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Student">The student we want to update</param>
        /// <param name="conn">Optional connection that is used if present</param>
         internal abstract int? UpdateStudent(Student Student, DbCommand cmd = null)
        {
            DbConnection conn;
            bool leaveConnectionOpen = true;
            if (cmd == null)
            {
                conn = Connect();
                cmd = conn.CreateCommand();
                leaveConnectionOpen = false;
            }
            cmd.CommandText = "UPDATE Students " +
                "SET" +
                " idStudent=" + Student.IdStudent +
                ",lastName=" + SqlString(Student.LastName) +
                ",firstName=" + SqlString(Student.FirstName) +
                ",residence=" + SqlString(Student.Residence) +
                ",birthDate=" + SqlDate(Student.BirthDate.ToString()) + "" +
                ",email=" + SqlString(Student.Email) +
                //",schoolyear=" + SqlString(Student.SchoolYear) + 
                ",origin=" + SqlString(Student.Origin) +
                ",birthPlace=" + SqlString(Student.BirthPlace) +
                ",drawable=" + SqlBool(Student.Eligible) + "" +
                ",disabled=" + SqlBool(Student.Disabled) + "" +
                ",hasSpecialNeeds=" + SqlBool(Student.HasSpecialNeeds) + "" +
                ",VFCounter=" + SqlInt(Student.RevengeFactorCounter) + "" +
                " WHERE idStudent=" + Student.IdStudent +
                ";";
            cmd.ExecuteNonQuery();
            if (Student.RegisterNumber != null && Student.RegisterNumber != "")
            {
                cmd.CommandText = "UPDATE Classes_Students" +
                    " SET" +
                    " registerNumber=" + Student.RegisterNumber +
                    " WHERE idStudent=" + Student.IdStudent +
                    " AND idClass=" + Student.IdClass;
                cmd.ExecuteNonQuery();
            }
            if (!leaveConnectionOpen)
            {
                cmd.Dispose();
                //conn.Close();
                //conn.Dispose();
            }
            return Student.IdStudent;
        }
        // internal abstract void SaveStudentsOfList(List<Student> studentsList, DbConnection conn)
        //{
        //    foreach (Student s in studentsList)
        //    {
        //        SaveStudent(s, conn);
        //    }
        //}
         internal abstract Student GetStudent(int? IdStudent)
        {
            Student s = new Student();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * " +
                    "FROM Students " +
                    "WHERE idStudent=" + IdStudent +
                    ";";
                dRead = cmd.ExecuteReader();
                dRead.Read();
                s = GetStudentFromRow(dRead);
                dRead.Dispose();
                cmd.Dispose();
            }
            return s;
        }
         internal abstract Student GetStudentFromRow(DbDataReader Row)
        {
            Student s = new Student();
            s.IdStudent = (int)Row["IdStudent"];
            s.LastName = Safe.String(Row["LastName"]);
            s.FirstName = Safe.String(Row["FirstName"]);
            s.Residence = Safe.String(Row["Residence"]);
            s.Origin = Safe.String(Row["Origin"]);
            s.Email = Safe.String(Row["Email"]);
            if (Safe.DateTime(Row["birthDate"]) != null)
                s.BirthDate = Safe.DateTime(Row["birthDate"]);
            s.BirthPlace = Safe.String(Row["birthPlace"]);
            s.Eligible = Safe.Bool(Row["drawable"]);
            s.Disabled = Safe.Bool(Row["disabled"]);
            s.HasSpecialNeeds = Safe.Bool(Row["hasSpecialNeeds"]);
            s.RevengeFactorCounter = Safe.Int(Row["VFCounter"]);
            return s;
        }
         internal abstract DataTable GetStudentsSameName(string LastName, string FirstName)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapt;
                DataSet dSet = new DataSet();
                string query = "SELECT Students.IdStudent, Students.lastName, Students.firstName," +
                    " Classes.abbreviation, Classes.idSchoolYear" +
                    " FROM Students" +
                    " LEFT JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent " +
                    " LEFT JOIN Classes ON Classes.idClass = Classes_Students.idClass " +
                    " WHERE Students.lastName " + SqlLikeStatement(LastName) + "" +
                    " AND Students.firstName " + SqlLikeStatement(FirstName) + "" +
                    ";";
                dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                dSet = new DataSet("GetStudentsSameName");
                dAdapt.Fill(dSet);
                t = dSet.Tables[0];

                dSet.Dispose();
                dAdapt.Dispose();
            }
            return t;
        }
         internal abstract DataTable FindStudentsLike(string LastName, string FirstName)
        {
            DataTable t;
            using (DbConnection conn = Connect())
            {
                DataAdapter dAdapt;
                DataSet dSet = new DataSet();
                string query = "SELECT Students.IdStudent, Students.lastName, Students.firstName," +
                    " Classes.abbreviation, Classes.idSchoolYear" +
                    " FROM Students" +
                    " LEFT JOIN Classes_Students ON Students.idStudent = Classes_Students.idStudent " +
                    " LEFT JOIN Classes ON Classes.idClass = Classes_Students.idClass ";
                if (LastName != "" && LastName != null)
                {
                    query += "WHERE Students.lastName " + SqlLikeStatement(LastName) + "";
                    if (FirstName != "" && FirstName != null)
                    {
                        query += " AND Students.firstName " + SqlLikeStatement(FirstName) + "";
                    }
                }
                else
                {
                    if (FirstName != "" && FirstName != null)
                    {
                        query += " WHERE Students.firstName " + SqlLikeStatement(FirstName) + "";
                    }
                }
                query += ";";
                dAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                dSet = new DataSet("GetStudentsSameName");
                dAdapt.Fill(dSet);
                t = dSet.Tables[0];

                dSet.Dispose();
                dAdapt.Dispose();
            }
            return t;
        }
         internal abstract void PutStudentInClass(int? IdStudent, int? IdClass)
        {
            using (DbConnection conn = Connect())
            {
                // add student to the class
                DbCommand cmd = conn.CreateCommand();
                // check if already present
                cmd.CommandText = "SELECT IdStudent FROM Classes_Students " +
                    "WHERE idClass=" + IdClass + " AND idStudent=" + IdStudent + "" +
                ";";
                if (cmd.ExecuteScalar() == null)
                {
                    // add student to the class
                    cmd.CommandText = "INSERT INTO Classes_Students " +
                    "(idClass, idStudent) " +
                    "Values ('" + IdClass + "'," + IdStudent + "" +
                    ");";
                }
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IdClass">Id of the class to be searched</param>
        /// <param name="conn">Connection already open on a database different from standard. 
        /// If not null this connection is left open</param>
        /// <returns>List of the </returns>
         internal abstract List<Student> GetStudentsOfClass(int? IdClass, DbCommand cmd)
        {
            DbConnection conn;
            List<Student> l = new List<Student>();
            bool leaveConnectionOpen = true;
            if (cmd == null)
            {
                conn = Connect();
                cmd = conn.CreateCommand();
                leaveConnectionOpen = false;
            }
            DbDataReader dRead;
            string query = "SELECT Students.*" +
                " FROM Students" +
                " JOIN Classes_Students ON Classes_Students.idStudent=Students.idStudent" +
                " WHERE Classes_Students.idClass=" + IdClass +
            ";";
            cmd.CommandText = query;
            dRead = cmd.ExecuteReader();
            while (dRead.Read())
            {
                Student s = GetStudentFromRow(dRead);
                l.Add(s);
            }
            dRead.Close();
            if (!leaveConnectionOpen)
            {
                cmd.Dispose();
                //conn.Close();
                //conn.Dispose();
            }
            return l;
        }
         internal abstract List<Student> GetStudentsOfClassList(string Scuola, string Anno,
            string SiglaClasse, bool IncludeNonActiveStudents)
        {
            DbDataReader dRead;
            DbCommand cmd;
            List<Student> ls = new List<Student>();
            using (DbConnection conn = Connect())
            {
                string query = "SELECT registerNumber, Classes.idSchoolYear, " +
                               "Classes.abbreviation, Classes.idClass, Classes.idSchool, " +
                               "Students.*" +
                " FROM Students" +
                " JOIN Classes_Students ON Students.idStudent=Classes_Students.idStudent" +
                " JOIN Classes ON Classes.idClass=Classes_Students.idClass" +
                " WHERE Classes.idSchoolYear=" + SqlString(Anno) +
                " AND Classes.abbreviation=" + SqlString(SiglaClasse);
                if (!IncludeNonActiveStudents)
                    query += " AND (Students.disabled = 0 OR Students.disabled IS NULL)";
                if (Scuola != null && Scuola != "")
                    query += " AND Classes.idSchool='" + Scuola + "'";
                query += " ORDER BY Students.LastName, Students.FirstName";
                query += ";";
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();


                while (dRead.Read())
                {
                    Student s = GetStudentFromRow(dRead);
                    s.ClassAbbreviation = (string)dRead["abbreviation"];
                    // read the properties from other tables
                    s.IdClass = (int)dRead["idClass"];
                    s.RegisterNumber = Safe.String(dRead["registerNumber"]);
                    ls.Add(s);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return ls;
        }
         internal abstract List<int> GetIdStudentsNonGraded(Class Class,
            GradeType GradeType, SchoolSubject SchoolSubject)
        {
            List<int> keys = new List<int>();

            DbDataReader dRead;
            DbCommand cmd;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT Classes_Students.idStudent" +
                " FROM Classes_Students" +
                " WHERE Classes_Students.idClass=" + Class.IdClass +
                " AND Classes_Students.idStudent NOT IN" +
                "(" +
                "SELECT DISTINCT Classes_Students.idStudent" +
                " FROM Classes_Students" +
                " LEFT JOIN Grades ON Classes_Students.idStudent = Grades.idStudent" +
                " WHERE Classes_Students.idClass=" + Class.IdClass +
                " AND Grades.idSchoolSubject='" + SchoolSubject.IdSchoolSubject + "'" +
                " AND Grades.idGradeType='" + GradeType.IdGradeType + "'" +
                " AND Grades.idSchoolYear='" + Class.SchoolYear + "'" +
                ")" +
                ";";
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    keys.Add((int)Safe.Int(dRead["idStudent"]));
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return keys;
        }
         internal abstract void ToggleDisabledFlagOneStudent(Student Student)
        {
            // if Disabled is null I want it to be true after method
            if (Student.Disabled == null)
                Student.Disabled = false;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE Students" +
                           " Set" +
                           " disabled = NOT " + Student.Disabled +
                           " WHERE IdStudent =" + Student.IdStudent +
                           ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
        private abstract Nullable<int> GetStudentsPhotoId(int? idStudent, string schoolYear, DbConnection conn)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT idStudentsPhoto FROM StudentsPhotos_Students " +
                "WHERE idStudent=" + idStudent + " AND (idSchoolYear=" + SqlString(schoolYear) + "" +
                " OR idSchoolYear=" + SqlString(schoolYear.Replace("-", "")) + // !!!! TEMPORARY: for compatibility with old database. erase this line in future 
                ")" +
                ";";
            return (int?)cmd.ExecuteScalar();
        }
        private abstract int? StudentHasAnswered(int? IdAnswer, int? IdTest, int? IdStudent)
        {
            int? key;
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT idStudentsAnswer" +
                    " FROM StudentsAnswers" +
                    " WHERE idStudent=" + IdStudent +
                    " AND IdTest=" + IdTest + "" +
                    " AND IdAnswer=" + IdAnswer + "" +
                    ";";
                cmd.CommandText = query;
                //idStudentsAnswer cmd.ExecuteScalar() != null;
                key = (int?)cmd.ExecuteScalar();
            }
            return key;
        }
         internal abstract List<Student> GetStudentsOnBirthday(Class Class, DateTime Date)
        {
            List<Student> list = new List<Student>();
            // strip daytime from date 
            string monthAndYear = Date.Month.ToString("00") + "-" + Date.Day.ToString("00");

            DbDataReader dRead;
            DbCommand cmd;
            using (DbConnection conn = Connect())
            {
                string query = "SELECT * " +
                " FROM Students" +
                " JOIN Classes_Students ON Students.idStudent=Classes_Students.idStudent" +
                " WHERE Classes_Students.idClass=" + Class.IdClass +
                " AND strftime('%m-%d',Students.BirthDate)='" + monthAndYear + "'" +
                ";";
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Student s = GetStudentFromRow(dRead);
                    list.Add(s);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return list;
        }
         internal abstract void SaveStudentsAnswer(Student Student, SchoolTest Test, Answer Answer,
            bool StudentsBoolAnswer, string StudentsTextAnswer)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                // find if an answer has already been given
                int? IdStudentsAnswer = StudentHasAnswered(Answer.IdAnswer, Test.IdTest, Student.IdStudent);
                if (IdStudentsAnswer != null)
                {   // update answer
                    cmd.CommandText = "UPDATE StudentsAnswers" +
                    " SET idStudent=" + SqlInt(Student.IdStudent) + "," +
                    "idAnswer=" + SqlInt(Answer.IdAnswer) + "," +
                    "studentsBoolAnswer=" + SqlBool(StudentsBoolAnswer) + "," +
                    "studentsTextAnswer=" + SqlString(StudentsTextAnswer) + "," +
                    "IdTest=" + SqlInt(Test.IdTest) +
                    "" +
                    " WHERE IdStudentsAnswer=" + Answer.IdAnswer +
                    ";";
                }
                else
                {   // create answer
                    int nextId = NextKey("StudentsAnswers", "IdStudentsAnswer");

                    cmd.CommandText = "INSERT INTO StudentsAnswers " +
                    "(idStudentsAnswer,idStudent,idAnswer,studentsBoolAnswer," +
                    "studentsTextAnswer,IdTest" +
                    ")" +
                    "Values " +
                    "(" + nextId + "," + SqlInt(Student.IdStudent) + "," +
                     SqlInt(Answer.IdAnswer) + "," + SqlBool(StudentsBoolAnswer) + "," +
                     SqlString(StudentsTextAnswer) + "," +
                     SqlInt(Test.IdTest) +
                    ");";
                }
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract void SaveSubjects(List<SchoolSubject> SubjectList)
        {
            foreach (SchoolSubject s in SubjectList)
            {
                SaveSubject(s);
            }
        }
         internal abstract string SaveSubject(SchoolSubject Subject)
        {
            if (Subject.Desc != "" && Subject.Desc != null)
            {
                using (DbConnection conn = Connect())
                {
                    try
                    {
                        DbCommand cmd = conn.CreateCommand();
                        if (Subject.OldId != "" && Subject.OldId != null)
                        {
                            cmd.CommandText = "UPDATE SchoolSubjects " +
                                "SET" +
                                " Name=" + SqlString(Subject.Name) + "" +
                                ",Desc=" + SqlString(Subject.Desc) + "" +
                                ",Color=" + SqlInt(Subject.Color) + "" +
                                ",orderOfVisualization=" + SqlInt(Subject.OrderOfVisualization) + "" +
                                " WHERE idSchoolSubject=" + SqlString(Subject.IdSchoolSubject) + "" +
                                ";";
                        }
                        else
                        {
                            // !! TODO verify that the new code in not already taken !!


                            cmd.CommandText = "INSERT INTO SchoolSubjects " +
                                "(idSchoolSubject, name, desc, color,orderOfVisualization) " +
                                "Values (" + SqlString(Subject.IdSchoolSubject) + "," + SqlString(Subject.Name)
                                + "," + SqlString(Subject.Desc) + "," + SqlInt(Subject.Color) + "" +
                                 "," + SqlInt(Subject.OrderOfVisualization) + "" +
                                ");";
                        }
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            return Subject.IdSchoolSubject;
        }
         internal abstract SchoolSubject GetSchoolSubject(string IdSchoolSubject)
        {
            SchoolSubject subject = new SchoolSubject();
            DbDataReader dRead;
            DbCommand cmd;
            string query;
            using (DbConnection conn = Connect())
            {
                query = "SELECT * FROM SchoolSubjects" +
                    " WHERE IdSchoolSubject='" + IdSchoolSubject + "'";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                dRead = cmd.ExecuteReader();
                while (dRead.Read()) // just the first row 
                {
                    subject.Name = dRead["Name"].ToString();
                    subject.Desc = dRead["Desc"].ToString();
                    subject.Color = Safe.Int(dRead["Color"]);
                    subject.IdSchoolSubject = IdSchoolSubject;
                    subject.OldId = IdSchoolSubject;
                }
            }
            cmd.Dispose();
            dRead.Dispose();
            return subject;
        }
         internal abstract List<SchoolSubject> GetListSchoolSubjects(bool IncludeANullObject)
        {
            List<SchoolSubject> lss = new List<SchoolSubject>();
            if (IncludeANullObject)
            {
                SchoolSubject ss = new SchoolSubject();
                ss.IdSchoolSubject = "";
                lss.Add(ss);
            }

            DbDataReader dRead;
            DbCommand cmd;
            string query;

            using (DbConnection conn = Connect())
            {
                try
                {
                    query = "SELECT * FROM SchoolSubjects" +
                        " ORDER BY orderOfVisualization, name";
                    cmd = new SQLiteCommand(query);
                    cmd.Connection = conn;
                    dRead = cmd.ExecuteReader();
                    while (dRead.Read())
                    {
                        SchoolSubject subject = new SchoolSubject();
                        subject.IdSchoolSubject = dRead["IdSchoolSubject"].ToString();
                        subject.Name = dRead["Name"].ToString();
                        subject.Desc = dRead["Desc"].ToString();
                        subject.Color = Safe.Int(dRead["color"]);
                        subject.OrderOfVisualization = Safe.Int(dRead["orderOfVisualization"]);
                        subject.OldId = subject.IdSchoolSubject; // to check if the user changes IdSchoolSubject

                        lss.Add(subject);
                    }
                }
                catch
                {   // if database is old, dont use orderOfVisualization
                    query = "SELECT * FROM SchoolSubjects;";
                    cmd = new SQLiteCommand(query);
                    cmd.Connection = conn;
                    dRead = cmd.ExecuteReader();
                    while (dRead.Read())
                    {
                        SchoolSubject subject = new SchoolSubject();
                        subject.IdSchoolSubject = dRead["IdSchoolSubject"].ToString();
                        subject.Name = dRead["Name"].ToString();
                        subject.Desc = dRead["Desc"].ToString();
                        subject.Color = Safe.Int(dRead["color"]);
                        subject.OldId = subject.IdSchoolSubject; // to check if the user changes IdSchoolSubject

                        lss.Add(subject);
                    }
                }
            }
            cmd.Dispose();
            dRead.Dispose();
            return lss;
        }
         internal abstract void EraseSchoolSubjectById(string IdSchoolSubject)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM SchoolSubjects" +
                    " WHERE idSchoolSubject=" + SqlString(IdSchoolSubject) +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}


namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract void GetLookupTable(string Table, ref DataSet DSet, ref DataAdapter DAdapt)
        {
            using (DbConnection conn = Connect())
            {
                string query = "SELECT * FROM " + Table + ";";
                DAdapt = new SQLiteDataAdapter(query, (SQLiteConnection)conn);
                DSet = new DataSet("OpenLookupTable");

                DAdapt.Fill(DSet);
                DAdapt.Dispose();
                DSet.Dispose();
            }
        }
         internal abstract void SaveTableOnCsv(DataTable Table, string FileName)
        {
            string fileContent = "";
            foreach (DataColumn col in Table.Columns)
            {
                fileContent += col.Caption + '\t';
            }
            fileContent += "\r\n";
            foreach (DataRow row in Table.Rows)
            {
                foreach (DataColumn col in Table.Columns)
                {
                    fileContent += row[col].ToString() + '\t';
                }
                fileContent += "\r\n";
            }
            TextFile.StringToFile(FileName, fileContent, false);
        }
         internal abstract void CreateLookupTableRow(string Table, string IdTable, DataRow Row)
        {
            // !!!! TODO !!!! GENERALIZZARE A TABELLE CON NOMI DEI CAMPI ARBITRARI E FAR FUNZIONARE !!!!
            string query;
            try
            {
                // if key field is Integer, this works
                int iId = (int)Row[0];
                query = "INSERT INTO " + Table +
                    " (" + IdTable + ", name, desc)" +
                    " VALUES (" + iId + ",'" + Row["name"] + "','" + Row["desc"] + "'" +
                ");";
            }
            catch
            {
                // if key field wasn't Integer, this other will work 
                string sId = (string)Row[0];
                query = "INSERT INTO " + Table +
                    " (" + IdTable + ", name, desc)" +
                    " VALUES ('" + sId + "','" + Row["name"] + "','" + Row["desc"] + "'" +
                ");";
            }
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;

namespace SchoolGrades
{
     internal abstract abstract partial class  DataLayer
    {
         internal abstract List<Tag> GetTagsContaining(string Pattern)
        {
            DbDataReader dRead;
            DbCommand cmd;
            List<Tag> TagList = new List<Tag>();

            using (DbConnection conn = Connect())
            {
                string query = "SELECT *" +
                    " FROM Tags" +
                    " WHERE Tag " + SqlLikeStatement(Pattern) + "" +
                    ";";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Tag t = new Tag();
                    t.IdTag = (int)dRead["IdTag"];
                    t.TagName = (string)dRead["tag"];
                    t.Desc = (string)dRead["Desc"];

                    TagList.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return TagList;
        }

         internal abstract int? CreateNewTag(Tag CurrentTag)
        {
            // trova una chiave da assegnare alla nuova domanda
            CurrentTag.IdTag = NextKey("Tags", "IdTag");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Tags " +
                    "(IdTag, tag, Desc) " +
                    "Values (" + CurrentTag.IdTag + "," +
                    "'" + CurrentTag.TagName + "'," +
                    "'" + CurrentTag.Desc + "'" +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            return CurrentTag.IdTag;
        }

         internal abstract void SaveTag(Tag CurrentTag)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Tags " +
                    " SET IdTag=" + CurrentTag.IdTag + "," +
                    " tag=" + "'" + CurrentTag.TagName + "'," +
                    " Desc=" + "'" + CurrentTag.Desc + "'" +
                    " WHERE idTag=" + CurrentTag.IdTag +
                    ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

         internal abstract List<Tag> TagsOfAQuestion(int? IdQuestion)
        {
            DbDataReader dRead;
            DbCommand cmd;
            List<Tag> l = new List<Tag>();
            using (DbConnection conn = Connect())
            {
                string query = "SELECT * " +
                    " FROM Questions_Tags, Tags" +
                    " WHERE Tags.IdTag = Questions_Tags.IdTag " +
                    " AND Questions_Tags.idQuestion=" + IdQuestion +
                    " ORDER BY Tags.tag;";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Tag t = new Tag();
                    t.Desc = (string)dRead["Desc"];
                    t.IdTag = (int)dRead["IdTag"];
                    t.TagName = (string)dRead["tag"];
                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }

         internal abstract void AddTagToQuestion(int? IdQuestion, int? IdTag)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Questions_Tags " +
                    "(idQuestion, IdTag) " +
                    "Values (" + IdQuestion + "," +
                    IdTag +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System.Collections.Generic;
using System.Data.Common;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract SchoolGrades.BusinessObjects.SchoolTest GetTestFromRow(DbDataReader Row)
        {
            SchoolGrades.BusinessObjects.SchoolTest t = new SchoolTest();
            t.IdTest = Safe.Int(Row["idTest"]);
            t.Name = Safe.String(Row["name"]);
            t.Desc = Safe.String(Row["desc"]);
            t.IdSchoolSubject = Safe.String(Row["IdSchoolSubject"]);
            t.IdTestType = Safe.String(Row["IdTestType"]);
            t.IdSchoolSubject = Safe.String(Row["IdSchoolSubject"]);
            t.IdTopic = Safe.Int(Row["IdTopic"]);

            return t;
        }
         internal abstract SchoolTest GetTest(int? IdTest)
        {
            SchoolTest t = new SchoolTest();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * " +
                    "FROM Tests " +
                    "WHERE IdTest=" + IdTest +
                    ";";
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    t = GetTestFromRow(dRead);
                }
            }
            return t;
        }
         internal abstract List<SchoolTest> GetTests()
        {
            List<SchoolTest> list = new List<SchoolTest>();
            using (DbConnection conn = Connect())
            {
                DbDataReader dRead;
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * " +
                    "FROM Tests " +
                    //"WHERE idSchoolYear=" + IdSchoolYear +
                    //" OR IdSchoolYear IS null OR IdSchoolYear=''" +
                    ";";
                dRead = cmd.ExecuteReader();

                while (dRead.Read())
                {
                    SchoolTest t = GetTestFromRow(dRead);
                    list.Add(t);
                }
            }
            return list;
        }

         internal abstract void SaveTest(SchoolTest TestToSave)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                if (TestToSave.IdTest == 0 || TestToSave.IdTest == null)
                {   // create new record
                    int nextId = NextKey("Tests", "idTest");

                    cmd.CommandText = "INSERT INTO Tests " +
                    "(idTest,name,desc,IdSchoolSubject,IdTestType,IdTopic" +
                    ")" +
                    "Values " +
                    "(" + nextId + "," + SqlString(TestToSave.Name) + "," +
                    SqlString(TestToSave.Desc) + "," + SqlString(TestToSave.IdSchoolSubject) + "," +
                     SqlInt(TestToSave.IdTestType) + "," + SqlInt(TestToSave.IdTopic) +
                    ");";
                }
                else
                {   // update old record
                    cmd.CommandText = "UPDATE Tests" +
                    " SET name=" + SqlString(TestToSave.Name) + "," +
                    "desc=" + SqlString(TestToSave.Desc) + "" +
                    ",IdSchoolSubject=" + SqlString(TestToSave.IdSchoolSubject) +
                    ",IdTestType=" + SqlInt(TestToSave.IdTestType) +
                    ",IdTopic=" + SqlInt(TestToSave.IdTopic) +
                    ")" +
                    " WHERE idTest=" + TestToSave.IdTest +
                    ";";
                }
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
        /// <summary>
        /// Gets the record of the Topic from the database, 
        /// </summary>
        /// <param name="dRead"></param>
        /// <returns></returns>
         internal abstract Topic GetTopicFromRow(DbDataReader dRead)
        {
            Topic t = new Topic();
            t.Id = Safe.Int(dRead["IdTopic"]);
            t.Name = Safe.String(dRead["name"]);
            t.Desc = Safe.String(dRead["desc"]);
            t.LeftNodeOld = Safe.Int(dRead["leftNode"]);
            t.LeftNodeNew = -1;
            t.RightNodeOld = Safe.Int(dRead["rightNode"]);
            t.RightNodeNew = -1;
            t.ParentNodeOld = Safe.Int(dRead["parentNode"]);
            t.ParentNodeNew = -1;
            t.ChildNumberOld = Safe.Int(dRead["childNumber"]);
            t.ChildNumberNew = -1;
            t.Changed = false;

            return t;
        }
         internal abstract int CreateNewTopic(Topic NewTopic)
        {
            int nextId;
            using (DbConnection conn = Connect())
            {
                nextId = NextKey("Topics", "idTopic");

                DbCommand cmd = conn.CreateCommand();
                // aggiunge la foto alle foto (cartella relativa, cui verrà aggiunta la path delle foto)
                cmd.CommandText = "INSERT INTO Topics " +
                "(idTopic,name,desc,leftNode,rightNode,parentNode,childNumber)" +
                "Values " +
                "(" + nextId + "," + SqlString(NewTopic.Name) + "," +
                SqlString(NewTopic.Desc) + "," + SqlInt(NewTopic.LeftNodeNew.ToString()) + "," +
                 SqlInt(NewTopic.RightNodeNew.ToString()) + "," + SqlInt(NewTopic.ParentNodeNew.ToString()) +
                "," + SqlInt(NewTopic.ChildNumberNew.ToString()) +
                ");";
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            return nextId;
        }
         internal abstract void EraseAllTopics()
        {
            using (DbConnection conn = Connect())
            {   // erase all the topics
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Topics;";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract Topic GetTopicById(int? idTopic)
        {
            Topic t = new Topic();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Topics" +
                    " WHERE idTopic=" + idTopic;
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    t = GetTopicFromRow(dRead);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return t;
        }
         internal abstract List<Topic> GetTopics()
        {
            List<Topic> lt = new List<Topic>();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Topics" +
                    " ORDER BY IdTopic;";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    lt.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return lt;
        }
         internal abstract bool IsTopicAlreadyTaught(Topic Topic)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idLesson" +
                    " FROM Lessons_Topics" +
                    " WHERE idTopic=" + Topic.Id +
                    " AND idTopic<>0" +
                    " LIMIT 1; ";
                var result = cmd.ExecuteScalar();
                return (result != null);
            }
        }
         internal abstract List<Topic> GetTopicsDoneFromThisTopic(Class Class, Topic StartTopic,
            SchoolSubject Subject)
        {
            // node numbering according to Modified Preorder Tree Traversal algorithm
            List<Topic> l = new List<Topic>();
            if (Class == null)
                return l;
            using (DbConnection conn = Connect())
            {
                // find descendant topics that are done  
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT DISTINCT Topics.idTopic, Topics.name, Topics.desc" +
                    ",Topics.leftNode, Topics.rightNode, Topics.parentNode, Topics.childNumber" +
                    " FROM Topics" +
                    " JOIN Lessons_Topics ON Lessons_Topics.idTopic = Topics.idTopic" +
                    " JOIN Lessons ON Lessons_Topics.idLesson = Lessons.idLesson" +
                    " WHERE leftNode BETWEEN " + StartTopic.LeftNodeOld +
                    " AND " + StartTopic.RightNodeOld;
                if (Class != null)
                    query += " AND Lessons.idClass = " + Class.IdClass;
                if (Subject != null)
                    query += " AND Lessons.idSchoolSubject ='" + Subject.IdSchoolSubject + "'";
                query += " ORDER BY leftNode ASC;";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
         internal abstract List<Topic> GetTopicsNotDoneFromThisTopic(Class Class, Topic StartTopic,
            SchoolSubject Subject)
        {
            // node numbering according to Modified Preorder Tree Traversal algorithm
            List<Topic> l = new List<Topic>();
            if (Class == null)
                return l;
            using (DbConnection conn = Connect())
            {
                // find descendant topics that aren't done  
                DbCommand cmd = conn.CreateCommand();
                // query that gets the Ids of topics done
                string queryDone = "SELECT DISTINCT Topics.idTopic" +
                    " FROM Topics" +
                    " JOIN Lessons_Topics ON Lessons_Topics.idTopic = Topics.idTopic" +
                    " JOIN Lessons ON Lessons_Topics.idLesson = Lessons.idLesson" +
                    " WHERE leftNode BETWEEN " + StartTopic.LeftNodeOld +
                    " AND " + StartTopic.RightNodeOld;
                if (Class != null)
                    queryDone += " AND Lessons.idClass = " + Class.IdClass;
                if (Subject != null)
                    queryDone += " AND Lessons.idSchoolSubject ='" + Subject.IdSchoolSubject + "'";

                string queryNotDone = "SELECT DISTINCT Topics.idTopic, Topics.name, Topics.desc" +
                    ",Topics.leftNode, Topics.rightNode, Topics.parentNode, Topics.childNumber" +
                    " FROM Topics" +
                    " WHERE leftNode BETWEEN " + StartTopic.LeftNodeOld +
                    " AND " + StartTopic.RightNodeOld +
                    " AND Topics.idTopic NOT IN (" + queryDone + ")";  
                queryNotDone += " ORDER BY leftNode ASC;";
                cmd = new SQLiteCommand(queryNotDone);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
         internal abstract List<Topic> GetAllTopicsDoneInClassAndSubject(Class Class,
            SchoolSubject Subject,
            DateTime DateStart = default(DateTime), DateTime DateFinish = default(DateTime))
        {
            // node order according to Modified Preorder Tree Traversal algorithm
            List<Topic> l = new List<Topic>();
            using (DbConnection conn = Connect())
            {
                // find topics that are done in a lesson of given class about and given subject 
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Topics" +
                    " JOIN Lessons_Topics ON Lessons_Topics.idTopic = Topics.idTopic " +
                    " JOIN Lessons ON Lessons_Topics.idLesson = Lessons.idLesson" +
                    " JOIN Classes ON Classes.idClass = Lessons.idClass" +
                    " WHERE Lessons.idClass = " + Class.IdClass +
                    " AND Lessons.idSchoolSubject ='" + Subject.IdSchoolSubject + "'";
                if (DateStart != default(DateTime) && DateFinish != default(DateTime))
                    query += " AND Lessons.date BETWEEN " +
                    SqlDate(DateStart) + " AND " + SqlDate(DateFinish);
                query += " ORDER BY Lessons.date ASC;";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
         internal abstract List<Topic> GetTopicsDoneInPeriod(Class currentClass, SchoolSubject currentSubject,
            DateTime DateFrom, DateTime DateTo)
        {
            List<Topic> lt = new List<Topic>();
            using (DbConnection conn = Connect())
            {
                DateTo = DateTo.AddDays(1); // add one day for lesson after 0 and to midnight 
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT Topics.idTopic,Topics.name,Topics.desc,Topics.LeftNode,Topics.RightNode," +
                    "Topics.ParentNode, Topics.childNumber, Lessons.date,Lessons.idSchoolSubject" +
                    " FROM Topics" +
                    " JOIN Lessons_Topics ON Lessons_Topics.IdTopic=Topics.IdTopic" +
                    " JOIN Lessons ON Lessons_Topics.IdLesson=Lessons.IdLesson" +
                    " WHERE Lessons.IdClass=" + currentClass.IdClass; 
                if (currentSubject != null && currentSubject.IdSchoolSubject != null && currentSubject.IdSchoolSubject != "")
                    query += " AND Lessons.idSchoolSubject='" + currentSubject.IdSchoolSubject + "'";
                if (DateFrom == Commons.DateNull)
                {
                    query += " AND (Lessons.Date BETWEEN '" + DateFrom.ToString("yyyy-MM-dd") + "'" +
                        " AND '" + DateTo.ToString("yyyy-MM-dd") + "')";
                }
                query += " ORDER BY Lessons.date DESC" +
                ";";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    t.Id = (int)dRead["IdTopic"];
                    t.Name = (string)dRead["name"];
                    t.Desc = Safe.String(dRead["desc"]);
                    //t.LeftNodeNew = -1;
                    //t.RightNodeNew = -1;
                    t.Date = (DateTime)dRead["date"]; // taken fron the Lessons taSafee 

                    // determine the path while still in the database
                    // if we don't, determination from the outside would be too costly 
                    query = "SELECT idTopic, name, desc, leftNode, rightNode" +
                        " FROM Topics" +
                        " WHERE leftNode <=" + t.LeftNodeOld +
                        " AND rightNode >=" + t.RightNodeOld +
                        " ORDER BY leftNode ASC;)";
                    cmd = new SQLiteCommand(query);
                    cmd.Connection = conn;
                    DbDataReader dRead1 = cmd.ExecuteReader();
                    string path = "";
                    while (dRead1.Read())
                    {
                        path += ((string)dRead1["name"]).Trim() + "|";
                    }
                    //t.Path = path;
                    t.Changed = false;
                    lt.Add(t);
                    dRead1.Dispose();
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return lt;
        }
         internal abstract int GetTopicDescendantsNumber(int? LeftNode, int? RightNode)
        {
            // node numbering according to Modified Preorder Tree Traversal algorithm
            return ((int)RightNode - (int)LeftNode - 1) / 2;
        }
         internal abstract void UpdateTopic(Topic t, DbConnection conn)
        {
            bool leaveConnectionOpen = true;
            if (conn == null)
            {
                conn = Connect();
                leaveConnectionOpen = false;
            }
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Topics" +
                " SET" +
                " name=" + SqlString(t.Name) + "" +
                ",desc=" + SqlString(t.Desc) + "" +
                ",parentNode=" + t.ParentNodeNew +
                ",leftNode=" + t.LeftNodeNew +
                ",rightNode=" + t.RightNodeNew +
                ",childNumber=" + t.ChildNumberNew +
                " WHERE idTopic=" + t.Id +
                ";";
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            if (!leaveConnectionOpen)
            {
                conn.Close();
                conn.Dispose();
            }
        }
         internal abstract void InsertTopic(Topic t, DbConnection Conn)
        {
            if (t.Id == null || t.Id == 0)
            {
                bool leaveConnectionOpen = true;
                if (Conn == null)
                {
                    Conn = Connect();
                    leaveConnectionOpen = false;
                }
                DbCommand cmd = Conn.CreateCommand();

                cmd.CommandText = "SELECT MAX(IdTopic) FROM Topics;";
                var temp = cmd.ExecuteScalar();
                if (!(temp is DBNull))
                    t.Id = Convert.ToInt32(temp) + 1;
                cmd.CommandText = "INSERT INTO Topics" +
                    " (idTopic,name,desc,leftNode,rightNode,parentNode,childNumber)" +
                    " Values (" +
                    t.Id.ToString() +
                    "," + SqlString(t.Name) + "" +
                    "," + SqlString(t.Desc) + "" +
                    "," + t.LeftNodeNew + "" +
                    "," + t.RightNodeNew + "" +
                    "," + t.ParentNodeNew + "" +
                    "," + t.ChildNumberNew + "" +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                if (!leaveConnectionOpen)
                {
                    Conn.Close();
                    Conn.Dispose();
                }
            }
        }
         internal abstract List<Topic> GetNodesByParentFromDatabase()
        {
            // node order according to parents' order (parentNode and childNumber)
            List<Topic> l = new List<Topic>();
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Topics" +
                    " ORDER BY parentNode ASC, childNumber ASC;";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    Topic t = GetTopicFromRow(dRead);
                    l.Add(t);
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
         internal abstract void SaveTopicsFromScratch(List<Topic> ListTopics)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Topics;";
                cmd.ExecuteNonQuery();
                int key;
                cmd.CommandText = "SELECT MAX(IdTopic) FROM Topics;";
                var temp = cmd.ExecuteScalar();
                if (temp is DBNull)
                    key = 0;
                else
                    key = (int)temp;
                foreach (Topic t in ListTopics)
                {   // insert new nodes
                    {
                        cmd.CommandText = "INSERT INTO Topics" +
                           " (idTopic,name,desc,parentNode,leftNode,rightNode,parentNode)" +
                           " Values (" +
                           (++key).ToString() +
                            "," + SqlString(t.Name) + "" +
                            "," + SqlString(t.Desc) + "" +
                            "," + t.ParentNodeNew + "" +
                            "," + t.LeftNodeNew + "" +
                            "," + t.RightNodeNew + "" +
                            "," + t.ParentNodeNew + "" +
                            ");";
                        cmd.ExecuteNonQuery();
                    }
                }
                cmd.Dispose();
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract User GetUser(string Username)
        {
            User t = new User(Username, "");
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Users" +
                    " WHERE username='" + Username + "';";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                dRead.Read(); 
                t = GetUserFromRow(dRead);
                dRead.Dispose();
                cmd.Dispose();
            }
            return t;
        }
         internal abstract List<User> GetAllUsers()
        {
            List<User> l = new List<User>(); 
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                string query = "SELECT *" +
                    " FROM Users";
                cmd = new SQLiteCommand(query);
                cmd.Connection = conn;
                DbDataReader dRead = cmd.ExecuteReader();
                while (dRead.Read())
                {
                    User u = GetUserFromRow(dRead);
                    l.Add(u); 
                }
                dRead.Dispose();
                cmd.Dispose();
            }
            return l;
        }
        private abstract User GetUserFromRow(DbDataReader dRead)
        {
            User u = null; 
            if (dRead.HasRows)
            {
                u = new User(Safe.String(dRead["username"]),
                    Safe.String(dRead["password"]));
                u.Description = Safe.String(dRead["description"]);
                u.LastName = Safe.String(dRead["lastName"]);
                u.FirstName = Safe.String(dRead["firstName"]);
                u.Email = Safe.String(dRead["email"]);
                //u.Password = Safe.SafeString(dRead["password"]);
                u.LastChange = Safe.DateTime(dRead["lastChange"]);
                u.LastPasswordChange = Safe.DateTime(dRead["lastPasswordChange"]);
                u.CreationTime = Safe.DateTime(dRead["creationTime"]);
                u.Salt = Safe.String(dRead["salt"]);
                u.IdUserCategory = Safe.Int(dRead["idUserCategory"]);
                u.IsEnabled = Safe.Bool(dRead["isEnabled"]);
            }
            return u;
        }
         internal abstract void ChangePassword(User User)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Users" +
                    " Set" +
                    " password=" + SqlString(User.Password) + "," +
                    " lastPasswordChange=" + SqlDate(DateTime.Now) + "," +
                    " salt=" + SqlString(User.Salt) + "" +
                    " WHERE username='" + User.Username + "'" +
                ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void CreateUser(User User)
        {
            using (DbConnection conn = Connect())
            {
                // check if username is existing. If exists, return null
                DbCommand cmd = conn.CreateCommand();
                // !!!! TODO !!!!

                // create row in table 
                string now = SqlDate(DateTime.Now);
                cmd.CommandText = "INSERT INTO Users " +
                "(username, lastName, firstName, email," +
                "password,creationTime,lastChange,lastPasswordChange,salt,idUserCategory,isEnabled)" +
                "Values " +
                "(" + SqlString(User.Username) + "," + SqlString(User.LastName) + "," + SqlString(User.FirstName) + "," +
                SqlString(User.Email) + "," + SqlString(User.Password) + "," +
                now + "," + now + "," + now + "," + SqlString(User.Salt) + "," +
                User.IdUserCategory + ", TRUE" + 
                ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
         internal abstract void UpdateUser(User User)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Users" +
                    " Set" +
                    " description=" + SqlString(User.Description) + "," +
                    " lastName=" + SqlString(User.LastName) + "," +
                    " firstName=" + SqlString(User.FirstName) + "," +
                    " email=" + SqlString(User.Email) + "," +
                    //" password=" + SqlString(User.Password) + "," +
                    " lastChange=" + SqlDate(DateTime.Now) + "," +
                    //" lastPasswordChange=" + SqlDate(DateTime.Now) + "," +
                    //" creationTime=" + SqlDate(User.CreationTime)  + "," +
                    " salt=" + SqlString(User.Salt) + "," +
                    " isEnabled=" + SqlBool(User.IsEnabled) +
                    " idUserCategory=" + SqlInt(User.IdUserCategory) +
                    " WHERE username='" + User.Username + "'" +
                ";";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}
using SchoolGrades.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SchoolGrades
{
    public abstract partial class DataLayer
    {
         internal abstract bool SchoolYearExists(string idSchoolYear)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT idSchoolYear" +
                    " FROM SchoolYears" +
                    " WHERE idSchoolYear='" + idSchoolYear + "'" + 
                    " LIMIT 1; ";
                var result = cmd.ExecuteScalar();
                return (result != null);
            }
        }
         internal abstract void AddSchoolYear(SchoolYear newSchoolYear)
        {
            using (DbConnection conn = Connect())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO SchoolYears" +
                    " (idSchoolYear,shortDesc,notes)" +
                    " Values (" +
                    SqlString(newSchoolYear.IdSchoolYear) +
                    "," + SqlString(newSchoolYear.ShortDescription) + "" +
                    "," + SqlString(newSchoolYear.Notes) + "" +
                    ");";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }
    }
}
