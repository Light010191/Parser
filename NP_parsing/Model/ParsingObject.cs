using System;
using System.Collections.Generic;
using System.Text;

namespace NP_parsing.Model
{
    public class ParsingObject
    {
        static int id = 1;
        public int Id { get; set; } 
        public string Adress { get; set; }
        public string NameVacancy { get; set; }
        public string DateOfCreate { get; set; }
        public ParsingObject()
        {
            Id=id++;
        }
    }
}
