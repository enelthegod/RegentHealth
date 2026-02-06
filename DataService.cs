using System;

public class DataService
{
    //one for everything auto
    public static DataService Instance { get; } = new DataService();

    //data list 
    public List<User> Users { get; set; }
    public List<Appointments>  Appointments { get; set; }

    //private constructor
    private DataService()
    {
        Users = new List<User>();
        Appointments = new List<Appointments>();
    }

}
