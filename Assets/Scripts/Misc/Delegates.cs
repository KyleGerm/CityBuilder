using Game.Interfaces;

namespace Game.Delegates
{
    public delegate void EntityDelegate();
    public delegate void CompanySubscription(Wallet wallet, EntityDelegate action);
    public delegate void EmployeeDetails(Wallet wallet);
    public delegate bool GameManagerDelegate(int num, out Company company);
    public delegate Business BusinessDelegate(out bool response);
    public delegate void Shopping(ref int resource, int amount = 0);
    public delegate void IntDelegate(int id);
    public delegate void FloatDelegate(float value);
    public delegate void EntityDetails(IEntity entity);
}
