using System;

namespace PostAPI
{
    public class PostData
    {
        public int emp_id { get; set; } = 0;

        public string emp_name { get; set; } = "";

        private string _emp_type = "";

        public string emp_dob { get; set; } = "";

        public string emp_doj { get; set; } = "";

        private string _emp_department = "";

        public string emp_type
        {
            get
            {
                return _emp_type;
            }
            set
            {
                if (value == "Fulltime" || value == "Intern" || value == "Contractor")  // To Check for valid values
                    _emp_type = value;
                else
                {
                    Console.WriteLine("Enter Valid Value");
                }
            }
        }

        public string emp_department
        {
            get
            {
                return _emp_department;
            }
            set
            {
                if (value == "Finance" || value == "HR" || value == "IT" || value == "Administration")  // To Check for valid values
                    _emp_department = value;
                else
                {
                    Console.WriteLine("Enter Valid Value");
                }
            }
        }
    }
}
