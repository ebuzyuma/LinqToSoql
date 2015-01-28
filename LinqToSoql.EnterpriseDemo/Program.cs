using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using LinqToSoql.EnterpriseDemo.Sforce;
using LinqToSoql.EnterpriseDemo.SforceEnterpriseService;

namespace LinqToSoql.EnterpriseDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Login to your account
            var sforceLoginManager = new SforceLoginManager();

            ISforceContext context = new SforceContext(sforceLoginManager.SessionHeader, sforceLoginManager.Client);
            
            Query<Student__c> table = context.GetTable<Student__c>();

            double course = 3;
            var query1 = from p in table
                       where p.Birthday__c < DateTime.Today.AddYears(-18)
                       select new { p.Name, p.Email__c, p.Course__c, p.Birthday__c, CreatedBy = p.CreatedBy.Name };

            Console.WriteLine("Age is more then 18:");
            foreach (var student in query1)
            {
                Console.WriteLine("{0,20} {1:d}", student.Name, student.Birthday__c);
            }
            

            IQueryable<Student__c> query2 =
                from p in table
                where p.Course__c >= course
                select new Student__c
                       {
                           Name = p.Name,
                           Email__c = p.Email__c,
                           Course__c = p.Course__c,
                           Birthday__c = p.Birthday__c
                       };

            Console.WriteLine("\nCourse is more or equal then " + course);
            foreach (Student__c student in query2)
            {
                Console.WriteLine("{0,20} {1}", student.Name, student.Course__c);
            }
            Console.ReadKey();
        }
    }
}
