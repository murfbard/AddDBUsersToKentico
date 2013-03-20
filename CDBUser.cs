namespace PullKentico
{
    class CDBUser
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }

        public CDBUser(string pName, string pEmail, string pPassword)
        {
            Name = pName;
            Email = pEmail;
            Password = pPassword;
        }
    }
}
